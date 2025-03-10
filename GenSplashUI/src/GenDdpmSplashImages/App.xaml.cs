using System.Configuration;
using System.Data;
using System.IO;
using System.Windows;

namespace GenDdpmSplashImages
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        //Command Line Arguments
        public static string SolutionDir = ".";
        public static string Year = "2025";
        public static string Build = "2.0.1.0";
        public static string Color = "dark";
        public static string Size = "normal";

        // ========================================================================================
        //                           GetAppDir
        // ========================================================================================
        //Get actual AppDir
        public static string GetAppDir(bool blEndWithBkSlash = true)
        {
            string strAppDir = System.IO.Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
            if (blEndWithBkSlash)
            {
                if (!strAppDir.EndsWith("\\"))
                    strAppDir += "\\";
            }
            return strAppDir;
        }
        // ========================================================================================
        //                           ParsingCmdLine
        // ========================================================================================
        private bool ParsingCmdLine(string[] args)
        {
            if (args.Length <= 0)
                return true;

            int state = 0;
            foreach (string arg in args) 
            {
                switch (state)
                {
                    case 0:
                        if (arg.Equals("-Dir", StringComparison.OrdinalIgnoreCase))
                            state = 10;
                        else if (arg.Equals("-Year", StringComparison.OrdinalIgnoreCase))
                            state = 20;
                        else if (arg.Equals("-Build", StringComparison.OrdinalIgnoreCase))
                            state = 21;
                        else if (arg.Equals("-Color", StringComparison.OrdinalIgnoreCase))
                            state = 30;
                        else if (arg.Equals("-Size", StringComparison.OrdinalIgnoreCase))
                            state = 40;
                        break;
                    case 10: // -SolutionDir {SolutionDir}
                        SolutionDir = arg;
                        if (!Directory.Exists(SolutionDir))
                            return false;
                        break;
                    case 20: //-Year {Year}
                        Year = arg;
                        state = 0;
                        break;
                    case 21: //-Build {Build}
                        Build = arg;
                        state = 0;
                        break;
                    case 30: //-Color {dark or light}
                        state = 0;
                        Color = arg;
                        break;
                    case 40: //-Size {normal or 4k}
                        state = 0;
                        Size = arg;
                        break;
                }
            }

            return true;
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            if (e.Args.Length > 0)
            {
                if (!ParsingCmdLine(e.Args))
                {
                    Application.Current.Shutdown(1);
                }
            }
        }
    }

}
