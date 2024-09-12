using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IndiLogic.DPeM.Broker;
using Newtonsoft.Json;

namespace DDPM.SA.Common
{
    public class WebcamProfile
    {
        public string Id;
        public string Name;
        public string Description;
        public int Priority;
        public bool IsFocusOn;
        public int Focus;
        public int Pan;
        public int Tilt;
        public int Zoom;
        public int Brightness;
        public int Contrast;
        public int AntiFlicker;
        public int Saturation;
        public int Sharpness;
        public bool IsAutoWhiteBalanceOn;
        public int AutoWhiteBalance;
        public bool IsAutoFramingOn;
        public int AutoFramingSensitivity;
        public int AutoFramingFrameSize;
        public bool IsAutoFramingTransitionOn;
        public int FieldOfView;
        public bool IsHDROn;

        //public WebcamProfile(IWebcamProfileReadOnly profile)
        //{
        //    this.Id = profile.Id;
        //    Name = profile.Name;
        //    Description = profile.Description;
        //    Priority = profile.Priority;
        //    IsFocusOn = profile.IsFocusOn;
        //    Focus = profile.Focus;
        //    Pan = profile.Pan;
        //    Tilt = profile.Tilt;
        //    Zoom = profile.Zoom;
        //    Brightness = profile.Brightness;
        //    Contrast = profile.Contrast;
        //    AntiFlicker = profile.AntiFlicker;
        //    Saturation = profile.Saturation;
        //    Sharpness = profile.Sharpness;
        //    IsAutoWhiteBalanceOn = profile.IsAutoWhiteBalanceOn;
        //    AutoWhiteBalance = profile.AutoWhiteBalance;
        //    IsAutoFramingOn = profile.IsAutoFramingOn;
        //    AutoFramingSensitivity = profile.AutoFramingSensitivity;
        //    AutoFramingFrameSize = profile.AutoFramingFrameSize;
        //    IsAutoFramingOn = profile.IsAutoFramingOn;
        //    FieldOfView = profile.FieldOfView;
        //    IsHDROn = profile.IsHDROn;
        //}
    }
}
