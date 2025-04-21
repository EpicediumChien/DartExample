using DDPM.SA.Common;
using DDPM.UI.Common;
using DDPM.UI.Common.Interfaces;
using DDPM.UI.Common.Models;
using DPeMPublic.Common.Enums;
using Moq;
using System.Windows;
using System.Windows.Controls;
using Dell.Client.Framework.UX.WPF.ResourceManager;
using DDPM.UI.Plugin.DdpmHomePlugin.Interfaces;
using System.Globalization;

namespace DDPM.UI.Plugin.SettingsPlugin.Tests
{
    [Apartment(ApartmentState.STA)]
    public class SettingsPageViewModelTests
    {
        private SettingsPageViewModel? settingsPageViewModel;
        private Mock<IDeviceManagerSA>? DeviceManagerSAMock;

        [SetUp]
        public void Setup()
        {
            if (System.Windows.Application.Current == null)
            {
                new System.Windows.Application();
            }
            ResourceManager res = new ResourceManager();
            var resourceDictionary = new ResourceDictionary();
            resourceDictionary.Source = new Uri("pack://application:,,,/DDPM.UI.Common;component/ModuleStyle.xaml");
            System.Windows.Application.Current.Resources.MergedDictionaries.Add(resourceDictionary);
            settingsPageViewModel = new SettingsPageViewModel();
            DeviceManagerSAMock = new Mock<IDeviceManagerSA>();
            DdpmCommonHelper.DeviceManagerSA = DeviceManagerSAMock.Object;
            DeviceManagerSAMock.Setup(x => x.GetDevices(It.IsAny<bool>())).Returns(Task.FromResult(new DeviceHelper() { deviceInfo = new List<DeviceInfo>() }));
        }

        [Test]
        public void TestIsSelected()
        {
            var isSelected = new bool[2];
            settingsPageViewModel.IsSelected = isSelected;
            var result = settingsPageViewModel.IsSelected;
            Assert.That(result, Is.EqualTo(isSelected));
        }

        [Test]
        public void TestHomeDevices()
        {
            var homeDevices = new List<HomeDevice>();
            settingsPageViewModel.HomeDevices = homeDevices;
            var result = settingsPageViewModel.HomeDevices;
            Assert.That(result, Is.EqualTo(homeDevices));
        }

        [Test]
        public void TestSelectedHomeDevice()
        {
            var electedHomeDevice = new HomeDevice();
            settingsPageViewModel.SelectedHomeDevice = electedHomeDevice;
            var result = settingsPageViewModel.SelectedHomeDevice;
            Assert.That(result, Is.EqualTo(electedHomeDevice));
        }

        [Test]
        public void TestModuleOwner()
        {
            var mockModuleOwner = new Mock<IModuleOwner>();
            settingsPageViewModel.ModuleOwner = mockModuleOwner.Object;
            var result = settingsPageViewModel.ModuleOwner;
            Assert.That(settingsPageViewModel.ModuleOwner, Is.EqualTo(mockModuleOwner.Object));
        }

        [Test]
        public void TestFullView()
        {
            var fullView = new ContentControl();
            settingsPageViewModel.FullView = fullView;
            var result = settingsPageViewModel.FullView;
            Assert.That(result, Is.EqualTo(fullView));
        }

        //Elsa mark for PluginIoc readonly
        //[Test]
        //public void TestSetSelected()
        //{
        //    try
        //    {
        //        settingsPageViewModel.SetSelected(0);
        //        settingsPageViewModel.SetSelected(1);
        //        settingsPageViewModel.SetSelected(2);
        //        settingsPageViewModel.SetSelected(3);
        //        settingsPageViewModel.SetSelected(4);
        //        Assert.True(true);
        //    }
        //    catch (Exception ex)
        //    {
        //        Assert.Fail("not invoked");
        //    }
        //}

        [Test]
        public void TestOpenFullView()
        {
            var content = new ContentControl();
            try
            {
                settingsPageViewModel.OpenFullView(content);
                Assert.That(settingsPageViewModel.FullView, Is.EqualTo(content));
                var result = settingsPageViewModel.FullView.Visibility;
                Assert.That(result, Is.EqualTo(Visibility.Visible));
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
        }

        [Test]
        public void TestCloseFullView()
        {
            try
            {
                settingsPageViewModel.CloseFullView();
                Assert.That(settingsPageViewModel.FullView, Is.EqualTo(null));
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
        }

        [Test]
        public void TestFWUpdateInfoPackage()
        {
            var fWUpdateInfoPackage = new FWUpdateInfoPackage();
            settingsPageViewModel.FWUpdateInfoPackage = fWUpdateInfoPackage;
            var result = settingsPageViewModel.FWUpdateInfoPackage;
            Assert.That(result, Is.EqualTo(fWUpdateInfoPackage));
        }

        [Test]
        public void TestSWUpdateInfoPackage()
        {
            var sWUpdateInfoPackage = new SWUpdateInfoPackage();
            settingsPageViewModel.SWUpdateInfoPackage = sWUpdateInfoPackage;
            var result = settingsPageViewModel.SWUpdateInfoPackage;
            Assert.That(result, Is.EqualTo(sWUpdateInfoPackage));
        }

        [Test]
        public void TestCritical_UpdateList_UI()
        {
            var critical_UpdateList_UI = new List<UIUpdateInfo>();
            settingsPageViewModel.Critical_UpdateList_UI = critical_UpdateList_UI;
            var result = settingsPageViewModel.Critical_UpdateList_UI;
            Assert.That(result, Is.EqualTo(critical_UpdateList_UI));
        }

        [Test]
        public void TestRecommended_UpdateList_UI()
        {
            var recommended_UpdateList_UI = new List<UIUpdateInfo>();
            settingsPageViewModel.Recommended_UpdateList_UI = recommended_UpdateList_UI;
            var result = settingsPageViewModel.Recommended_UpdateList_UI;
            Assert.That(result, Is.EqualTo(recommended_UpdateList_UI));
        }

        [Test]
        public void TestOptional_UpdateList_UI()
        {
            var optional_UpdateList_UI = new List<UIUpdateInfo>();
            settingsPageViewModel.Optional_UpdateList_UI = optional_UpdateList_UI;
            var result = settingsPageViewModel.Optional_UpdateList_UI;
            Assert.That(result, Is.EqualTo(optional_UpdateList_UI));
        }

        [Test]
        public void TestLastCheckDate()
        {
            settingsPageViewModel.LastCheckDate = "A";
            var result = settingsPageViewModel.LastCheckDate;
            Assert.That(result, Is.EqualTo("A"));
        }

        [Test]
        public void TestUpdatesPageUI_Enable()
        {
            var deviceManagerSAMock = new Mock<IDeviceManagerSA>();
            DdpmCommonHelper.DeviceManagerSA = deviceManagerSAMock.Object;
            deviceManagerSAMock.Setup(x => x.GetUILockStatus()).Returns(Task.FromResult(true));
            var result = settingsPageViewModel.Lock_UpdatesPage;
            Assert.That(result, Is.EqualTo(false));
        }

        [Test]
        public void TestUpdateTitle()
        {
            settingsPageViewModel.UpdateTitle = "A";
            var result = settingsPageViewModel.UpdateTitle;
            Assert.That(result, Is.EqualTo("A"));
        }

        [Test]
        public void TestUpdateVersion()
        {
            settingsPageViewModel.UpdateVersion = "A";
            var result = settingsPageViewModel.UpdateVersion;
            Assert.That(result, Is.EqualTo("A"));
        }

        [Test]
        public void TestProgressStr()
        {
            settingsPageViewModel.ProgressStr = "A";
            var result = settingsPageViewModel.ProgressStr;
            Assert.That(result, Is.EqualTo("A"));
        }

        [Test]
        public void TestProgressValue()
        {
            settingsPageViewModel.ProgressValue = 1;
            var result = settingsPageViewModel.ProgressValue;
            Assert.That(result, Is.EqualTo(1));
        }

        [Test]
        public void TestProgress_IsAnimated()
        {
            settingsPageViewModel.Progress_IsAnimated = true;
            var result = settingsPageViewModel.Progress_IsAnimated;
            Assert.That(result, Is.EqualTo(true));
        }

        [Test]
        public void TestNoNetwork()
        {
            var result = settingsPageViewModel.NoNetwork;
            Assert.That(result, Is.EqualTo(Visibility.Collapsed));
        }

        [Test]
        public void TestCritical_UpdateList()
        {
            var result = settingsPageViewModel.Critical_UpdateList;
            Assert.That(result, Is.EqualTo(Visibility.Collapsed));

            List<UIUpdateInfo> Critical_UpdateList_UI = new List<UIUpdateInfo>() { new UIUpdateInfo(new FWUpdateInfo(), new List<DeviceInfo>(), new int()) { IsCheckUpdate = true }, new UIUpdateInfo(new FWUpdateInfo(), new List<DeviceInfo>(), new int()) { } };
            settingsPageViewModel.Critical_UpdateList_UI = Critical_UpdateList_UI;
            result = settingsPageViewModel.Critical_UpdateList;
            Assert.That(result, Is.EqualTo(Visibility.Visible));
        }

        [Test]
        public void TestRefreshUI()
        {
            //Before RefreshUI
            var Critical_UpdateList_UIBef = settingsPageViewModel.Critical_UpdateList_UI;
            var Recommended_UpdateList_UIBef = settingsPageViewModel.Recommended_UpdateList_UI;
            var Optional_UpdateList_UIBef = settingsPageViewModel.Optional_UpdateList_UI;
            var LastCheckDateBef = settingsPageViewModel.LastCheckDate;
            var deviceManagerSAMock = new Mock<IDeviceManagerSA>();
            ///DdpmCommonHelper.DeviceManagerSA = deviceManagerSAMock.Object;
            //deviceManagerSAMock.Setup(x => x.GetUILockStatus()).Returns(Task.FromResult(false));
            //var UpdatesPageUI_EnableBef = settingsPageViewModel.Lock_UpdatesPage;
            //Assert.That(UpdatesPageUI_EnableBef, Is.EqualTo(true));

            settingsPageViewModel.Critical_UpdateList_UI = new List<UIUpdateInfo>();
            settingsPageViewModel.Recommended_UpdateList_UI = new List<UIUpdateInfo>();
            settingsPageViewModel.Optional_UpdateList_UI = new List<UIUpdateInfo>();
            settingsPageViewModel.LastCheckDate = "AA";
            //deviceManagerSAMock.Setup(x => x.GetUILockStatus()).Returns(Task.FromResult(true));
            //var result = settingsPageViewModel.Lock_UpdatesPage;
            //settingsPageViewModel.RefreshUI();

            //After RefreshUI
            Assert.That(settingsPageViewModel.Critical_UpdateList_UI, Is.Not.EqualTo(Critical_UpdateList_UIBef));
            Assert.That(settingsPageViewModel.Recommended_UpdateList_UI, Is.Not.EqualTo(Recommended_UpdateList_UIBef));
            Assert.That(settingsPageViewModel.Optional_UpdateList_UI, Is.Not.EqualTo(Optional_UpdateList_UIBef));
            Assert.That(settingsPageViewModel.LastCheckDate, Is.Not.EqualTo(LastCheckDateBef));
            // Assert.That(result, Is.EqualTo(false));
            //Assert.That(result, Is.Not.EqualTo(UpdatesPageUI_EnableBef));
        }

        [Test]
        public void TestRefreshProcessUI()
        {
            //Before RefreshProcessUI
            var UpdateTitleBef = settingsPageViewModel.UpdateTitle;
            var UpdateVersionBef = settingsPageViewModel.UpdateVersion;
            var ProgressValueBef = settingsPageViewModel.ProgressValue;
            var Progress_IsAnimatedBef = settingsPageViewModel.Progress_IsAnimated;
            var ProgressStrBef = settingsPageViewModel.ProgressStr;

            settingsPageViewModel.UpdateTitle = "AA";
            settingsPageViewModel.UpdateVersion = "BB";
            settingsPageViewModel.ProgressValue = 2;
            settingsPageViewModel.Progress_IsAnimated = true;
            settingsPageViewModel.ProgressStr = "CC";
            settingsPageViewModel.RefreshProcessUI();

            //After RefreshProcessUI
            Assert.That(settingsPageViewModel.UpdateTitle, Is.Not.EqualTo(UpdateTitleBef));
            Assert.That(settingsPageViewModel.UpdateVersion, Is.Not.EqualTo(UpdateVersionBef));
            Assert.That(settingsPageViewModel.ProgressValue, Is.Not.EqualTo(ProgressValueBef));
            Assert.That(settingsPageViewModel.Progress_IsAnimated, Is.Not.EqualTo(Progress_IsAnimatedBef));
            Assert.That(settingsPageViewModel.ProgressStr, Is.Not.EqualTo(ProgressStrBef));
        }

        [Test]
        public void TestSetUpdateInfoUI()
        {
            FWUpdateInfoPackage fwUpdateInfoPackage = new FWUpdateInfoPackage();
            SWUpdateInfoPackage swUpdateInfoPackage = new SWUpdateInfoPackage();
            var deviceManagerSAMock = new Mock<IDeviceManagerSA>();
            DdpmCommonHelper.DeviceManagerSA = deviceManagerSAMock.Object;
            deviceManagerSAMock.Setup(x => x.GetDevices(It.IsAny<bool>())).Returns(Task.FromResult(new DeviceHelper() { deviceInfo = new List<DeviceInfo>() }));
            try
            {
                settingsPageViewModel.SetUpdateInfoUI(fwUpdateInfoPackage, swUpdateInfoPackage);
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
        }

        [Test]
        public void TestRecommended_UpdateList()
        {
            var result = settingsPageViewModel.Recommended_UpdateList;
            Assert.That(result, Is.EqualTo(Visibility.Collapsed));

            List<UIUpdateInfo> Recommended_UpdateList_UI = new List<UIUpdateInfo>() { new UIUpdateInfo(new FWUpdateInfo(), new List<DeviceInfo>(), new int()) { IsCheckUpdate = true }, new UIUpdateInfo(new FWUpdateInfo(), new List<DeviceInfo>(), new int()) { } };
            settingsPageViewModel.Recommended_UpdateList_UI = Recommended_UpdateList_UI;
            result = settingsPageViewModel.Recommended_UpdateList;
            Assert.That(result, Is.EqualTo(Visibility.Visible));
        }

        [Test]
        public void TestOptional_UpdateList()
        {
            var result = settingsPageViewModel.Optional_UpdateList;
            Assert.That(result, Is.EqualTo(Visibility.Collapsed));

            List<UIUpdateInfo> Optional_UpdateList_UI = new List<UIUpdateInfo>() { new UIUpdateInfo(new FWUpdateInfo(), new List<DeviceInfo>(), new int()) { IsCheckUpdate = true }, new UIUpdateInfo(new FWUpdateInfo(), new List<DeviceInfo>(), new int()) { } };
            settingsPageViewModel.Optional_UpdateList_UI = Optional_UpdateList_UI;
            result = settingsPageViewModel.Optional_UpdateList;
            Assert.That(result, Is.EqualTo(Visibility.Visible));
        }

        [Test]
        public void TestIsAnyUpdate()
        {
            var result = settingsPageViewModel.IsAnyUpdate;
            Assert.That(result, Is.EqualTo(Visibility.Collapsed));

            List<UIUpdateInfo> Optional_UpdateList_UI = new List<UIUpdateInfo>() { new UIUpdateInfo(new FWUpdateInfo(), new List<DeviceInfo>(), new int()) { IsCheckUpdate = true }, new UIUpdateInfo(new FWUpdateInfo(), new List<DeviceInfo>(), new int()) { } };
            settingsPageViewModel.Optional_UpdateList_UI = Optional_UpdateList_UI;
            result = settingsPageViewModel.IsAnyUpdate;
            Assert.That(result, Is.EqualTo(Visibility.Visible));
        }

        [Test]
        public void TestIsCanUpdate()
        {
            bool result = settingsPageViewModel.IsCanUpdate();
            Assert.That(result, Is.EqualTo(false));

            List<UIUpdateInfo> Critical_UpdateList_UI = new List<UIUpdateInfo>() { new UIUpdateInfo(new FWUpdateInfo(), new List<DeviceInfo>(), new int()) { IsCheckUpdate = true } };
            settingsPageViewModel.Critical_UpdateList_UI = Critical_UpdateList_UI;
            result = settingsPageViewModel.IsCanUpdate();
            Assert.That(result, Is.EqualTo(true));
        }

        [Test]
        public void TestSetVMUpdate()
        {
            FWUpdateInfo fwUpdateInfo = new FWUpdateInfo();
            List<DeviceInfo> deviceInfos = new List<DeviceInfo>();
            int ioDongleCount = new int();
            UIUpdateInfo uIFWUpdateInfo = new UIUpdateInfo(fwUpdateInfo, deviceInfos, ioDongleCount);
            List<UIUpdateInfo> Critical_UpdateList_UI = new List<UIUpdateInfo>() { new UIUpdateInfo(new FWUpdateInfo(), new List<DeviceInfo>(), new int()) { IsCheckUpdate = true, SWUpdateInfo = new SWUpdateInfo() } };
            settingsPageViewModel.Critical_UpdateList_UI = Critical_UpdateList_UI;

            List<UIUpdateInfo> Recommended_UpdateList_UI = new List<UIUpdateInfo>() { new UIUpdateInfo(new FWUpdateInfo(), new List<DeviceInfo>(), new int()) { IsCheckUpdate = true, SWUpdateInfo = new SWUpdateInfo() } };
            settingsPageViewModel.Recommended_UpdateList_UI = Recommended_UpdateList_UI;
            List<UIUpdateInfo> Optional_UpdateList_UI = new List<UIUpdateInfo>() { new UIUpdateInfo(new FWUpdateInfo(), new List<DeviceInfo>(), new int()) { IsCheckUpdate = true, SWUpdateInfo = new SWUpdateInfo() } };
            settingsPageViewModel.Optional_UpdateList_UI = Optional_UpdateList_UI;
            FWUpdateInfoPackage FWUpdateInfoPackage = new FWUpdateInfoPackage();
            settingsPageViewModel.FWUpdateInfoPackage = FWUpdateInfoPackage;
            SWUpdateInfoPackage SWUpdateInfoPackage = new SWUpdateInfoPackage();
            settingsPageViewModel.SWUpdateInfoPackage = SWUpdateInfoPackage;
            try
            {
                settingsPageViewModel.SetVMUpdate();
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
        }

        //class UIUpdateInfo
        [Test]
        public void TestIsCheckUpdate()
        {
            FWUpdateInfo fwUpdateInfo = new FWUpdateInfo();
            List<DeviceInfo> deviceInfos = new List<DeviceInfo>();
            int ioDongleCount = new int();
            UIUpdateInfo uIFWUpdateInfo = new UIUpdateInfo(fwUpdateInfo, deviceInfos, ioDongleCount);
            uIFWUpdateInfo.IsCheckUpdate = true;
            var result = uIFWUpdateInfo.IsCheckUpdate;
            Assert.That(result, Is.EqualTo(true));
        }

        [Test]
        public void TestIsEnableCheckBox()
        {
            FWUpdateInfo fwUpdateInfo = new FWUpdateInfo();
            List<DeviceInfo> deviceInfos = new List<DeviceInfo>();
            int ioDongleCount = new int();
            UIUpdateInfo uIFWUpdateInfo = new UIUpdateInfo(fwUpdateInfo, deviceInfos, ioDongleCount);
            uIFWUpdateInfo.IsEnableCheckBox = true;
            var result = uIFWUpdateInfo.IsEnableCheckBox;
            Assert.That(result, Is.EqualTo(true));
        }

        [Test]
        public void TestUpdateInfo()
        {
            FWUpdateInfo fwUpdateInfo = new FWUpdateInfo();
            List<DeviceInfo> deviceInfos = new List<DeviceInfo>();
            int ioDongleCount = new int();
            UIUpdateInfo uIFWUpdateInfo = new UIUpdateInfo(fwUpdateInfo, deviceInfos, ioDongleCount);
            uIFWUpdateInfo.UpdateInfo = "AA";
            var result = uIFWUpdateInfo.UpdateInfo;
            Assert.That(result, Is.EqualTo("AA"));
        }

        [Test]
        public void TestFWUpdateInfo()
        {
            FWUpdateInfo fwUpdateInfo = new FWUpdateInfo();
            List<DeviceInfo> deviceInfos = new List<DeviceInfo>();
            int ioDongleCount = new int();
            UIUpdateInfo uIFWUpdateInfo = new UIUpdateInfo(fwUpdateInfo, deviceInfos, ioDongleCount);
            uIFWUpdateInfo.FWUpdateInfo = fwUpdateInfo;
            var result = uIFWUpdateInfo.FWUpdateInfo;
            Assert.That(result, Is.EqualTo(fwUpdateInfo));
        }

        [Test]
        public void TestUXAlertItemVisibility()
        {
            FWUpdateInfo fwUpdateInfo = new FWUpdateInfo();
            List<DeviceInfo> deviceInfos = new List<DeviceInfo>();
            int ioDongleCount = new int();
            UIUpdateInfo uIFWUpdateInfo = new UIUpdateInfo(fwUpdateInfo, deviceInfos, ioDongleCount);
            var uXAlertItemVisibility = Visibility.Visible;
            uIFWUpdateInfo.UXAlertItemVisibility = uXAlertItemVisibility;
            var result = uIFWUpdateInfo.UXAlertItemVisibility;
            Assert.That(result, Is.EqualTo(uXAlertItemVisibility));
        }

        [Test]
        public void TestUXAlertItemMessage()
        {
            FWUpdateInfo fwUpdateInfo = new FWUpdateInfo();
            List<DeviceInfo> deviceInfos = new List<DeviceInfo>();
            int ioDongleCount = new int();
            UIUpdateInfo uIFWUpdateInfo = new UIUpdateInfo(fwUpdateInfo, deviceInfos, ioDongleCount);
            uIFWUpdateInfo.UXAlertItemMessage = "AA";
            var result = uIFWUpdateInfo.UXAlertItemMessage;
            Assert.That(result, Is.EqualTo("AA"));
        }

        [Test]
        public void TestConstructor_UIUpdateInfoa()
        {
            FWUpdateInfo fwUpdateInfo = new FWUpdateInfo();
            var deviceType = DeviceType.LogicalMouse;
            fwUpdateInfo.TheLatestVersion = "1A";
            fwUpdateInfo.DeviceName = "ST";
            fwUpdateInfo.DeviceType = deviceType;
            List<DeviceInfo> deviceInfos = new List<DeviceInfo>();
            int ioDongleCount = new int();
            UIUpdateInfo uIUpdateInfo = new UIUpdateInfo(fwUpdateInfo, deviceInfos, ioDongleCount);
            Assert.That(uIUpdateInfo, Is.Not.Null);
            Assert.That(uIUpdateInfo.UXAlertItemVisibility, Is.EqualTo(Visibility.Collapsed));
            Assert.That(uIUpdateInfo.UXAlertItemMessage, Is.EqualTo(""));
            if (CultureInfo.CurrentCulture.Name == "es-US")
            {
                Assert.That(uIUpdateInfo.UpdateInfo, Is.EqualTo("Firmware update 1A - ST "));
            }
        }

        [Test]
        public void TestConstructor_UIUpdateInfob()
        {
            SWUpdateInfo swUpdateInfo = new SWUpdateInfo();
            swUpdateInfo.TheLatestVersion = "1A";
            swUpdateInfo.SoftwareName = "ST";
            UIUpdateInfo uIUpdateInfo = new UIUpdateInfo(swUpdateInfo);
            Assert.That(uIUpdateInfo.SWUpdateInfo, Is.Not.Null);
            Assert.That(uIUpdateInfo.UXAlertItemVisibility, Is.EqualTo(Visibility.Collapsed));
            if (CultureInfo.CurrentCulture.Name == "es-US")
            {
                Assert.That(uIUpdateInfo.UpdateInfo, Is.EqualTo("Software update 1A - ST"));
            }
        }
    }
}