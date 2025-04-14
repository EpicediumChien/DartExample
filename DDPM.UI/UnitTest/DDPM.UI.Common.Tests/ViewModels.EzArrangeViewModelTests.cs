using DDPM.SA.Common;
using DDPM.SA.Common.Settings;
using DDPM.UI.Common.Interfaces;
using DDPM.UI.Common.Models;
using DDPM.UI.Common.UserControls;
using DDPM.UI.Common.ViewModels;
using DDPM.UI.Common.Views;
using DDPM.UI.Interfaces;
using DDPM.UI.Module.AddHeadset_BL;
using DDPM.UI.Module.Brightness;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.UX.WPF;
using Microsoft.VisualBasic.Logging;
using Moq;
using NGA.UnitTest.PrivateObject;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using VcpCore.Common;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Menu;

namespace DDPM.UI.Common.Tests
{
    [Apartment(ApartmentState.STA)]
    public class EzArrangeViewModellTests
    {
        private EzArrangeViewModel? ezArrangeViewModel;
        private Mock<IDeviceManagerSA> deviceManagerSAMock;
        private PrivateObject? privateObject;
        private Mock<ILog>? logMock;
        private ILog? log;
        private Mock<IConsole>? myConsoleMock;
        private HomeDevice? homeDev;

        [SetUp]
        public void Setup()
        {
            if (System.Windows.Application.Current == null)
            {
                new System.Windows.Application();
            }
            var resourceDictionary = new ResourceDictionary();
            resourceDictionary.Source = new Uri("pack://application:,,,/DDPM.UI.Common;component/ModuleStyle.xaml");
            System.Windows.Application.Current.Resources.MergedDictionaries.Add(resourceDictionary);
            homeDev = new HomeDevice();
            logMock = new Mock<ILog>();
            log = logMock.Object;
            var myConsoleMock = new Mock<IConsole>();
            DdpmCommonHelper.MyConsole = myConsoleMock.Object;
            myConsoleMock.Setup(x => x.CreateLog(It.IsAny<string>())).Returns(log);
            deviceManagerSAMock = new Mock<IDeviceManagerSA>();
            HomeDevice.DeviceManagerSA = deviceManagerSAMock.Object;
            //Robert_Lin, 2025-1-7 IsEAFuctionEnabled is deleted.
            //deviceManagerSAMock.Setup(x => x.GetEAFunctionEnabled()).Returns(Task.FromResult(new ObjGetVCP()));
            ezArrangeViewModel = new EzArrangeViewModel(homeDev);
            privateObject = new PrivateObject(ezArrangeViewModel);
        }

        [Test]
        public void TestConstructor_EzArrangeViewModel()
        {
            // Assert
            Assert.That(ezArrangeViewModel, Is.Not.Null);
        }

        [Test]
        public void TestIsEaFunctionEnabled()
        {
            //Robert_Lin, 2025-1-7 IsEAFuctionEnabled is deleted.
            /*
            deviceManagerSAMock.Setup(x => x.SetEAFunctionEnabled(It.IsAny<bool>())).Returns(Task.FromResult(true));
            ezArrangeViewModel.IsEaFunctionEnabled = true;

            // Assert
            Assert.That(ezArrangeViewModel.IsEaFunctionEnabled, Is.EqualTo(true));

            deviceManagerSAMock.Setup(x => x.SetEAFunctionEnabled(It.IsAny<bool>())).Returns(Task.FromResult(false));
            ezArrangeViewModel.IsEaFunctionEnabled = false;

            // Assert
            Assert.That(ezArrangeViewModel.IsEaFunctionEnabled, Is.EqualTo(true));
            */
        }

        [Test]
        public void TestSetWorkSplit()
        {
            //Robert_Lin, 2025-1-7 SetEAWorkSplit is deleted.
            /*
            deviceManagerSAMock.Setup(x=>x.SetEAWrokSplit(It.IsAny<MonitorInfo>(),It.IsAny<int>(), It.IsAny<char>(),It.IsAny<List<double>>())).Returns(Task.FromResult(true));
            try
            {
                ezArrangeViewModel.SetWorkSplit(1,'a');
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
            */
        }

        [Test]
        public void TestListViewItemClickCommand()
        {
            var iCommandMock = new Mock<ICommand>();
            ezArrangeViewModel.ListViewItemClickCommand = iCommandMock.Object;
            Assert.That(ezArrangeViewModel.ListViewItemClickCommand, Is.EqualTo(iCommandMock.Object));
        }

        [Test]
        public void TestOnListViewItemClicked()
        {
            var spItem = new SplitItem();
            try
            {
                ezArrangeViewModel.OnListViewItemClicked(spItem);
                Assert.True(true);
                Assert.That(ezArrangeViewModel.SelectedSplitItem, Is.EqualTo(spItem));
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
        }

        [Test]
        public void TestSelectedSplitItem()
        {
            //_selectedSplitItem != null
            var spItem = new SplitItem() { Buddy = new SplitItem() };
            privateObject.SetFieldOrProperty("_selectedSplitItem", spItem);
            ezArrangeViewModel.SelectedSplitItem = spItem;
            // Assert
            Assert.That(ezArrangeViewModel.SelectedSplitItem, Is.EqualTo(spItem));

            var spItema = new SplitItem() { Buddy = new SplitItem() };
            privateObject.SetFieldOrProperty("_selectedSplitItem", spItema);
            ezArrangeViewModel.SelectedSplitItem = spItem;
            // Assert
            Assert.That(ezArrangeViewModel.SelectedSplitItem, Is.EqualTo(spItem));

            //value != null
            ezArrangeViewModel.SelectedSplitItem = null;
            // Assert
            Assert.That(ezArrangeViewModel.SelectedSplitItem, Is.EqualTo(null));
        }

        [Test]
        public void TestSplitItemEditCommand()
        {
            var iCommandMock = new Mock<ICommand>();
            ezArrangeViewModel.SplitItemEditCommand = iCommandMock.Object;
            Assert.That(ezArrangeViewModel.SplitItemEditCommand, Is.EqualTo(iCommandMock.Object));
        }

        [Test]
        public void TestOnSplitItemEditCommand()
        {
            var spItem = new SplitItem();
            try
            {
                ezArrangeViewModel.OnSplitItemEditCommand(spItem);
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
        }

        [Test]
        public void TestCreateLog()
        {
            var consoleMock = new Mock<IConsole>();
            try
            {
                ezArrangeViewModel.CreateLog(consoleMock.Object, "logName");
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
        }

        [Test]
        public void TestLogInfo()
        {
            var spItem = new SplitItem();
            try
            {
                ezArrangeViewModel.LogInfo("message");
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
        }

        [Test]
        public void TestIsVertical()
        {
            ezArrangeViewModel.IsVertical = true;
            Assert.That(ezArrangeViewModel.IsVertical, Is.EqualTo(true));
        }

        [Test]
        public void TestsplitListRightView()
        {
            var splitListRightView = new SplitListView();
            ezArrangeViewModel.splitListRightView = splitListRightView;
            Assert.That(ezArrangeViewModel.splitListRightView, Is.EqualTo(splitListRightView));
        }

        [Test]
        public void TestFindProfileSettingById()
        {
            var easyArrangementDDPM = new EasyArrangementDDPM();
            var result = ezArrangeViewModel.FindProfileSettingById(easyArrangementDDPM, 1);
            Assert.That(result, Is.Null);
            HomeDevice homeDevice = new HomeDevice();
            homeDevice.MonitorInfo = new MonitorInfo { edid = new EDID { Instance = "1" } };
            privateObject.SetFieldOrProperty("_homeDevice", homeDevice);
            easyArrangementDDPM = new EasyArrangementDDPM() { Desktops = new List<DesktopDDPM>() { new DesktopDDPM("1", 2) { ProfileSettings = new List<EzProfileSettingDDPM>() { new EzProfileSettingDDPM(1, true, 2, true) { ID = 1 } } }, new DesktopDDPM("3", 4) } };
            result = ezArrangeViewModel.FindProfileSettingById(easyArrangementDDPM, 1);
            Assert.That(result, Is.Not.Null);

            easyArrangementDDPM = new EasyArrangementDDPM() { Desktops = new List<DesktopDDPM>() { new DesktopDDPM("a", 2), new DesktopDDPM("3", 4) } };
            result = ezArrangeViewModel.FindProfileSettingById(easyArrangementDDPM, 1);
            Assert.That(result, Is.Null);

        }

        [Test]
        public void TestConvertAutoLaunchtimeToTime()
        {
            var result = ezArrangeViewModel.ConvertAutoLaunchtimeToTime(null);
            Assert.That(result, Is.EqualTo(String.Empty));

            result = ezArrangeViewModel.ConvertAutoLaunchtimeToTime(1);
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void TestConvertToLayout()
        {
            var result = ezArrangeViewModel.ConvertToLayout(15, 'B');
            Assert.That(result, Is.EqualTo(0));

            result = ezArrangeViewModel.ConvertToLayout(1, '0');
            Assert.That(result, Is.EqualTo(0));

            result = ezArrangeViewModel.ConvertToLayout(1, 'B');
            Assert.That(result, Is.Not.EqualTo(0));
        }

        [Test]
        public void TestParseFromLayout()
        {
            var result = ezArrangeViewModel.ParseFromLayout(100);
            Assert.AreNotEqual(null, result);
        }

        [Test]
        public void TestProgressValue()
        {
            ezArrangeViewModel.ProgressValue = 12;
            Assert.That(ezArrangeViewModel.ProgressValue, Is.EqualTo(12));
        }

        [Test]
        public void TesCurrentAnimationPage()
        {
            ezArrangeViewModel.CurrentAnimationPage = 12;
            Assert.That(ezArrangeViewModel.CurrentAnimationPage, Is.EqualTo(12));
        }

        [Test]
        public void TesMainText()
        {
            ezArrangeViewModel.MainText = "MainText";
            Assert.That(ezArrangeViewModel.MainText, Is.EqualTo("MainText"));
        }

        [Test]
        public void TesSubText()
        {
            ezArrangeViewModel.SubText = "SubText";
            Assert.That(ezArrangeViewModel.SubText, Is.EqualTo("SubText"));
        }

        [Test]
        public void TestGetEzPages()
        {
            var result = ezArrangeViewModel.GetEzPages();
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void TestCurrentSelectspItem()
        {
            //_selectedSplitItem != null
            var currentSelectspItem = new SplitItem() { Buddy = new SplitItem() };
            ezArrangeViewModel.CurrentSelectspItem = currentSelectspItem;
            // Assert
            Assert.That(ezArrangeViewModel.CurrentSelectspItem, Is.EqualTo(currentSelectspItem));
        }

        [Test]
        public void TestCurrentEditSelectspItem()
        {
            //_selectedSplitItem != null
            var currentEditSelectspItem = new SplitItem() { Buddy = new SplitItem() };
            ezArrangeViewModel.CurrentEditSelectspItem = currentEditSelectspItem;
            // Assert
            Assert.That(ezArrangeViewModel.CurrentEditSelectspItem, Is.EqualTo(currentEditSelectspItem));
        }

        [Test]
        public void TestIsEditProfile()
        {
            ezArrangeViewModel.IsEditProfile = true;
            Assert.That(ezArrangeViewModel.IsEditProfile, Is.EqualTo(true));
        }

        [Test]
        public void TestIsAddPageBack()
        {
            ezArrangeViewModel.IsAddPageBack = true;
            Assert.That(ezArrangeViewModel.IsAddPageBack, Is.EqualTo(true));
        }

        [Test]
        public void TestInputText()
        {
            ezArrangeViewModel.InputText = "InputText";
            Assert.That(ezArrangeViewModel.InputText, Is.EqualTo("InputText"));
        }

        [Test]
        public void TestProfileTitleTextBlockValue()
        {
            ezArrangeViewModel.ProfileTitleTextBlockValue = "ProfileTitleTextBlockValue";
            Assert.That(ezArrangeViewModel.ProfileTitleTextBlockValue, Is.EqualTo("ProfileTitleTextBlockValue"));
        }

        [Test]
        public void TestAutomaticStartupValue()
        {
            ezArrangeViewModel.AutomaticStartupValue = "AutomaticStartupValue";
            Assert.That(ezArrangeViewModel.AutomaticStartupValue, Is.EqualTo("AutomaticStartupValue"));
        }

        [Test]
        public void TestLaunchByTimeValue()
        {
            ezArrangeViewModel.LaunchByTimeValue = "LaunchByTimeValue";
            Assert.That(ezArrangeViewModel.LaunchByTimeValue, Is.EqualTo("LaunchByTimeValue"));
        }

        [Test]
        public void TestAppDocumentValue()
        {
            ezArrangeViewModel.AppDocumentValue = "AppDocumentValue";
            Assert.That(ezArrangeViewModel.AppDocumentValue, Is.EqualTo("AppDocumentValue"));
        }

        [Test]
        public void TestLeaveHoverState()
        {
            ezArrangeViewModel.currentUXTextBoxInfo = new Dell.Client.Framework.UX.WPF.Controls.UXTextBox();
            try
            {
                ezArrangeViewModel.currentUXTextBoxInfo = new Dell.Client.Framework.UX.WPF.Controls.UXTextBox() { AcceptsReturn = true };
                ezArrangeViewModel.UpdateTextBlockAppName("AddButton2_1", "Window2_1AppName");
                //Assert.That(ezArrangeViewModel.Window2_1AppName, Is.EqualTo("..."));
                //ezArrangeViewModel.UpdateTextBlockAppName("AddButton2_2", "Window2_2AppName");
                //Assert.That(ezArrangeViewModel.Window2_2AppName, Is.EqualTo("Window2_2AppName"));
                //ezArrangeViewModel.UpdateTextBlockAppName("AddButton1", "Window1AppName");
                //Assert.That(ezArrangeViewModel.Window1AppName, Is.EqualTo("Window1AppName"));
                //ezArrangeViewModel.UpdateTextBlockAppName("AddButton2", "Window2AppName");
                //Assert.That(ezArrangeViewModel.Window2AppName, Is.EqualTo("Window2AppName"));
                //ezArrangeViewModel.UpdateTextBlockAppName("AddButton3", "Window3AppName");
                //Assert.That(ezArrangeViewModel.Window3AppName, Is.EqualTo("Window3AppName"));
                //ezArrangeViewModel.UpdateTextBlockAppName("AddButton4", "Window4AppName");
                //Assert.That(ezArrangeViewModel.Window4AppName, Is.EqualTo("Window4AppName"));
                //ezArrangeViewModel.UpdateTextBlockAppName("AddButton5", "Window5AppName");
                //Assert.That(ezArrangeViewModel.Window5AppName, Is.EqualTo("Window5AppName"));
                //ezArrangeViewModel.UpdateTextBlockAppName("AddButton6", "Window6AppName");
                //Assert.That(ezArrangeViewModel.Window6AppName, Is.EqualTo("Window6AppName"));
                //ezArrangeViewModel.UpdateTextBlockAppName("AddButton7", "Window7AppName");
                //Assert.That(ezArrangeViewModel.Window7AppName, Is.EqualTo("Window7AppName"));
                //ezArrangeViewModel.UpdateTextBlockAppName("AddButton8", "Window8AppName");
                //Assert.That(ezArrangeViewModel.Window8AppName, Is.EqualTo("Window8AppName"));
                //ezArrangeViewModel.UpdateTextBlockAppName("AddButton9", "Window9AppName");
                //Assert.That(ezArrangeViewModel.Window9AppName, Is.EqualTo("Window9AppName"));
                //ezArrangeViewModel.UpdateTextBlockAppName("AddButton10", "Window10AppName");
                //Assert.That(ezArrangeViewModel.Window10AppName, Is.EqualTo("Window10AppName"));
                //ezArrangeViewModel.UpdateTextBlockAppName("AddButton11", "Window11AppName");
                //Assert.That(ezArrangeViewModel.Window11AppName, Is.EqualTo("Window11AppName"));
                //ezArrangeViewModel.UpdateTextBlockAppName("AddButton12", "Window12AppName");
                //Assert.That(ezArrangeViewModel.Window12AppName, Is.EqualTo("Window12AppName"));
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
        }

        [Test]
        public void TestClearTextBlockAppName()
        {
            try
            {
                ezArrangeViewModel.ClearTextBlockAppName();
                Assert.That(ezArrangeViewModel.Window2_1AppName, Is.EqualTo(""));
                Assert.That(ezArrangeViewModel.Window2_2AppName, Is.EqualTo(""));
                Assert.That(ezArrangeViewModel.Window1AppName, Is.EqualTo(""));
                Assert.That(ezArrangeViewModel.Window2AppName, Is.EqualTo(""));
                Assert.That(ezArrangeViewModel.Window3AppName, Is.EqualTo(""));
                Assert.That(ezArrangeViewModel.Window4AppName, Is.EqualTo(""));
                Assert.That(ezArrangeViewModel.Window5AppName, Is.EqualTo(""));
                Assert.That(ezArrangeViewModel.Window6AppName, Is.EqualTo(""));
                Assert.That(ezArrangeViewModel.Window7AppName, Is.EqualTo(""));
                Assert.That(ezArrangeViewModel.Window8AppName, Is.EqualTo(""));
                Assert.That(ezArrangeViewModel.Window9AppName, Is.EqualTo(""));
                Assert.That(ezArrangeViewModel.Window10AppName, Is.EqualTo(""));
                Assert.That(ezArrangeViewModel.Window11AppName, Is.EqualTo(""));
                Assert.That(ezArrangeViewModel.Window12AppName, Is.EqualTo(""));
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
        }

        [Test]
        public void TestIsRightGridPage2Visible()
        {
            ezArrangeViewModel.IsRightGridPage2Visible = true;
            Assert.That(ezArrangeViewModel.IsRightGridPage2Visible, Is.EqualTo(true));
        }

        [Test]
        public void TestSelectedValue()
        {
            ezArrangeViewModel.SelectedValue = 2;
            Assert.That(ezArrangeViewModel.SelectedValue, Is.EqualTo(2));
        }

        [Test]
        public void TestButtonName()
        {
            ezArrangeViewModel.ButtonName = "ButtonName";
            Assert.That(ezArrangeViewModel.ButtonName, Is.EqualTo("ButtonName"));
        }

        [Test]
        public void TestHourList()
        {
            var hourList = new List<string>();
            ezArrangeViewModel.HourList = hourList;
            Assert.That(ezArrangeViewModel.HourList, Is.EqualTo(hourList));
        }

        [Test]
        public void TestMinuteList()
        {
            var minuteList = new List<string>();
            ezArrangeViewModel.MinuteList = minuteList;
            Assert.That(ezArrangeViewModel.MinuteList, Is.EqualTo(minuteList));
        }

        [Test]
        public void TestAMPMList()
        {
            var aMPMList = new List<string>();
            ezArrangeViewModel.AMPMList = aMPMList;
            Assert.That(ezArrangeViewModel.AMPMList, Is.EqualTo(aMPMList));
        }

        [Test]
        public void TestSelectedHour()
        {
            ezArrangeViewModel.SelectedHour = "SelectedHour";
            Assert.That(ezArrangeViewModel.SelectedHour, Is.EqualTo("SelectedHour"));
        }

        [Test]
        public void TestSelectedMinute()
        {
            ezArrangeViewModel.SelectedMinute = "SelectedMinute";
            Assert.That(ezArrangeViewModel.SelectedMinute, Is.EqualTo("SelectedMinute"));
        }

        [Test]
        public void TestSelectedAMPM()
        {
            ezArrangeViewModel.SelectedAMPM = "SelectedAMPM";
            Assert.That(ezArrangeViewModel.SelectedAMPM, Is.EqualTo("SelectedAMPM"));
        }

        [Test]
        public void TestIsLaunchAtStartup()
        {
            ezArrangeViewModel.IsLaunchAtStartup = true;
            Assert.That(ezArrangeViewModel.IsLaunchAtStartup, Is.EqualTo(true));
        }

        [Test]
        public void TestIsManualLaunch()
        {
            ezArrangeViewModel.IsManualLaunch = true;
            Assert.That(ezArrangeViewModel.IsManualLaunch, Is.EqualTo(true));
            Assert.That(ezArrangeViewModel.IsAutoLaunch, Is.EqualTo(false));
        }

        [Test]
        public void TestIsAutoLaunch()
        {
            ezArrangeViewModel.IsAutoLaunch = true;
            Assert.That(ezArrangeViewModel.IsAutoLaunch, Is.EqualTo(true));
            Assert.That(ezArrangeViewModel.IsManualLaunch, Is.EqualTo(false));
        }


        //class EzMemoryPageData
        [Test]
        public void TestMainText()
        {
            var ezMemoryPageData = new EzMemoryPageData();
            ezMemoryPageData.MainText = "MainText";
            Assert.That(ezMemoryPageData.MainText, Is.EqualTo("MainText"));
        }

        [Test]
        public void TestSubText()
        {
            var ezMemoryPageData = new EzMemoryPageData();
            ezMemoryPageData.SubText = "SubText";
            Assert.That(ezMemoryPageData.SubText, Is.EqualTo("SubText"));
        }

        //class ApplicationItem
        [Test]
        public void TestAppName()
        {
            var applicationItem = new ApplicationItem();
            applicationItem.AppName = "AppName";
            Assert.That(applicationItem.AppName, Is.EqualTo("AppName"));
        }

        [Test]
        public void TestAppIcon()
        {
            var applicationItem = new ApplicationItem();
            applicationItem.AppIcon = "AppIcon";
            Assert.That(applicationItem.AppIcon, Is.EqualTo("AppIcon"));
        }

        [Test]
        public void TestAppPath()
        {
            var applicationItem = new ApplicationItem();
            applicationItem.AppPath = "AppPath";
            Assert.That(applicationItem.AppPath, Is.EqualTo("AppPath"));
        }
    }
}
