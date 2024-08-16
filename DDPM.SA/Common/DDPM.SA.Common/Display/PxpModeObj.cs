using System;
using System.Collections.Generic;

namespace DDPM.SA.Common.Display
{
    public class PxpModeObj
    {
        #region Native data

        private string _arg;
        private UInt16 _modeCode;
        private string _description;

        public string Arg { get => _arg; }
        public UInt16 ModeCode { get => _modeCode; }

        #endregion Native data

        #region ctor

        public PxpModeObj(string arg, UInt16 modeMode, string desc)
        {
            _arg = arg;
            _modeCode = modeMode;
            _description = desc;
        }

        public PxpModeObj(UInt16 modeCode)
        {
            _modeCode = modeCode;
            PxpModeObj? obj = Array.Find(Table, x => x._modeCode == modeCode);
            if (obj != null)
            {
                _arg = obj._arg;
                _description = obj._description;
            }
            else
            {
                _arg = "";
                _description = "(Invalid modeCode)";
            }
        }

        #endregion ctor

        #region Table

        public static PxpModeObj[] Table =
        {
            new PxpModeObj("off", 0x00, "PIP/PBP off, full screen"),
            new PxpModeObj("pip", 0x21, "PIP small"),
            new PxpModeObj("pip-small", 0x21, "PIP small"),
            new PxpModeObj("pip-large", 0x22, "PIP large"),
            new PxpModeObj("pip-2h", 0x23, "PBP 2 window h-split"),
            new PxpModeObj("pbp-2h", 0x23, "PBP 2 window h-split"),
            new PxpModeObj("pbp", 0x23, "PBP 2 window h-split"),
            new PxpModeObj("split", 0x23, "PBP 2 window h-split"),
            new PxpModeObj("pbp-2h-fill", 0x24, "PBP 2 window h-split, fill"),
            new PxpModeObj("pbp-2h-37", 0x25, "PBP 2 window h-split,3:7"),
            new PxpModeObj("pbp-2h-73", 0x26, "PBP 2 window h-split,7:3"),
            new PxpModeObj("pbp-2h-28", 0x27, "PBP 2 window h-split,2:8"),
            new PxpModeObj("pbp-2h-82", 0x28, "PBP 2 windows h-split,8:2"),
            new PxpModeObj("pbp-2h-2575", 0x29, "PBP 2 windows h-split,25%:75%"),
            new PxpModeObj("pbp-2h-7525", 0x2A, "PBP 2 windows h-split,75%:25%"),
            new PxpModeObj("pbp-2h-2674", 0x2B, "PBP 2 windows h-split,26%:74%"),
            new PxpModeObj("pbp-2h-7426", 0x2C, "PBP 2 windows h-split,74%:26%"),
            new PxpModeObj("pbp-2h-3367", 0x2D, "PBP 2 windows h-split,33%:67%"),
            new PxpModeObj("pbp-2h-6733", 0x2E, "PBP 2 windows h-split,67%:33%"),
            new PxpModeObj("pbp-2v", 0x2F, "PBP 2 windows v-split"),
            new PxpModeObj("pbp-3a", 0x31, "PBP 3 windows-L1(half)/R2(up|down split)"),
            new PxpModeObj("pbp-3b", 0x32, "PBP 3 windows-L2(up|down split)/R1(half)"),
            new PxpModeObj("pbp-3c", 0x33, "PBP 3 windows-Up1(half)/Down2(left|right split)"),
            new PxpModeObj("pbp-3d", 0x34, "PBP 3 windows-1row, 3column"),
            new PxpModeObj("pbp-3e", 0x35, "PBP 3 windows-Up2(left|right split)/Down1(half)"),
            new PxpModeObj("pbp-4a", 0x41, "PBP 4 windows-quadrant"),
            new PxpModeObj("quad", 0x41, "PBP 4 windows-quadrant"),
            new PxpModeObj("pbp-4b", 0x42, "PBP 4 windows-1row, 4column")
        };

        public static string[] GetModeArgListFromModes(UInt16[] modes)
        {
            List<string> argList = new List<string>();
            foreach (UInt16 mode in modes)
            {
                PxpModeObj? obj = Array.Find(Table, x => x._modeCode == mode);
                if (obj != null)
                {
                    argList.Add(obj._arg);
                }
            }
            return argList.ToArray();
        }

        public static string GetArgFromModeCode(UInt16 modeCode)
        {
            PxpModeObj? obj = Array.Find(Table, x => x._modeCode == modeCode);
            if (obj != null)
            {
                return obj._arg;
            }
            return "";
        }

        #endregion Table

    }
}