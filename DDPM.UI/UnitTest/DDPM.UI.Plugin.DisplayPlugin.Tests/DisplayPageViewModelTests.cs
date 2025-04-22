using DDPM.UI.Common;
using DDPM.UI.Common.Models;
using DDPM.UI.Module.AddDisplay;
using DDPM.UI.Module.AddWebcam;
using DDPM.UI.Plugin.DisplayPlugin.ViewModels;
using DDPM.UI.Plugin.ViewModels;
using Moq;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Input;
using VcpCore.Common;

namespace DDPM.UI.Plugin.DisplayPlugin.Tests
{
    [Apartment(ApartmentState.STA)]
    public class DisplayPageViewModelTests
    {
        private DisplayPageViewModel? displayPageViewModel;
        private readonly AddDeviceViewModel? _vm;        

        [SetUp]
        public void Setup()
        {
            if (!UriParser.IsKnownScheme("pack"))
            {
                new System.Windows.Application();
            }
            displayPageViewModel = new DisplayPageViewModel();
        }

        [Test]
        public void TestModuleGroups()
        {
            var moduleGroups = new List<ModuleGroup>();
            displayPageViewModel.ModuleGroups = moduleGroups;
            Assert.That(displayPageViewModel.ModuleGroups, Is.EqualTo(moduleGroups));
        }

        [Test]
        public void TestGroupSelIdx()
        {
            //_groupSelIdx >= GroupCount
            var groupSelIdx = 2;
            displayPageViewModel.GroupSelIdx = groupSelIdx;
            Assert.That(displayPageViewModel.GroupSelIdx, Is.EqualTo(groupSelIdx));

            //_groupSelIdx < 0
            groupSelIdx = -1;
            displayPageViewModel.GroupSelIdx = groupSelIdx;
            Assert.That(displayPageViewModel.GroupSelIdx, Is.EqualTo(groupSelIdx));

            //_groupSelIdx < GroupCount
            List<ModuleGroup> groups = [];
            ModuleGroup moduleGroup;
            moduleGroup = new ModuleGroup()
            {
                GroupName = "Display",
                GroupIcon = DdpmCommonHelper.GetImageSourceFromCommonResource("Resources/Images/Monitor.png", "DDPM.UI.Resources")
            };
            moduleGroup.AddHeader("Display", new AddDisplayModule(_vm!));
            groups.Add(moduleGroup);

            moduleGroup = new ModuleGroup()
            {
                GroupName = "Webcam",
                GroupIcon = DdpmCommonHelper.GetImageSourceFromCommonResource("Resources/Images/Webcamera.png", "DDPM.UI.Resources")
            };
            moduleGroup.AddHeader("Webcam", new AddWebcamModule(_vm!));
            groups.Add(moduleGroup);

            displayPageViewModel.ModuleGroups = groups;
            groupSelIdx = 1;
            displayPageViewModel.GroupSelIdx = groupSelIdx;
            Assert.That(displayPageViewModel.GroupSelIdx, Is.EqualTo(groupSelIdx));
            Assert.That(displayPageViewModel.RightViewHeaders.Count, Is.EqualTo(1));

            moduleGroup.Dispose();
        }

        [Test]
        public void TestGroupCount()
        {
            //ModuleGroups.Count==0;
            Assert.That(displayPageViewModel.GroupCount, Is.EqualTo(0));

            //ModuleGroups.Count!=0;
            var moduleGroups = new List<ModuleGroup>(){new ModuleGroup(){
                GroupName = "Display",
                GroupIcon = DdpmCommonHelper.GetImageSourceFromCommonResource("Resources/Images/Monitor.png", "DDPM.UI.Resources")}};
            displayPageViewModel.ModuleGroups = moduleGroups;
            Assert.That(displayPageViewModel.ModuleGroups, Is.EqualTo(moduleGroups));
            var result = displayPageViewModel.GroupCount;
            Assert.Greater(result, 0);
        }

        [Test]
        public void TestVbarItems()
        {
            //ModuleGroups.Count==0;
            Assert.That(displayPageViewModel.VbarItems, Is.Not.Null);
        }

        [Test]
        public void TestVbarSelectedIndex()
        {
            //value >= GroupCount
            var vbarSelectedIndex = 2;
            displayPageViewModel.VbarSelectedIndex = vbarSelectedIndex;
            Assert.That(displayPageViewModel.VbarSelectedIndex, Is.EqualTo(vbarSelectedIndex));

            //value < 0
            vbarSelectedIndex = -1;
            displayPageViewModel.VbarSelectedIndex = vbarSelectedIndex;
            Assert.That(displayPageViewModel.VbarSelectedIndex, Is.EqualTo(vbarSelectedIndex));

            //value < GroupCount
            List<ModuleGroup> groups = [];
            ModuleGroup moduleGroup = new ModuleGroup();

            moduleGroup = new ModuleGroup()
            {
                GroupName = "Display",
                GroupIcon = DdpmCommonHelper.GetImageSourceFromCommonResource("Resources/Images/Monitor.png", "DDPM.UI.Resources")
            };
            moduleGroup.AddHeader("Display", new AddDisplayModule(_vm!));
            groups.Add(moduleGroup);

            moduleGroup = new ModuleGroup()
            {
                GroupName = "Webcam",
                GroupIcon = DdpmCommonHelper.GetImageSourceFromCommonResource("Resources/Images/Webcamera.png", "DDPM.UI.Resources")
            };
            moduleGroup.AddHeader("Webcam", new AddWebcamModule(_vm!));
            groups.Add(moduleGroup);

            displayPageViewModel.ModuleGroups = groups;
            vbarSelectedIndex = 1;
            displayPageViewModel.VbarSelectedIndex = vbarSelectedIndex;
            Assert.That(displayPageViewModel.VbarSelectedIndex, Is.EqualTo(vbarSelectedIndex));
            Assert.That(displayPageViewModel.RightViewHeaders.Count, Is.EqualTo(1));

            moduleGroup.Dispose();

        }

        [Test]
        public void TestVbarItemClickCommand()
        {
            var VbarItemClickCommandMock = new Mock<ICommand>();
            var vbarItemClickCommand = VbarItemClickCommandMock.Object;
            displayPageViewModel.VbarItemClickCommand = vbarItemClickCommand;
            Assert.That(displayPageViewModel.VbarItemClickCommand, Is.EqualTo(vbarItemClickCommand));
        }

        [Test]
        public void TestDefaultLeftView()
        {
            Assert.That(displayPageViewModel.DefaultLeftView, Is.Not.Null);
        }

        [Test]
        public void TestLeftView()
        {
            //selGroup == null
            var leftView = new UserControl();
            displayPageViewModel.LeftView = leftView;
            var result = displayPageViewModel.LeftView;
            Assert.That(result, Is.EqualTo(leftView));

            //selGroup != null
            displayPageViewModel.VbarSelectedIndex = 1;
            List<ModuleGroup> groups = [];
            ModuleGroup moduleGroup;
            moduleGroup = new ModuleGroup()
            {
                GroupName = "Display",
                GroupIcon = DdpmCommonHelper.GetImageSourceFromCommonResource("Resources/Images/Monitor.png", "DDPM.UI.Resources")
            };
            moduleGroup.AddHeader("Display", new AddDisplayModule(_vm!));
            groups.Add(moduleGroup);

            moduleGroup = new ModuleGroup()
            {
                GroupName = "Webcam",
                GroupIcon = DdpmCommonHelper.GetImageSourceFromCommonResource("Resources/Images/Webcamera.png", "DDPM.UI.Resources")
            };
            moduleGroup.AddHeader("Webcam", new AddWebcamModule(_vm!));
            groups.Add(moduleGroup);
            displayPageViewModel.ModuleGroups = groups;
            displayPageViewModel.LeftView = leftView;
            result = displayPageViewModel.LeftView;
            Assert.That(result, Is.EqualTo(null));

            moduleGroup.Dispose();
        }

        [Test]
        public void TestRightView()
        {
            //selGroup == null
            var rightView = new UserControl();
            displayPageViewModel.RightView = rightView;
            var result = displayPageViewModel.RightView;
            Assert.That(result, Is.EqualTo(null));

            //selGroup != null
            displayPageViewModel.VbarSelectedIndex = 1;
            List<ModuleGroup> groups = [];
            ModuleGroup moduleGroup;
            moduleGroup = new ModuleGroup()
            {
                GroupName = "Display",
                GroupIcon = DdpmCommonHelper.GetImageSourceFromCommonResource("Resources/Images/Monitor.png", "DDPM.UI.Resources")
            };
            moduleGroup.AddHeader("Display", new AddDisplayModule(_vm!));
            groups.Add(moduleGroup);

            moduleGroup = new ModuleGroup()
            {
                GroupName = "Webcam",
                GroupIcon = DdpmCommonHelper.GetImageSourceFromCommonResource("Resources/Images/Webcamera.png", "DDPM.UI.Resources")
            };
            moduleGroup.AddHeader("Webcam", new AddWebcamModule(_vm!));
            groups.Add(moduleGroup);
            displayPageViewModel.ModuleGroups = groups;
            displayPageViewModel.RightView = rightView;
            result = displayPageViewModel.RightView;
            Assert.That(result, Is.Not.Null);

            moduleGroup.Dispose();
        }

        [Test]
        public void TestRightViewModuleName()
        {
            //SelRightViewHeader==null
            Assert.That(displayPageViewModel.RightViewModuleName, Is.EqualTo("(ERROR)"));

            //SelRightViewHeader!=null,header.DdpmModule != null
            List<ModuleGroup> groups = [];
            ModuleGroup moduleGroup;
            moduleGroup = new ModuleGroup()
            {
                GroupName = "Display",
                GroupIcon = DdpmCommonHelper.GetImageSourceFromCommonResource("Resources/Images/Monitor.png", "DDPM.UI.Resources")
            };
            moduleGroup.AddHeader("Display", new AddDisplayModule(_vm!));
            groups.Add(moduleGroup);

            moduleGroup = new ModuleGroup()
            {
                GroupName = "Webcam",
                GroupIcon = DdpmCommonHelper.GetImageSourceFromCommonResource("Resources/Images/Webcamera.png", "DDPM.UI.Resources")
            };
            moduleGroup.AddHeader("Webcam", new AddWebcamModule(_vm!));
            groups.Add(moduleGroup);
            displayPageViewModel.VbarSelectedIndex = 1;
            displayPageViewModel.ModuleGroups = groups;
            Assert.That(displayPageViewModel.RightViewModuleName, Is.EqualTo("AddWebcamModule"));

            moduleGroup.Dispose();
        }

        [Test]
        public void TestRightViewHeaderSelectedIndex()
        {
            //SelectedGroup==null
            int rightViewHeaderSelectedIndex = 1;
            displayPageViewModel.RightViewHeaderSelectedIndex = rightViewHeaderSelectedIndex;
            Assert.That(displayPageViewModel.RightViewHeaderSelectedIndex, Is.EqualTo(0));

            //SelectedGroup!=null
            List<ModuleGroup> groups = [];
            ModuleGroup moduleGroup;
            moduleGroup = new ModuleGroup()
            {
                GroupName = "Display",
                GroupIcon = DdpmCommonHelper.GetImageSourceFromCommonResource("Resources/Images/Monitor.png", "DDPM.UI.Resources")
            };
            moduleGroup.AddHeader("Display", new AddDisplayModule(_vm!));
            groups.Add(moduleGroup);

            moduleGroup = new ModuleGroup()
            {
                GroupName = "Webcam",
                GroupIcon = DdpmCommonHelper.GetImageSourceFromCommonResource("Resources/Images/Webcamera.png", "DDPM.UI.Resources")
            };
            moduleGroup.AddHeader("Webcam", new AddWebcamModule(_vm!));
            groups.Add(moduleGroup);
            displayPageViewModel.VbarSelectedIndex = 1;
            displayPageViewModel.ModuleGroups = groups;
            displayPageViewModel.RightViewHeaderSelectedIndex = rightViewHeaderSelectedIndex;
            Assert.That(displayPageViewModel.RightViewHeaderSelectedIndex, Is.EqualTo(1));

            moduleGroup.Dispose();
        }

        [Test]
        public void TestSelRightViewHeader()
        {
            //SelectedGroup==null
            Assert.That(displayPageViewModel.SelRightViewHeader, Is.EqualTo(null));

            //SelectedGroup!=null
            List<ModuleGroup> groups = [];
            ModuleGroup moduleGroup;
            moduleGroup = new ModuleGroup()
            {
                GroupName = "Display",
                GroupIcon = DdpmCommonHelper.GetImageSourceFromCommonResource("Resources/Images/Monitor.png", "DDPM.UI.Resources")
            };
            moduleGroup.AddHeader("Display", new AddDisplayModule(_vm!));
            groups.Add(moduleGroup);

            moduleGroup = new ModuleGroup()
            {
                GroupName = "Webcam",
                GroupIcon = DdpmCommonHelper.GetImageSourceFromCommonResource("Resources/Images/Webcamera.png", "DDPM.UI.Resources")
            };
            moduleGroup.AddHeader("Webcam", new AddWebcamModule(_vm!));
            groups.Add(moduleGroup);
            displayPageViewModel.VbarSelectedIndex = 1;
            displayPageViewModel.ModuleGroups = groups;
            var re = displayPageViewModel.SelRightViewHeader;
            Assert.That(displayPageViewModel.SelRightViewHeader, Is.Not.Null);

            moduleGroup.Dispose();
        }

        [Test]
        public void TestRightViewHeaders()
        {
            //SelectedGroup==null&&ModuleGroups.Count==0
            var rightViewHeaders = new ObservableCollection<RightViewHeader>();
            displayPageViewModel.RightViewHeaders = rightViewHeaders;
            Assert.That(displayPageViewModel.RightViewHeaders, Is.EqualTo(rightViewHeaders));

            //SelectedGroup!=null
            List<ModuleGroup> groups = [];
            ModuleGroup moduleGroup;
            moduleGroup = new ModuleGroup()
            {
                GroupName = "Display",
                GroupIcon = DdpmCommonHelper.GetImageSourceFromCommonResource("Resources/Images/Monitor.png", "DDPM.UI.Resources")
            };
            moduleGroup.AddHeader("Display", new AddDisplayModule(_vm!));
            groups.Add(moduleGroup);

            moduleGroup = new ModuleGroup()
            {
                GroupName = "Webcam",
                GroupIcon = DdpmCommonHelper.GetImageSourceFromCommonResource("Resources/Images/Webcamera.png", "DDPM.UI.Resources")
            };
            moduleGroup.AddHeader("Webcam", new AddWebcamModule(_vm!));
            groups.Add(moduleGroup);
            displayPageViewModel.ModuleGroups = groups;
            displayPageViewModel.RightViewHeaders = rightViewHeaders;
            displayPageViewModel.VbarSelectedIndex = 0;
            Assert.That(displayPageViewModel.RightViewHeaders, Is.Not.Null);

            moduleGroup.Dispose();
        }

        [Test]
        public void TestSelectedGroup()
        {
            //ModuleGroups.Count==0
            Assert.That(displayPageViewModel.SelectedGroup, Is.EqualTo(null));

            //VbarSelectedIndex >= 0 && VbarSelectedIndex < ModuleGroups.Count
            List<ModuleGroup> groups = [];
            ModuleGroup moduleGroup;
            moduleGroup = new ModuleGroup()
            {
                GroupName = "Display",
                GroupIcon = DdpmCommonHelper.GetImageSourceFromCommonResource("Resources/Images/Monitor.png", "DDPM.UI.Resources")
            };
            moduleGroup.AddHeader("Display", new AddDisplayModule(_vm!));
            groups.Add(moduleGroup);

            moduleGroup = new ModuleGroup()
            {
                GroupName = "Webcam",
                GroupIcon = DdpmCommonHelper.GetImageSourceFromCommonResource("Resources/Images/Webcamera.png", "DDPM.UI.Resources")
            };
            moduleGroup.AddHeader("Webcam", new AddWebcamModule(_vm!));
            groups.Add(moduleGroup);
            displayPageViewModel.ModuleGroups = groups;
            displayPageViewModel.VbarSelectedIndex = 0;
            Assert.That(displayPageViewModel.SelectedGroup, Is.Not.Null);

            moduleGroup.Dispose();
        }

        [Test]
        public void TestReset()
        {
            try
            {
                displayPageViewModel.Reset();
                Assert.True(true);
                Assert.That(displayPageViewModel.GroupSelIdx, Is.EqualTo(-1));
                Assert.That(displayPageViewModel.VbarSelectedIndex, Is.EqualTo(-1));
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
        }

        [Test]
        public void TestHomeDevices()
        {
            var homeDevices = new List<HomeDevice>();
            displayPageViewModel.HomeDevices = homeDevices;
            Assert.That(displayPageViewModel.HomeDevices, Is.EqualTo(homeDevices));
        }

        [Test]
        public void TestHomeDeviceCount()
        {
            var homeDevices = new List<HomeDevice>() { new HomeDevice() { }, new HomeDevice() { }, new HomeDevice() { } };
            displayPageViewModel.HomeDevices = homeDevices;
            Assert.That(displayPageViewModel.HomeDeviceCount, Is.EqualTo(homeDevices.Count));
        }

        [Test]
        public void TestSelectedHomeDevice()
        {
            var selectedHomeDevice = new HomeDevice();
            displayPageViewModel.SelectedHomeDevice = selectedHomeDevice;
            Assert.That(displayPageViewModel.SelectedHomeDevice, Is.EqualTo(selectedHomeDevice));
        }

        [Test]
        public void TestSelectedMonitorInfo()
        {
            var selectedMonitorInfo = new MonitorInfo();
            displayPageViewModel.SelectedMonitorInfo = selectedMonitorInfo;
            Assert.That(displayPageViewModel.SelectedMonitorInfo, Is.EqualTo(selectedMonitorInfo));
        }

        //[TearDown]
        //public void TeatDown()
        //{
        //    moduleGroup?.Dispose();
        //    moduleGroup = null;
        //}
    }
}