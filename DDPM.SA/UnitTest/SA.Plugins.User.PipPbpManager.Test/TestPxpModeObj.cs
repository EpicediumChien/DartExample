using DDPM.SA.Common.Display;
using Dell.Client.Framework.UnitTestShared.Tests;

namespace SA.Plugins.User.PipPbpManager.Test
{
    public class TestPxpModeObj
    {
        [Test]
        public void PxpModeObjTest()
        {
            string arg;
            UInt16 modeMode;
            string desc;

            arg = "pip-large";
            modeMode = 0x22;
            desc = "PIP large";

            PxpModeObj pxpModeObj = new PxpModeObj(arg, modeMode, desc);
            PrivateObject privatepxpModeObj = new PrivateObject(pxpModeObj);
            var get_pxpMode_arg = privatepxpModeObj.GetFieldOrProperty("_arg");
            var get_pxpMode_modeMode = privatepxpModeObj.GetFieldOrProperty("_modeCode");
            var get_pxpMode_description = privatepxpModeObj.GetFieldOrProperty("_description");

            Assert.That(arg, Is.EqualTo(get_pxpMode_arg));
            Assert.That(modeMode, Is.EqualTo(get_pxpMode_modeMode));
            Assert.That(desc, Is.EqualTo(get_pxpMode_description));
        }

        [Test]
        public void PxpModeObjTableTest()
        {
            PxpModeObj[] Table1 =
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
            var PxpModeObjTable = PxpModeObj.Table;
            Assert.That(Table1.Length, Is.EqualTo(PxpModeObjTable.Length));
            Assert.That(Table1[1].ModeCode, Is.EqualTo(PxpModeObjTable[1].ModeCode));
            Assert.That(Table1[1].Arg, Is.EqualTo(PxpModeObjTable[1].Arg));
            Assert.That(Table1[15].ModeCode, Is.EqualTo(PxpModeObjTable[15].ModeCode));
            Assert.That(Table1[15].Arg, Is.EqualTo(PxpModeObjTable[15].Arg));
            Assert.That(Table1[27].ModeCode, Is.EqualTo(PxpModeObjTable[27].ModeCode));
            Assert.That(Table1[27].Arg, Is.EqualTo(PxpModeObjTable[27].Arg));
        }
    }
}