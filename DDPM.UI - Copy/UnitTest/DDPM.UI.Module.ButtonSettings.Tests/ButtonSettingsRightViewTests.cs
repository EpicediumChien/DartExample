using DDPM.SA.Common;
using DDPM.UI.Common;
using DDPM.UI.Plugin.ViewModels;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.UX.WPF;
using Moq;
using NGA.UnitTest.PrivateObject;
using NUnit.Framework;
using System.Windows;
using System.Windows.Controls;

namespace DDPM.UI.Module.ButtonSettings.Test
{
    [TestFixture, Apartment(ApartmentState.STA)]
    public class ButtonSettingsRightViewTests
    {
        private Mock<IConsole>? consoleMock;
        private Mock<ILog>? logMock;
        private Mock<IDeviceManagerSA>? deviceManagerMock;
        private MouseViewModel? mouseViewModel;
        private ButtonSettingsRightView? buttonSettingsRightView;
        private PrivateObject? privateObject;

        [SetUp]
        public void SetUp()
        {
            consoleMock = new Mock<IConsole>();
            logMock = new Mock<ILog>();
            deviceManagerMock = new Mock<IDeviceManagerSA>();
            mouseViewModel = new MouseViewModel(consoleMock.Object, logMock.Object, deviceManagerMock.Object);
            buttonSettingsRightView = new ButtonSettingsRightView(mouseViewModel);
            privateObject = new PrivateObject(buttonSettingsRightView);
        }

        [Test]
        public void TestConstructor()
        {
            // Act
            Dictionary<string, string> buttonCaptions = (Dictionary<string, string>)privateObject.GetFieldOrProperty("ButtonCaptions");
            var txtMessageText = Strings.ButtonCustomizeMessage;
            var txtRestoreText = Strings.ButtonCustomizeRestoreCaption;
            var txtSuggestedActionsText = Strings.SuggestedActionsCaption;
            var txtProductivityActionsText = Strings.ProductivityActionsCaption;
            var txtWindowsActionsText = Strings.WindowsActionsCaption;
            var txtMultimediaActionsText = Strings.MultimediaActionsCaption;

            // Assert
            Assert.That(buttonCaptions.Count, Is.EqualTo(5));
            Assert.That(txtMessageText, Is.EqualTo("To customize, click one of the outlined sections on the image to the left"));
            Assert.That(txtRestoreText, Is.EqualTo("Restore to All Applictions"));
            Assert.That(txtSuggestedActionsText, Is.EqualTo("Suggested Actions"));
            Assert.That(txtProductivityActionsText, Is.EqualTo("Productivity Actions"));
            Assert.That(txtWindowsActionsText, Is.EqualTo("Windows Actions"));
            Assert.That(txtMultimediaActionsText, Is.EqualTo("Multimedia Actions"));
        }

        [Test]
        public void TestInitialize()
        {
            buttonSettingsRightView.Initialize();

            var txtCaptionText = Strings.ButtonCustomizeCaption;
            var imgBack = (UIElement)privateObject.GetFieldOrProperty("imgBack");
            var section1 = (StackPanel)privateObject.GetFieldOrProperty("Section1");
            //var selectedActionID = privateObject.GetFieldOrProperty("SelectedActionID");

            Assert.That(txtCaptionText, Is.EqualTo("Button Customization"));
            Assert.That(imgBack.Visibility, Is.EqualTo(Visibility.Collapsed));
            Assert.That(section1.Visibility, Is.EqualTo(Visibility.Visible));
            //Assert.That(selectedActionID, Is.EqualTo(-1));

            mouseViewModel.MouseAction.ButtonActions.Add(MouseButtonName.ScrollWheelClick, new SelectedMouseAction(1, new AssignedAction(1)));
            string selectedButton = "ScrollWheelClick";
            mouseViewModel.SelectedButton = selectedButton;
            buttonSettingsRightView.Initialize();
            section1 = (StackPanel)privateObject.GetFieldOrProperty("Section1");
            //selectedActionID = privateObject.GetFieldOrProperty("SelectedActionID");
            Assert.That(section1.Visibility, Is.EqualTo(Visibility.Collapsed));
            //Assert.That(selectedActionID, Is.EqualTo(0));
        }
    }
}