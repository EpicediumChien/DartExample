using DdmLibrary;
using System;
using System.Management;

namespace DDPM.SA.Common
{
    public class GeneralFunction
    {
        public string GetMonitorAdapter()
        {
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_VideoController");
            foreach (ManagementObject mo in searcher.Get())
            {
                PropertyData currentBitsPerPixel = mo.Properties["CurrentBitsPerPixel"];
                PropertyData description = mo.Properties["Description"];
                if (currentBitsPerPixel != null && description != null)
                {
                    if (currentBitsPerPixel.Value != null)
                        return(description.Value).ToString();
                }
            }
            return string.Empty;
        }
    }    
}