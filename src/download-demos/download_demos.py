'''
Downloads a series of demos from pro matches hosted on hltv.org

Hltv hosts demos for a match in .rar format. The rar can
be downloaded from a url like /download/demo/{demo_id}. The DemoScraper
class provides an interface for downloading and extracting demos by
demo_id, and also scrapes hltv.org to find recent demo_ids of 1 star (or more)
pro matches.

Written by Neil Bostian 9/17/2018

Update 9/18/2018 - Hltv appears to have since blacklisted the user-agent for
requests 2.19.1. This can be circumvented by setting a custom user-agent header
for the outgoing requests in this script however at this point in time I have
enough demos downloaded that I won't be making that change. Update at your own
risk.
'''

import os
import re
import sys
import math
import glob
import pyunpack
import requests
from tqdm import tqdm
from bs4 import BeautifulSoup

class DemoScraper():
    '''
    Provides a simpler interface to web scraping and extracting demos from hltv
    '''

    def __init__(self, download_dir: str):
        '''
        @param download_dir: str -> the directory where demos will be downloaded to
        '''

        self.download_dir = os.path.abspath(download_dir)

        if not os.path.exists(self.download_dir):
            os.makedirs(self.download_dir)

    def download_rar(self, demo_id: int):
        '''
        Hltv hosts demos as .rar since a single match can have multiple demos.
        Downloads a .rar for a matchup to download_dir/demo_id.rar

        @param demo_id: int -> ID used in the demo download URL

        @returns rar_download_path: str -> the path where .rar file was saved
        '''
        
        r = requests.get('https://www.hltv.org/download/demo/' + str(demo_id), stream=True)

        # A proper download url should return status 200. Raise status exception otherwise
        if r.status_code != 200:
            r.raise_for_status()

        rar_download_path = os.path.join(self.download_dir, str(demo_id) + '.rar')

        rar_bytes = int(r.headers['Content-Length'])

        block_size = 1024 * 1024

        print(f'{demo_id}.rar:')

        with open(rar_download_path, 'wb') as f:
            for chunk in tqdm(r.iter_content(block_size), total=math.ceil(rar_bytes/block_size), unit='MB', unit_scale=True):
                f.write(chunk)

        return rar_download_path
       
    def extract_rar(self, rar_path: str, extract_dir: str):
        '''
        Extracts the contents of rar at @rar_path
        to the directory @extract_dir. File names are not unique
        per demo_id, so files are first extracted to a temp directory
        then moved and renamed.

        @param rar_path: str -> path to a rar file
        '''

        if not os.path.exists(rar_path):
            raise FileNotFoundError()

        # Create a temporary directory based on rar name. Ex '9999.rar' creates dir '9999',
        # extracts the contents of 9999.rar to 9999 and then moves the contents of 9999 to
        # extract_dir, prefixing file names with '9999'.
        
        print('extracting rar ' + rar_path)

        rar_prefix = os.path.splitext(os.path.basename(rar_path))[0]
        tempdir = os.path.join(os.path.dirname(rar_path), rar_prefix)

        if not os.path.exists(tempdir):
            os.makedirs(tempdir)

        # extract our rar to tempdir
        arch = pyunpack.Archive(rar_path)
        arch.extractall(tempdir)

        # rename and move our extracted files from tempdir to @extract_dir
        for filename in os.listdir(tempdir):
            new_filename = os.path.join(extract_dir, rar_prefix + '-' + os.path.basename(filename))
            os.rename(os.path.join(tempdir, filename), new_filename)

        os.rmdir(tempdir)

    def download_and_extract(self, demo_id: int):
        '''
        Downloads demo_id, extracts the rar to self.download_dir, and deletes the rar.

        @param demo_id: int - demo id from url /download/demo/{demo_id}
        '''

        existing_demos = glob.glob(os.path.join(self.download_dir, f'{demo_id}*.dem'))

        if len(existing_demos) > 0:
            print('Demos already downloaded for demo_id, skipping this id')
            return

        rar_path = self.download_rar(demo_id)
        self.extract_rar(rar_path, self.download_dir)
        os.remove(rar_path)

    def download_recent(self, pages: int=1):
        '''
        Downloads demos from the 100 most recent matchups on hltv.
        These matches can be bo1/2/3/5, so the actual number of
        .dem files will vary.

        https://www.hltv.org/results?content=demo&stars=1
        '''

        for demo_id in self.get_recent_demos(pages=pages):
            self.download_and_extract(demo_id)

    def get_recent_matches(self, offset: int):
        '''
        Iterates hltv.org recent results using the specified offset for this page: https://www.hltv.org/results?offset={offset}&content=demo&stars=1
        '''

        print('getting recent matches with offset ' + str(offset))

        r = requests.get(f'https://www.hltv.org/results?offset={offset}&content=demo&stars=1')

        if r.status_code != 200:
            r.raise_for_status()

        soup = BeautifulSoup(r.text, features='html.parser')

        matches = [x.parent.get('href') for x in soup.find_all('div', 'result')]
        return matches

    def get_demo_id_from_match_path(self, path: str):
        '''
        Returns a demo_id from a given match path (returned by get_recent_matches)

        @param path: str -> Should match the paths returned from get_recent_matches,
        looks something like /matches/2326592/nrg-vs-swole-patrol-epicenter-2018-north-america-closed-qualifier

        @returns demo_id: int -> demo_id which can be used with download_and_extract to retrieve all .dem files associated with the given @path
        '''

        print('obtaining demo id from ' + path)

        if not hasattr(self, '_parse_demo_id_regex'):
            self._parse_demo_id_regex = re.compile('/download/demo/(\d+)')

        r = requests.get('https://www.hltv.org' + path)

        if r.status_code != 200:
            r.raise_for_status()

        match = self._parse_demo_id_regex.search(r.text)

        if match:
            return int(match.group(1))

    def get_recent_demos(self, pages: int=1):
        '''
        Iterates recent demo ids from matches on this page: https://www.hltv.org/results?content=demo&stars=1

        @param pages: int -> The number of recent pages to return demo_ids from. Should be >= 1

        1. Get all match pages from recent results with >= 1 star
        2. Navigate to each match page and search for /download/demo/{demo_id}, yielding demo_ids as found
        '''

        offsets = [x * 100 for x in range(pages)]
        matches = [match for sublist in [self.get_recent_matches(x) for x in offsets] for match in sublist]

        for match_path in matches:
            yield self.get_demo_id_from_match_path(match_path)

if '__main__' == __name__:
    if len(sys.argv) < 2:
        print('Script should be called with a first-argument directory path to download files.')
        print('Usage "python download_demos.py download_dir num_pages"')
        print('\t download_dir - required, path to directory where demo files will be saved')
        print('\t num_pages - optional, specifies the number of pages of recent matches to scrape. Default 1, which returns the 100 most recent matches. Must be >0')
    else:
        if len(sys.argv) > 3:
            print('Warning: More args received than can be used. Run script with no params for usage.')
        
        pages = 1

        if len(sys.argv) > 2:
            pages = int(sys.argv[2])

        scrape = DemoScraper(sys.argv[1])
        scrape.download_recent(pages=pages)

