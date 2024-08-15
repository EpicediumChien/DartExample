using DDPM.SA.Common;
using DDPM.UI.Plugin.ViewModels;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.UX.WPF;
using Moq;
using NGA.UnitTest.PrivateObject;
using NUnit.Framework;
using System.Windows;
using System.Windows.Input;

namespace DDPM.UI.Module.Collaboration.Test
{
    [TestFixture, Apartment(ApartmentState.STA)]
    public class CollaborationRightViewTests
    {
        private Mock<IConsole>? consoleMock;
        private Mock<ILog>? logMock;
        private Mock<IDeviceManagerSA>? deviceManagerMock;
        private KeyboardViewModel? keyboardViewModel;
        private CollaborationRightView? collaborationRightView;
        private PrivateObject? privateObject;

        [SetUp]
        public void SetUp()
        {
            consoleMock = new Mock<IConsole>();
            logMock = new Mock<ILog>();
            deviceManagerMock = new Mock<IDeviceManagerSA>();

            keyboardViewModel = new KeyboardViewModel(consoleMock.Object, logMock.Object, deviceManagerMock.Object);

            collaborationRightView = new CollaborationRightView(keyboardViewModel);
            privateObject = new PrivateObject(collaborationRightView);
        }

        [Test]
        public void TestConstructor()
        {
            // Act
            var vmField = (KeyboardViewModel)privateObject!.GetFieldOrProperty("_vm");

            var txtCollabsCaptionText = privateObject!.GetFieldOrProperty("CollabsCaption1");
            var txtCollabsTooltipText = privateObject!.GetFieldOrProperty("CollaborationToolTip");
            var cbCollaborationBlinkEffectTextContent = privateObject!.GetFieldOrProperty("CollaborationBlinkEffectText");
            var cbCollaborationDoubleTapTextContent = privateObject!.GetFieldOrProperty("CollaborationDoubleTapText");

            var txtAlert1Text = privateObject!.GetFieldOrProperty("Alert5");
            var txtLearnMore1Text = privateObject!.GetFieldOrProperty("LearnMoreLink");
            var txtVideoText = privateObject!.GetFieldOrProperty("VideoCaption");
            var txtShareText = privateObject!.GetFieldOrProperty("ShareCaption");
            var txtChatText = privateObject!.GetFieldOrProperty("ChatCaption");
            var txtMicText = privateObject!.GetFieldOrProperty("MicCaption");

            // Assert
            Assert.That(vmField, Is.Not.Null);
            Assert.That(vmField, Is.EqualTo(keyboardViewModel));
            Assert.That(collaborationRightView, Is.Not.Null);

            Assert.That(txtCollabsCaptionText, Is.EqualTo("Collaboration Icons"));
            Assert.That(txtCollabsTooltipText, Is.EqualTo("Provides quick access to conference controls. Toggling the\nkeys on/off will show or hide them on the keyboard while in\na Microsoft Teams or Zoom call."));
            Assert.That(cbCollaborationBlinkEffectTextContent, Is.EqualTo("Enable blink effect when there is a new chat message in conference\ncall"));
            Assert.That(cbCollaborationDoubleTapTextContent, Is.EqualTo("Activate icons on the keyboard by double tapping instead of single\ntapping"));
            Assert.That(txtAlert1Text, Is.EqualTo("To use Collaboration Keyboard with Microsoft Teams, ensure that you are signed into and using the latest version of Microsoft Teams, and that Third-party app API is enabled"));
            Assert.That(txtLearnMore1Text, Is.EqualTo("Learn more"));

            Assert.That(txtVideoText, Is.EqualTo("Video"));
            Assert.That(txtShareText, Is.EqualTo("Share"));
            Assert.That(txtChatText, Is.EqualTo("Chat"));
            Assert.That(txtMicText, Is.EqualTo("Mic"));
        }

        [Test]
        public void TestCloseDescription()
        {
            // Arrange
            //var descriptionGrid = new RowDefinition();
            //privateObject!.SetFieldOrProperty("DescriptionGrid", descriptionGrid);
            var mouseButtonEventArgs = new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Left)
            {
                RoutedEvent = UIElement.MouseLeftButtonDownEvent
            };

            // Act
            privateObject.Invoke("CloseDescription", this, mouseButtonEventArgs);
            var bdrAlert = (UIElement)privateObject.GetFieldOrProperty("bdrAlert");
            var txtCollabsCaptionText = privateObject.GetFieldOrProperty("CollabsCaption2");

            // Assert
            Assert.That(bdrAlert.Visibility, Is.EqualTo(Visibility.Collapsed));
            Assert.That(txtCollabsCaptionText, Is.EqualTo("Collaboration"));
            //Assert.That(descriptionGrid.Height, Is.EqualTo(new GridLength(0)));
            //Assert.That(keyboardViewModel!.CollabsCaption, Is.EqualTo("Collaboration"));
        }
    }
}