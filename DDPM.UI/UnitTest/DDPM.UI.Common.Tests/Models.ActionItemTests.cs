using DDPM.UI.Common.EAEM;
using DDPM.UI.Common.Interfaces;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.UX.WPF;
using Moq;
using System.Security.Policy;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

namespace DDPM.UI.Common.Tests
{
    [Apartment(ApartmentState.STA)]
    public class ActionItemTests
    {
        private ActionItem? actionItem;

        [SetUp]
        public void Setup()
        {
            actionItem = new ActionItem();
        }

        [Test]
        public void TestConstructor_ActionItem()
        {
            // Assert
            Assert.That(actionItem, Is.Not.Null);
        }

        [Test]
        public void TestConstructor_ActionItemA()
        {
            // Act
            var actionItemA = new ActionItem(ActionCategory.None, Strings.None, true, true, 4, 4);
            // Assert
            Assert.That(actionItemA, Is.Not.Null);
            Assert.That(actionItemA.Category, Is.EqualTo(ActionCategory.None));
            Assert.That(actionItemA.Caption, Is.EqualTo(Strings.None));
            Assert.That(actionItemA.IsSuggestedForKeyboard, Is.EqualTo(true));
            Assert.That(actionItemA.IsSuggestedForMouse, Is.EqualTo(true));
        }

        [Test]
        public void TestConstructor_ActionItemB()
        {
            // Act
            var actionItemB = new ActionItem(ActionCategory.WordAction, Strings.Autoscroll, false, false);
            // Assert
            Assert.That(actionItemB, Is.Not.Null);
            Assert.That(actionItemB.Category, Is.EqualTo(ActionCategory.WordAction));
            Assert.That(actionItemB.Caption, Is.EqualTo(Strings.Autoscroll));
            Assert.That(actionItemB.IsSuggestedForKeyboard, Is.EqualTo(false));
            Assert.That(actionItemB.IsSuggestedForMouse, Is.EqualTo(false));
        }

        //class Actions
        [Test]
        public void TestOpenRunActionsList()
        {
            // Act
            var result = Actions.OpenRunActionsList();
            // Assert
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void TestRadialMenuActionsList()
        {
            // Act
            var result = Actions.RadialMenuActionsList();
            // Assert
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void TestAllActionsKnM()
        {
            // Act
            var result = Actions.AllActionsKnM();
            // Assert
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void TestAllActionsPenBarrelButton()
        {
            // Act
            var result = Actions.AllActionsPenBarrelButton();
            // Assert
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void TestAllActionsPenTopButton()
        {
            // Act
            var result = Actions.AllActionsPenTopButton();
            // Assert
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void TestWindowsActionsKnM()
        {
            // Act
            var result = Actions.WindowsActionsKnM();
            // Assert
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void TestWindowsActionsPenBarrelButton()
        {
            // Act
            var result = Actions.WindowsActionsPenBarrelButton();
            // Assert
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void TestWindowsActionsPenTopButton()
        {
            // Act
            var result = Actions.WindowsActionsPenTopButton();
            // Assert
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void TestProductivityActionsKnM()
        {
            // Act
            var result = Actions.ProductivityActionsKnM();
            // Assert
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void TestProductivityActionsPenBarrelButton()
        {
            // Act
            var result = Actions.ProductivityActionsPenBarrelButton();
            // Assert
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void TestProductivityActionsPenTopButton()
        {
            // Act
            var result = Actions.ProductivityActionsPenTopButton();
            // Assert
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void TestMultimediaActionsKnM()
        {
            // Act
            var result = Actions.MultimediaActionsKnM();
            // Assert
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void TestMultimediaActionsPenBarrelButton()
        {
            // Act
            var result = Actions.MultimediaActionsPenBarrelButton();
            // Assert
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void TestMultimediaActionsPenTopButton()
        {
            // Act
            var result = Actions.MultimediaActionsPenTopButton();
            // Assert
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void TestWordActions()
        {
            // Act
            var result = Actions.WordActions();
            // Assert
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void TestExcelActions()
        {
            // Act
            var result = Actions.ExcelActions();
            // Assert
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void TestPowerPointActions()
        {
            // Act
            var result = Actions.PowerPointActions();
            // Assert
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void TestOutlookActions()
        {
            // Act
            var result = Actions.OutlookActions();
            // Assert
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void TestSuggestedActionsK()
        {
            // Act
            var result = Actions.SuggestedActionsK();
            // Assert
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void TestSuggestedActionsM()
        {
            // Act
            var result = Actions.SuggestedActionsM();
            // Assert
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void TestSuggestedActionsPenTopButton()
        {
            // Act
            var result = Actions.SuggestedActionsPenTopButton();
            // Assert
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void TestSuggestedActionsPenBarrelButton()
        {
            // Act
            var result = Actions.SuggestedActionsPenBarrelButton();
            // Assert
            Assert.That(result, Is.Not.Null);
        }
    }
}