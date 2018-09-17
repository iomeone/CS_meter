# CS_meter
Win probability meter for Counter-Strike

## Additional Resources

Details of live-stream communcation with running CS observer client can be found here: https://developer.valvesoftware.com/wiki/Counter-Strike:_Global_Offensive_Game_State_Integration

Details of .dem file structure can be found in these two repos which both implement demo parsing and event triggering:
* Valve Official, C++ - https://github.com/ValveSoftware/csgo-demoinfo
* Unofficial, Python - https://github.com/Bakkes/demoinfo-csgo-python



## download_demos.py
This script is used to download .dem files from hltv. Hltv hosts demos for matches in a .rar archive. A series of recent demos can be downloaded using `python download_demos.py {dest_directory}`. Run without arguments for usage.

_Demos are downloaded as .rar files, the following packages are used to download .dem files from hltv and extract them from the .rar archive_
- `conda install requests` _# Responsible for downloading .rar from hltv.org_
- `conda install tqdm` _# Provides an in-console download progress bar_
- `pip install BeautifulSoup4` _# Allows parsing of html for scraping hltv for training data_
- `pip install pyunpack` _# Allows extract of .dem from .rar file_
- ├ (Dependency) `pip install patool`
- └ (Dependency) `sudo apt-get install rar`