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
        HashMap<string, List<string>>[] filesOfAllDrives;
        private bool doesDatabaseExist = true;
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


        private string _selectedDrive;// = "All Drives";

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

        private bool _readyToSearch = true;

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
                Thread loadingThread = new Thread(delegate ()
                {
                    ExecuteSearch();
                });
                loadingThread.Start();
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
                Thread loadingThread = new Thread(delegate ()
                {
                    ExecuteSearch();
                });
                loadingThread.Start();
            }
        }
        #endregion

        #region Command

        public object ExecuteSearch()
        {
            if (string.IsNullOrEmpty(SearchString))
                return null;

            string path;
            string keyword;
            var includesPath = IncludesPath(out path, out keyword);

            if (includesPath == SearchStringType.NoPath)
            {
                keyword = SearchString;
            }

            List<string> allAvailablepaths = new List<string>();

            //HashMap<string, List<string>> tempFiles = new HashMap<string, List<string>>();
            List<string> paths = null;
            //Regex regex = new Regex(keyword);
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            for (int i = 0; i < _drives.Count; i++)
            {
                foreach (var key in filesOfAllDrives[i].Keys)
                {
                    if (IsMatch(keyword, key, SearchString.Contains('*')))
                    {
                        filesOfAllDrives[i].TryGetValue(key, out paths);

                        if (paths != null)
                            allAvailablepaths.AddRange(paths);
                    }
                }
            }

            if (!(includesPath == SearchStringType.NoPath))
            {
                List<string> tempAvailablepaths = new List<string>(allAvailablepaths);
                allAvailablepaths.Clear();
                foreach (var eachFilePath in tempAvailablepaths)
                {
                    if (IsMatch(path, eachFilePath, false))
                    {
                        allAvailablepaths.Add(eachFilePath);
                    }
                }
            }

            Files = allAvailablepaths;

            //GetFilesFromPath(SearchString, tempFiles);
            stopWatch.Stop();
            Console.WriteLine("GetFilesFromPath" + stopWatch.Elapsed);

            Messages = "About " +
                (allAvailablepaths == null ? 0.ToString() : allAvailablepaths.Count().ToString()) +
                " results found in " + stopWatch.Elapsed.TotalSeconds.ToString("n2") + " second";

            return null;
        }

        private SearchStringType IncludesPath(out string path, out string keyword)
        {
            // check if searchString includs path
            path = null;
            keyword = null;
            int pathEndIndex = SearchString.LastIndexOf('\\');
            if (pathEndIndex < 0)
            {
                return SearchStringType.NoPath;
            }

            path = SearchString.Substring(0, pathEndIndex + 1);
            keyword = SearchString.Substring(pathEndIndex + 1, _searchString.Length - pathEndIndex - 1);

            if (Directory.Exists(path))
            {
                return SearchStringType.FullPath;
            }

            return SearchStringType.PartialPath;
        }

        enum SearchStringType
        {
            FullPath,
            PartialPath,
            NoPath
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
            //_drives.Add("All Drives");
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

        private bool IsMatch(string keyword, string key, bool hasRegex = true)
        {
            if (!hasRegex)
            {
                if (MatchCase)
                    return key.Contains(keyword);
                return key.ToUpper().Contains(keyword.ToUpper());
            }

            keyword = WildCardToRegular(keyword);

            if (MatchCase)
                return Regex.IsMatch(key, keyword);
            return Regex.IsMatch(key, keyword, RegexOptions.IgnoreCase);
        }

        private object Load()
        {
            // initialize 
            filesOfAllDrives = new HashMap<string, List<string>>[_drives.Count];
            InitializeTempFilesArray(filesOfAllDrives);

            if (!Directory.Exists(appDir + "db\\"))
            {
                Messages = "Creating database for the first time";
                LoadAllFiles();
                doesDatabaseExist = false;
            }
            else
            {
                Messages = "Loading database";
                var bins = Directory.GetFiles(appDir + "db\\", "*.bin");
                bool hasDatabase;
                for (int i = 0; i < _drives.Count; i++)
                {
                    hasDatabase = false;
                    foreach (var bin in bins)
                    {
                        var filename = Path.GetFileNameWithoutExtension(bin);
                        if (_drives[i].Contains(filename))
                        {
                            filesOfAllDrives[i] = BinarySerializer.Deserialize<HashMap<string, List<string>>>(bin);
                            hasDatabase = true;
                            break;
                        }
                    }
                    if (!hasDatabase)
                    {
                        ReadyToSearch = false;
                        Messages = "Creating new drive database";
                        GetFilesFromPath(_drives[i], filesOfAllDrives[i]);
                        BinarySerializer.Serialize(filesOfAllDrives[i], appDir + "db\\" + _drives[i].Substring(0, _drives[i].Length - 2) + ".bin");
                    }
                }
            }

            Messages = "Ready to search";
            ReadyToSearch = true;

            if (!doesDatabaseExist)
            {
                CreateDataBase();
            }

            return null;
        }

        private void CreateDataBase()
        {
            int x = 0;
            foreach (var tempFiles in filesOfAllDrives)
            {
                if (!(tempFiles != null && tempFiles.Count > 0))
                {
                    continue;
                }

                BinarySerializer.Serialize(tempFiles, appDir + "db\\" + _drives[x].Substring(0, _drives[x].Length - 2) + ".bin");
                x++;
            }
        }

        private void LoadAllFiles()
        {

            Parallel.For(0, _drives.Count, index =>
                        {
                            GetFilesFromPath(_drives[index], filesOfAllDrives[index]);
                        }
                        );


            //object obj = new object();
            //lock (obj)
            //{
            //    for (int i = 1; i < _drives.Count; i++)
            //    {
            //        Thread loadingThread = new Thread(delegate ()
            //        {
            //            GetFilesFromPath(_drives[i], tempFilesArray[(i - 1)]);

            //        //allFiles = tempFilesArray[i];
            //        //BinarySerializer.Serialize(tempFilesArray[i - 1], appDir + "searchDatabase_" + i.ToString() + ".bin");
            //    });
            //        loadingThread.Start();
            //    }
            //}
        }

        private void InitializeTempFilesArray(HashMap<string, List<string>>[] tempFilesArray)
        {
            for (int i = 0; i < _drives.Count; i++)
            {
                tempFilesArray[i] = new HashMap<string, List<string>>();
            }
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
