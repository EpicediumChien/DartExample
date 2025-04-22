using DDPM.SA.Common;
using DDPM.UI.Common;
using DDPM.UI.Common.Interfaces;
using DDPM.UI.Plugin.ViewModels;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.UX.WPF;
using Dell.Client.Framework.UX.WPF.ResourceManager;
using Moq;
using NGA.UnitTest.PrivateObject;
using System.Windows;

namespace DDPM.UI.Module.SpeakerAudioPreset.Tests
{
    [TestFixture, Apartment(ApartmentState.STA)]
    public class SpeakerAudioPresetRightViewTests
    {
        private SpeakerAudioPresetRightView? speakerAudioPresetRightView;
        private PrivateObject? privateObject;
        private Mock<IModuleOwner>? moduleOwnerMock;
        private Mock<IDeviceManagerSA>? deviceManagerMock;
        private IDeviceManagerSA? deviceManager;
        private Mock<IConsole>? consoleMock;
        private IConsole? console;
        private Mock<ILog>? logMock;
        private Mock<IShowPluginManager>? showPluginManagerMock;
        private IShowPluginManager? showPluginManager;
        private ILog? log;
        private SoundBarViewModel? vm;
        private DeviceInfo? CurrentDeviceInfo;

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
            moduleOwnerMock = new Mock<IModuleOwner>();
            var moduleOwner = moduleOwnerMock!.Object;
            DdpmCommonHelper.ModuleOwner = moduleOwner;
            deviceManagerMock = new Mock<IDeviceManagerSA>();
            deviceManager = deviceManagerMock.Object;
            DdpmCommonHelper.DeviceManagerSA = deviceManager;
            consoleMock = new Mock<IConsole>();
            console = consoleMock.Object;
            DdpmCommonHelper.MyConsole = console;
            logMock = new Mock<ILog>();
            log = logMock.Object;
            showPluginManagerMock = new Mock<IShowPluginManager>();
            showPluginManager = showPluginManagerMock.Object;
            vm = new SoundBarViewModel(console, log, deviceManager);
        }

        [Test]
        public void TestConstructor_SpeakerAudioPresetRightView()
        {
            CurrentDeviceInfo = new DeviceInfo() { IsWiredAudioIMicNSEnable = true, IsWiredAudioMicMuteSoundEnable = true, WiredAudioVolumeAdjustmentTone = 1 };
            vm.CurrentDeviceInfo = CurrentDeviceInfo;
            deviceManagerMock.Setup(x => x.GetProfileAsync(It.IsAny<string>())).Returns(Task.FromResult("SpeakerProfile"));
            speakerAudioPresetRightView = new SpeakerAudioPresetRightView(vm);
            privateObject = new PrivateObject(speakerAudioPresetRightView);
            // Assert
            Assert.That(speakerAudioPresetRightView, Is.Not.Null);
            Assert.That(privateObject!.GetFieldOrProperty("_vm"), Is.EqualTo(vm));
        }

        //[Test]
        //public void TestSetNodeValue()
        //{
        //    var node = new Image();
        //    try
        //    {
        //        speakerAudioPresetRightView.SetNodeValue(node, 2.0);
        //        Assert.True(true);
        //    }
        //    catch (Exception ex)
        //    {
        //        Assert.Fail("not invoked");
        //    }
        //}

        //class CollaborationCheckedToVisibilityConverter
        //[Test]
        //public void TestConstructor_CollaborationCheckedToVisibilityConverter()
        //{
        //    CollaborationCheckedToVisibilityConverter collaborationCheckedToVisibilityConverter = new CollaborationCheckedToVisibilityConverter();
        //    Assert.That(collaborationCheckedToVisibilityConverter, Is.Not.Null);
        //}

        //[Test]
        //public void TestViewModel()
        //{
        //    CollaborationCheckedToVisibilityConverter collaborationCheckedToVisibilityConverter = new CollaborationCheckedToVisibilityConverter();
        //    var viewModel = new HeadsetViewModel(console, log, deviceManager);
        //    collaborationCheckedToVisibilityConverter.ViewModel = viewModel;
        //    Assert.That(collaborationCheckedToVisibilityConverter.ViewModel, Is.EqualTo(viewModel));
        //}

        //[Test]
        //public void TestConvert()
        //{
        //    CollaborationCheckedToVisibilityConverter collaborationCheckedToVisibilityConverter = new CollaborationCheckedToVisibilityConverter();
        //    var res = collaborationCheckedToVisibilityConverter.Convert(null, null, null, null);
        //    Assert.That(res, Is.EqualTo(Visibility.Collapsed));

        //    //var parameter == "",isChecked && ViewModel.Model == "WL7024"
        //    var parameter = "";
        //    var viewModel = new HeadsetViewModel(console, log, deviceManager);
        //    viewModel.Model = "WL7024";
        //    collaborationCheckedToVisibilityConverter.ViewModel = viewModel;
        //    res = collaborationCheckedToVisibilityConverter.Convert(true, null, parameter, null);
        //    Assert.That(res, Is.EqualTo(Visibility.Collapsed));

        //    //parameter == "MicNoiseCancellationPageShow"&&isChecked,viewModel.Model == "WL7024"
        //    parameter = "MicNoiseCancellationPageShow";
        //    viewModel = new HeadsetViewModel(console, log, deviceManager);
        //    viewModel.Model = "WL7024";
        //    collaborationCheckedToVisibilityConverter.ViewModel = viewModel;
        //    res = collaborationCheckedToVisibilityConverter.Convert(true, null, parameter, null);
        //    Assert.That(res, Is.EqualTo(Visibility.Visible));

        //    //parameter=="",isChecked && ViewModel.Model != "WL7024"
        //    parameter = "";
        //    collaborationCheckedToVisibilityConverter.ViewModel = new HeadsetViewModel(console, log, deviceManager);
        //    res = collaborationCheckedToVisibilityConverter.Convert(true, null, parameter, null);
        //    Assert.That(res, Is.EqualTo(Visibility.Collapsed));

        //    //parameter=="MicNoiseCancellationFewPageShow",isChecked && ViewModel.Model != "WL7024"
        //    parameter = "MicNoiseCancellationFewPageShow";
        //    collaborationCheckedToVisibilityConverter.ViewModel = new HeadsetViewModel(console, log, deviceManager);
        //    res = collaborationCheckedToVisibilityConverter.Convert(true, null, parameter, null);
        //    Assert.That(res, Is.EqualTo(Visibility.Visible));
        //}

        //class ValueToVisibilityConverter
        //[Test]
        //public void ValueToVisibilityConvert()
        //{
        //    ValueToVisibilityConverter valueToVisibilityConverter = new ValueToVisibilityConverter();
        //    //value==null,parameter==null
        //    var res = valueToVisibilityConverter.Convert(null, null, null, null);
        //    Assert.That(res, Is.EqualTo(Visibility.Collapsed));

        //    //value==parameter
        //    var parameter = "1";
        //    res = valueToVisibilityConverter.Convert(1, null, parameter, null);
        //    Assert.That(res, Is.EqualTo(Visibility.Visible));

        //    //value!=parameter
        //    res = valueToVisibilityConverter.Convert(2, null, parameter, null);
        //    Assert.That(res, Is.EqualTo(Visibility.Collapsed));
        //}

        ////class InverseBooleanConverter
        //[Test]
        //public void TestInverseBooleanConvert()
        //{
        //    InverseBooleanConverter inverseBooleanConverter = new InverseBooleanConverter();
        //    //value is bool boolean
        //    var res = inverseBooleanConverter.Convert(false, null, null, null);
        //    Assert.That(res, Is.EqualTo(true));

        //    //value is not bool boolean
        //    res = inverseBooleanConverter.Convert(null, null, null, null);
        //    Assert.That(res, Is.EqualTo(false));

        //}

        //[Test]
        //public void TestInverseBooleanConvertBack()
        //{
        //    InverseBooleanConverter inverseBooleanConverter = new InverseBooleanConverter();
        //    //value is bool boolean
        //    var res = inverseBooleanConverter.ConvertBack(false, null, null, null);
        //    Assert.That(res, Is.EqualTo(true));

        //    //value is not bool boolean
        //    res = inverseBooleanConverter.ConvertBack(null, null, null, null);
        //    Assert.That(res, Is.EqualTo(false));
        //}

        ////class BooleanToInverseForegroundConverter
        //[Test]
        //public void BooleanToInverseForegroundConvert()
        //{
        //    BooleanToInverseForegroundConverter booleanToInverseForegroundConverter = new BooleanToInverseForegroundConverter();
        //    //value==true
        //    var res = booleanToInverseForegroundConverter.Convert(true, null, null, null);
        //    Assert.That(res, Is.EqualTo(Brushes.Gray));

        //    //value==false
        //    res = booleanToInverseForegroundConverter.Convert(false, null, null, null);
        //    Assert.That(res, Is.EqualTo(Brushes.White));

        //    //value is not bool boolean
        //    res = booleanToInverseForegroundConverter.Convert(null, null, null, null);
        //    Assert.That(res, Is.EqualTo(Brushes.White));
        //}
    }
}