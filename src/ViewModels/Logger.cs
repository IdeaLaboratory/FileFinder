using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace FileFinder.ViewModels
{
    internal static class Logger
    {
        /// <summary>
        /// Write to Log file
        /// </summary>
        /// <param name="path"></param>
        /// <param fileName="fileName"></param>
        /// <param name="message"></param>
        internal static void WriteLog(string path, string fileName, string message)
        {
            GetReadyToLog(path);

            using (StreamWriter writetext = new StreamWriter(path + fileName, true))
            {
                writetext.WriteLine(message);
            }
        }

        private static void GetReadyToLog(string appDir)
        {
            if (!Directory.Exists(appDir))
            {
                Directory.CreateDirectory(appDir);
            }

            if (!File.Exists(appDir + "FileFinderCrash.log"))
            {
                var file = File.Create(appDir + "FileFinderCrash.log");
                file.Close();
                file.Dispose();
            }
        }
    }
}
