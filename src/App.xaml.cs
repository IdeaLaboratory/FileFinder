using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace FileFinder
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            AppDomain appDomain1 = AppDomain.CurrentDomain;
            appDomain1.UnhandledException += MyHandler;
        }

        private void AppDomain1_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            throw new NotImplementedException();
        }

        static void MyHandler(object sender, UnhandledExceptionEventArgs args)
        {
            string appDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Search\\";
            Exception e = (Exception)args.ExceptionObject;

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

            using (StreamWriter writetext = new StreamWriter(appDir + "FileFinderCrash.log", true))
            {
                writetext.WriteLine(e.ToString());
                                writetext.WriteLine("Runtime terminating: {0}", args.IsTerminating);

            }
        }
    }
}
