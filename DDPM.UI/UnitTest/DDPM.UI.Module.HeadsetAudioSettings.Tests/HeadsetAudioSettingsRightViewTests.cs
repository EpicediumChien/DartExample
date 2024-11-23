using DDPM.SA.Common;
using DDPM.UI.Common.Interfaces;
using DDPM.UI.Plugin.ViewModels;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.UX.WPF;
using Moq;
using NGA.UnitTest.PrivateObject;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DDPM.UI.Module.HeadsetAudioSettings.Tests
{
    [Apartment(ApartmentState.STA)]
    public class HeadsetAudioSettingsRightViewTests
    {
        private HeadsetAudioSettingsRightView? headsetAudioSettingsRightView;
        private HeadsetViewModel? vm;
        private Mock<ILog>? logMock;
        private ILog? log;
        private Mock<IShowPluginManager>? showPluginManagerMock;
        private IShowPluginManager? showPluginManager;
        private Mock<IDeviceManagerSA>? deviceManagerSAMock;
        private IDeviceManagerSA? deviceManagerSA;
        private Mock<IConsole>? consoleMock;
        private IConsole? console;
        private PrivateObject? privateObject;

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
            logMock = new Mock<ILog>();
            log = logMock.Object;
            deviceManagerSAMock = new Mock<IDeviceManagerSA>();
            deviceManagerSA = deviceManagerSAMock.Object;
            consoleMock = new Mock<IConsole>();
            console = consoleMock.Object;
            showPluginManagerMock = new Mock<IShowPluginManager>();
            showPluginManager = showPluginManagerMock.Object;
            vm = new HeadsetViewModel(showPluginManager, console, log, deviceManagerSA);
            var currentDeviceInfo = new DeviceInfo();
            vm.CurrentDeviceInfo = currentDeviceInfo;
            headsetAudioSettingsRightView = new HeadsetAudioSettingsRightView(vm);
            privateObject = new PrivateObject(headsetAudioSettingsRightView);
        }

        [Test]
        public void TestConstructor_HeadsetAudioSettingsModule()
        {
            // Act
            var _vm = privateObject.GetFieldOrProperty("_vm");

            // Assert
            Assert.That(headsetAudioSettingsRightView, Is.Not.Null);
            Assert.That(_vm, Is.EqualTo(vm));
        }

        [Test]
        public void TestSetNodeValue()
        {
            var node = new Image();
            try
            {
                headsetAudioSettingsRightView.SetNodeValue(node, 2.0);
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
        }

        //class CollaborationCheckedToVisibilityConverter
        [Test]
        public void TestViewModel()
        {
            // Act
            CollaborationCheckedToVisibilityConverter collaborationCheckedToVisibilityConverter = new CollaborationCheckedToVisibilityConverter();
            showPluginManagerMock = new Mock<IShowPluginManager>();
            showPluginManager = showPluginManagerMock.Object;
            vm = new HeadsetViewModel(showPluginManager, console, log, deviceManagerSA);
            //collaborationCheckedToVisibilityConverter.ViewModel = viewModel;
            // Assert
            Assert.That(collaborationCheckedToVisibilityConverter, Is.Not.Null);
            //Assert.That(collaborationCheckedToVisibilityConverter.ViewModel, Is.EqualTo(viewModel));
        }

        [Test]
        public void TestConvert()
        {
            CollaborationCheckedToVisibilityConverter collaborationCheckedToVisibilityConverter = new CollaborationCheckedToVisibilityConverter();
            var res = collaborationCheckedToVisibilityConverter.Convert(null, null, null, null);
            Assert.That(res, Is.EqualTo(Visibility.Collapsed));

            //var parameter == "",isChecked && ViewModel.Model == "WL7024"
            var parameter = "";
            var viewModel = new HeadsetViewModel(showPluginManager, console, log, deviceManagerSA);
            viewModel.Model = "WL7024";
            collaborationCheckedToVisibilityConverter.ViewModel = viewModel;
            res = collaborationCheckedToVisibilityConverter.Convert(true, null, parameter, null);
            Assert.That(res, Is.EqualTo(Visibility.Collapsed));

            //parameter == "MicNoiseCancellationPageShow"&&isChecked,viewModel.Model == "WL7024"
            parameter = "MicNoiseCancellationPageShow";
            viewModel = new HeadsetViewModel(showPluginManager, console, log, deviceManagerSA);
            viewModel.Model = "WL7024";
            collaborationCheckedToVisibilityConverter.ViewModel = viewModel;
            res = collaborationCheckedToVisibilityConverter.Convert(true, null, parameter, null);
            Assert.That(res, Is.EqualTo(Visibility.Visible));

            //parameter=="",isChecked && ViewModel.Model != "WL7024"
            parameter = "";
            collaborationCheckedToVisibilityConverter.ViewModel = new HeadsetViewModel(showPluginManager, console, log, deviceManagerSA);
            res = collaborationCheckedToVisibilityConverter.Convert(true, null, parameter, null);
            Assert.That(res, Is.EqualTo(Visibility.Collapsed));

            //parameter=="MicNoiseCancellationFewPageShow",isChecked && ViewModel.Model != "WL7024"
            parameter = "MicNoiseCancellationFewPageShow";
            collaborationCheckedToVisibilityConverter.ViewModel = new HeadsetViewModel(showPluginManager, console, log, deviceManagerSA);
            res = collaborationCheckedToVisibilityConverter.Convert(true, null, parameter, null);
            Assert.That(res, Is.EqualTo(Visibility.Visible));
        }

        //class ValueToVisibilityConverter
        [Test]
        public void TestValueToVisibilityConvert()
        {
            ValueToVisibilityConverter valueToVisibilityConverter = new ValueToVisibilityConverter();
            //value==null,parameter==null
            var res = valueToVisibilityConverter.Convert(null, null, null, null);
            Assert.That(res, Is.EqualTo(Visibility.Collapsed));

            //value==parameter
            var parameter = "1";
            res = valueToVisibilityConverter.Convert(1, null, parameter, null);
            Assert.That(res, Is.EqualTo(Visibility.Visible));

            //value!=parameter
            res = valueToVisibilityConverter.Convert(2, null, parameter, null);
            Assert.That(res, Is.EqualTo(Visibility.Collapsed));
        }

        //class InverseBooleanConverter
        [Test]
        public void TestInverseBooleanConvert()
        {
            InverseBooleanConverter inverseBooleanConverter = new InverseBooleanConverter();
            //value is bool boolean
            var res = inverseBooleanConverter.Convert(false, null, null, null);
            Assert.That(res, Is.EqualTo(true));

            //value is not bool boolean
            res = inverseBooleanConverter.Convert(null, null, null, null);
            Assert.That(res, Is.EqualTo(false));
        }

        [Test]
        public void TestInverseBooleanConvertBack()
        {
            InverseBooleanConverter inverseBooleanConverter = new InverseBooleanConverter();
            //value is bool boolean
            var res = inverseBooleanConverter.ConvertBack(false, null, null, null);
            Assert.That(res, Is.EqualTo(true));

            //value is not bool boolean
            res = inverseBooleanConverter.ConvertBack(null, null, null, null);
            Assert.That(res, Is.EqualTo(false));
        }

        //class BooleanToInverseForegroundConverter
        [Test]
        public void TestBooleanToInverseForegroundConverter()
        {
            BooleanToInverseForegroundConverter booleanToInverseForegroundConverter = new BooleanToInverseForegroundConverter();
            //value==true
            var res = booleanToInverseForegroundConverter.Convert(true, null, null, null);
            Assert.That(res, Is.EqualTo(Brushes.Gray));

            //value==false
            res = booleanToInverseForegroundConverter.Convert(false, null, null, null);
            Assert.That(res, Is.EqualTo(Brushes.White));

            //value is not bool boolean
            res = booleanToInverseForegroundConverter.Convert(null, null, null, null);
            Assert.That(res, Is.EqualTo(Brushes.White));
        }
    }
}