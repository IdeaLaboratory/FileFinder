using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using HashMap;

namespace FileFinder.ViewModel
{
    public partial class SearchManagerViewModel : BaseViewModel
    {

        #region private fields
        HashMap<string, List<string>> allFiles = null;
        string appDir; 
        #endregion

        #region ctor
        private SearchManagerViewModel()
        {
            LoadAllDrives();
            CheckSystemRequirements();
            Thread loadingThread = new Thread(delegate ()
            {
                Load();
            });
            loadingThread.Start();
        }
        #endregion

        #region singleton

        private static SearchManagerViewModel _instance;

        public static SearchManagerViewModel Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new SearchManagerViewModel();
                }
                return _instance;
            }
        } 
        #endregion

        #region Public properties

        private List<string> _files = new List<string>();

        public List<string> Files
        {
            get
            {
                return _files;
            }
            set
            {
                _files = value;
                OnPropertyChanged("Files");
            }
        }


        private string _selectedDrive = "All Drives";

        public string SelectedDrive
        {
            get { return _selectedDrive; }
            set { _selectedDrive = value; }
        }


        private List<string> _drives = new List<string>();

        public List<string> Drives
        {
            get
            {
                return _drives;
            }
            set
            {
                _drives = value;
                OnPropertyChanged("Drives");
            }
        }

        private string _messages = "Welcome";

        public string Messages
        {
            get
            {
                return _messages;
            }
            set
            {
                _messages = value;
                OnPropertyChanged("Messages");
            }
        }

        private bool _readyToSearch;

        public bool ReadyToSearch
        {
            get { return _readyToSearch; }
            set
            {
                _readyToSearch = value;
                OnPropertyChanged("ReadyToSearch");
            }
        }

        private string _searchString;

        public string SearchString
        {
            get { return _searchString; }
            set
            {
                _searchString = value;
                OnPropertyChanged("SearchString");
            }
        }

        private bool tpl;

        public bool TPL
        {
            get
            {
                return tpl;
            }
            set
            {
                tpl = value;
            }
        }

        private bool _matchCase = false;

        public bool MatchCase
        {
            get { return _matchCase; }
            set
            {
                _matchCase = value;
                OnPropertyChanged("MatchCase");
            }
        }
        #endregion
        
        #region Command

        public object ExecuteSearch()
        {
            if (string.IsNullOrEmpty(SearchString))
                return null;

            string keyword = SearchString;

            keyword = WildCardToRegular(keyword);

            List<string> allAvailablepaths = new List<string>();

            HashMap<string, List<string>> tempFiles = new HashMap<string, List<string>>();
            List<string> paths = null;
            Regex regex = new Regex(keyword);
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            foreach (var key in allFiles.Keys)
            {
                if (IsMatch(keyword, key))
                {
                    allFiles.TryGetValue(key, out paths);

                    if (paths != null)
                        allAvailablepaths.AddRange(paths);
                }
            }
            Files = allAvailablepaths;

            GetFilesFromPath(SearchString, tempFiles);
            stopWatch.Stop();
            Console.WriteLine("GetFilesFromPath" + stopWatch.Elapsed);

            Messages = "About " +
                (allAvailablepaths == null ? 0.ToString() : allAvailablepaths.Count().ToString()) +
                " results found in " + stopWatch.Elapsed.TotalSeconds.ToString("n2") + " second";

            return null;
        }

        #endregion

        #region private methods

        private void CheckSystemRequirements()
        {
            appDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Search\\";
            #region create application directory
            if (!Directory.Exists(appDir))
            {
                Directory.CreateDirectory(appDir);
            }
            #endregion

            #region create log file

            if (!File.Exists(appDir + "searchLoadException.log"))
            {
                var file = File.Create(appDir + "searchLoadException.log");
                file.Close();
                file.Dispose();
            }
            #endregion
        }

        private void LoadAllDrives()
        {
            _drives.Add("All Drives");
            foreach (var eachDrive in DriveInfo.GetDrives())
            {
                if (eachDrive.IsReady)
                    _drives.Add(eachDrive.Name);
            }
        }

        private String WildCardToRegular(String value)
        {
            return "^" + Regex.Escape(value).Replace("\\?", ".").Replace("\\*", ".*") + "$";
        }

        private bool IsMatch(string keyword, string key)
        {
            if (MatchCase)
                return Regex.IsMatch(key, keyword);
            return Regex.IsMatch(key, keyword, RegexOptions.IgnoreCase);
        }

        private object Load()
        {
            if (!File.Exists(appDir + "searchDatabase.bin"))
            {
                Messages = "Creating Database for the first time";
                LoadAllFiles();
            }
            else
            {
                Messages = "Loading database";
                allFiles = BinarySerializer.Deserialize<HashMap<string, List<string>>>(appDir + "searchDatabase.bin");
            }
            Messages = "Ready to search";
            ReadyToSearch = true;
            return null;
        }

        private void LoadAllFiles()
        {
            HashMap<string, List<string>> tempFiles = new HashMap<string, List<string>>();
            if (SelectedDrive == "All Drives")
            {
                int x = 0;
                if (TPL == true)
                {
                    Parallel.For(1,
                                 _drives.Count - 1,
                                 index =>
                                 {
                                     GetFilesFromPath(_drives[x++], tempFiles);
                                 });
                }
                else
                {

                    for (int i = 1; i < _drives.Count; i++)
                    {
                        GetFilesFromPath(_drives[i], tempFiles);
                    }
                }
            }
            else
            {
                GetFilesFromPath(SelectedDrive, tempFiles);
            }
            allFiles = tempFiles;
            BinarySerializer.Serialize(tempFiles, appDir + "searchDatabase.bin");
        }

        private void GetFilesFromPath(string path, HashMap<string, List<string>> files)
        {
            try
            {
                foreach (var filePath in Directory.GetFiles(path))
                {
                    var fileAttributes = File.GetAttributes(filePath);
                    if ((fileAttributes & FileAttributes.System) == FileAttributes.System &&
                        (fileAttributes & FileAttributes.Directory) != FileAttributes.Directory)
                        continue;

                    var eachFileInfo = new FileInfo(filePath);
                    List<string> fileAvailablePaths;
                    if (files.TryGetValue(eachFileInfo.Name, out fileAvailablePaths))
                    {
                        fileAvailablePaths.Add(filePath);
                    }
                    else
                    {
                        fileAvailablePaths = new List<string>();
                        fileAvailablePaths.Add(filePath);
                        files.Add(eachFileInfo.Name, fileAvailablePaths);
                    }
                }
            }
            catch (Exception e)
            {
#if DEBUG
                using (StreamWriter writetext = new StreamWriter(appDir + "searchLoadException.log", true))
                {
                    writetext.WriteLine(e.Message);
                }
#endif
                return;
            }

            foreach (string eachPath in Directory.GetDirectories(path))
            {
                GetFilesFromPath(eachPath, files);
            }
        } 
        #endregion
    }
}
