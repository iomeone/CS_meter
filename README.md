# CS_meter
Win probability meter for Counter-Strike

# Project parts
1. Download demos which will be parsed for training data
2. Parse demos into csv data for use with python/tensorflow
3. Create tensorflow model from training data

## 1. download_demos.py
This script is used to download .dem files from hltv. Hltv hosts demos for matches in a .rar archive. A series of recent demos can be downloaded using `python download_demos.py {dest_directory}`. Run without arguments for usage.

_Demos are downloaded as .rar files, the following packages are used to download .dem files from hltv and extract them from the .rar archive_
- `conda install requests` | Version 2.19.1 | _# Responsible for downloading .rar from hltv.org_
- `conda install tqdm` | Version 4.25.0 | _# Provides an in-console download progress bar_
- `pip install BeautifulSoup4` | Version 4.6.3 | _# Allows parsing of html for scraping hltv for training data_
- `pip install pyunpack` | Version 0.1.2 | _# Allows extract of .dem from .rar file_
- ├ (Dependency) `pip install patool` | Version 1.12
- └ (Dependency) `sudo apt-get install rar`

## 2. parse-demos
**Note, parse-demos uses a version of [C# DemoInfo](https://github.com/StatsHelix/demoinfo) which has been ported to .net core. No modifications were made to the source files. See Additional Resources**
1. Install `dotnet` sdk. For ubuntu: 18.04.1, [(instructions from)]( https://dev.to/carlos487/installing-dotnet-core-in-ubuntu-1804-7lp):
    - `sudo apt-key adv --keyserver packages.microsoft.com --recv-keys EB3E94ADBE1229CF`
    - `sudo apt-key adv --keyserver packages.microsoft.com --recv-keys 52E16F86FEE04B979B07E28DB02C46DF417A0893`
    - `sudo sh -c 'echo "deb [arch=amd64] https://packages.microsoft.com/repos/microsoft-ubuntu-bionic-prod bionic main" > /etc/apt/sources.list.d/dotnetdev.list'`
    - `sudo apt-get update`
    - `sudo apt-get install dotnet-sdk-2.1.105`
2. `cd src/parse-demos`
3. `dotnet restore` _# Restores dotnet packages from nuget as they aren't included in git_
4. `dotnet build -c Release` _# Build release configuration of parse-demos_
5. `dotnet bin/Release/netcoreapp2.0/parse-demos.dll <demo-directory> ../../csv/individual-matches/`
    - _# This parser requires 2 positional arguments_
    - _\<demo-directory\> The input directory of demo files to be parsed. Likely the directory from step 1: `download_demos.py`_
    - _`../../csv/individual-matches` The output directory of csv files from parsed demos. Can be the same as input directory. parse-demos will not overwrite csv files which already exist, it will instead skip parsing the demo file entirely_
6. `cd /csv` _# The output from step 5 should be in /csv/individual-matches. The git repo should contain a sample of match csv files_
7. `python merge_csv.py` _# Merges all csv files in the individual-matches directory to a single file, /csv/all_data.csv_


---

# Additional Resources

Details of live-stream communcation with running CS observer client can be found here: https://developer.valvesoftware.com/wiki/Counter-Strike:_Global_Offensive_Game_State_Integration

Details of .dem file structure can be found in these repos which implement demo parsing and event triggering:
* Valve Official, C++ - https://github.com/ValveSoftware/csgo-demoinfo
* Unofficial, Python - https://github.com/Bakkes/demoinfo-csgo-python
* Unofficial, C# *appears to have good support, using this to generate training data - https://github.com/StatsHelix/demoinfo
* https://github.com/tgjeon/TensorFlow-Tutorials-for-Time-Series
