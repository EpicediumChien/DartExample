using DDPM.SA.Common;
using DDPM.UI.Common;
using DDPM.UI.Plugin.ViewModels;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.UX.WPF;
using Moq;
using NGA.UnitTest.PrivateObject;
using NUnit.Framework;
using System.Windows;
using System.Windows.Input;
using Dell.Client.Framework.UX.WPF.ResourceManager;
using System.Globalization;
namespace DDPM.UI.Module.Collaboration.Test
{
    [TestFixture, Apartment(ApartmentState.STA)]
    public class CollaborationRightViewTests
    {
        private Mock<IConsole>? consoleMock;
        private Mock<ILog>? logMock;
        private Mock<IDeviceManagerSA>? deviceManagerMock;
        private KeyboardViewModel? vm;
        private CollaborationRightView? collaborationRightView;
        private PrivateObject? privateObject;

        [SetUp]
        public void SetUp()
        {
            if (System.Windows.Application.Current == null)
            {
                new System.Windows.Application();
            }
            ResourceManager res = new ResourceManager();
            var resourceDictionary = new ResourceDictionary();
            resourceDictionary.Source = new Uri("pack://application:,,,/DDPM.UI.Common;component/ModuleStyle.xaml");
            System.Windows.Application.Current.Resources.MergedDictionaries.Add(resourceDictionary);
            consoleMock = new Mock<IConsole>();
            logMock = new Mock<ILog>();
            deviceManagerMock = new Mock<IDeviceManagerSA>();
            DdpmCommonHelper.DeviceManagerSA = deviceManagerMock.Object;
            vm = new KeyboardViewModel(consoleMock.Object, logMock.Object);

            collaborationRightView = new CollaborationRightView(vm);
            privateObject = new PrivateObject(collaborationRightView);
        }

        [Test]
        public void TestConstructor_CollaborationRightView()
        {
            var vmres = privateObject.GetFieldOrProperty("_vm");
            Assert.That(vmres, Is.EqualTo(vm));
            if (CultureInfo.CurrentCulture.Name == "es-US")
            {
                // Act
                var txtCollabsCaptionText = Strings.CollabsCaption;
                var txtCollabsTooltipText = Strings.CollaborationToolTip;
                var cbCollaborationBlinkEffectTextContent = Strings.CollaborationBlinkEffectText;
                var cbCollaborationDoubleTapTextContent = Strings.CollaborationDoubleTapText;
                var txtVideoText = Strings.VideoCaption;
                var txtShareText = Strings.ShareCaption;
                var txtChatText = Strings.ChatCaption;
                var txtMicText = Strings.MicCaption;

                // Assert
                Assert.That(txtCollabsCaptionText, Is.EqualTo("Collaboration"));
                Assert.That(txtCollabsTooltipText, Is.EqualTo("Provides quick access to conference controls. Toggling the\nkeys on/off will show or hide them on the keyboard while in\na Microsoft Teams or Zoom call."));
                Assert.That(cbCollaborationBlinkEffectTextContent, Is.EqualTo("Enable blink effect when there is a new chat message in conference\ncall"));
                Assert.That(cbCollaborationDoubleTapTextContent, Is.EqualTo("Activate icons on the keyboard by double tapping instead of single\ntapping"));
                Assert.That(txtVideoText, Is.EqualTo("Video"));
                Assert.That(txtShareText, Is.EqualTo("Share"));
                Assert.That(txtChatText, Is.EqualTo("Chat"));
                Assert.That(txtMicText, Is.EqualTo("Mic"));
            }
        }

        //[Test]
        //public void TestCloseDescription()
        //{
        //    // Arrange
        //    //var descriptionGrid = new RowDefinition();
        //    //privateObject!.SetFieldOrProperty("DescriptionGrid", descriptionGrid);
        //    var mouseButtonEventArgs = new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Left)
        //    {
        //        RoutedEvent = UIElement.MouseLeftButtonDownEvent
        //    };

        //    // Act
        //    privateObject.Invoke("CloseDescription", this, mouseButtonEventArgs);
        //    var bdrAlert = (UIElement)privateObject.GetFieldOrProperty("bdrAlert");
        //    var txtCollabsCaptionText = privateObject.GetFieldOrProperty("CollabsCaption2");

        //    // Assert
        //    Assert.That(bdrAlert.Visibility, Is.EqualTo(Visibility.Collapsed));
        //    Assert.That(txtCollabsCaptionText, Is.EqualTo("Collaboration"));
        //    //Assert.That(descriptionGrid.Height, Is.EqualTo(new GridLength(0)));
        //    //Assert.That(keyboardViewModel!.CollabsCaption, Is.EqualTo("Collaboration"));
        //}
    }
}