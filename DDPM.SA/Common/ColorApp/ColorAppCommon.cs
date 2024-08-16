namespace DDPM.ColorApp
{
    internal class ColorAppCommon
    {
        //Get actual AppDir
        public static string GetAppDir(bool blEndWithBkSlash = true)
        {
            string? strAppDir = System.IO.Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
            if (string.IsNullOrEmpty(strAppDir))
                return string.Empty;

            if (blEndWithBkSlash)
            {
                if (!strAppDir.EndsWith("\\"))
                    strAppDir += "\\";
            }
            return strAppDir;
        }
    }
}