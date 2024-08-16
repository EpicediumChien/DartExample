namespace DDPM.SA.Common.Settings
{
    public class DDPMITConfig
    {
        public double Version { get; set; } = 1.0;

        //global setting -> Analytics page -> checkbox enable/disable
        public bool isTelemetryConsentAllow { get; set; } = true;
    }
}