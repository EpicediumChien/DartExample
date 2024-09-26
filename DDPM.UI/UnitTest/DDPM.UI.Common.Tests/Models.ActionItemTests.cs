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
        public void TestKnMAction()
        {
            var result = Actions.KnMActions.Count;
            // Assert
            Assert.That(result, Is.GreaterThan(0));
        }

        [Test]
        public void TestOfficeActions()
        {
            var result = Actions.OfficeActions.Count;
            // Assert
            Assert.That(result, Is.GreaterThan(0));
        }

        [Test]
        public void TestPenActions()
        {
            var result = Actions.PenActions.Count;
            // Assert
            Assert.That(result, Is.GreaterThan(0));
        }

        [Test]
        public void TestOpenRunActions()
        {
            var result = Actions.OpenRunActions.Count;
            // Assert
            Assert.That(result, Is.GreaterThan(0));
        }

        [Test]
        public void TestRadialMenuActions()
        {
            var result = Actions.RadialMenuActions.Count;
            // Assert
            Assert.That(result, Is.GreaterThan(0));
        }

        [Test]
        public void TestOpenRunActionsList()
        {
            var result = Actions.OpenRunActionsList;
            // Assert
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void TestRadialMenuActionsList()
        {
            var result = Actions.RadialMenuActionsList;
            // Assert
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void TestAllActionsKnM()
        {
            var result = Actions.AllActionsKnM;
            // Assert
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void TestAllActionsPenBarrelButton()
        {
            var result = Actions.AllActionsPenBarrelButton;
            // Assert
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void TestAllActionsPenTopButton()
        {
            var result = Actions.AllActionsPenTopButton;
            // Assert
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void TestWindowsActionsKnM()
        {
            var result = Actions.WindowsActionsKnM;
            // Assert
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void TestWindowsActionsPenBarrelButton()
        {
            var result = Actions.WindowsActionsPenBarrelButton;
            // Assert
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void TestWindowsActionsPenTopButton()
        {
            var result = Actions.WindowsActionsPenTopButton;
            // Assert
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void TestProductivityActionsKnM()
        {
            var result = Actions.ProductivityActionsKnM;
            // Assert
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void TestProductivityActionsPenBarrelButton()
        {
            var result = Actions.ProductivityActionsPenBarrelButton;
            // Assert
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void TestProductivityActionsPenTopButton()
        {
            var result = Actions.ProductivityActionsPenTopButton;
            // Assert
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void TestMultimediaActionsKnM()
        {
            var result = Actions.MultimediaActionsKnM;
            // Assert
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void TestMultimediaActionsPenBarrelButton()
        {
            var result = Actions.MultimediaActionsPenBarrelButton;
            // Assert
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void TestMultimediaActionsPenTopButton()
        {
            var result = Actions.MultimediaActionsPenTopButton;
            // Assert
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void TestWordActions()
        {
            var result = Actions.WordActions;
            // Assert
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void TestExcelActions()
        {
            var result = Actions.ExcelActions;
            // Assert
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void TestPowerPointActions()
        {
            var result = Actions.PowerPointActions;
            // Assert
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void TestOutlookActions()
        {
            var result = Actions.OutlookActions;
            // Assert
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void TestSuggestedActionsK()
        {
            var result = Actions.SuggestedActionsK;
            // Assert
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void TestSuggestedActionsM()
        {
            var result = Actions.SuggestedActionsM;
            // Assert
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void TestSuggestedActionsPenTopButton()
        {
            var result = Actions.SuggestedActionsPenTopButton;
            // Assert
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void TestSuggestedActionsPenBarrelButton()
        {
            var result = Actions.SuggestedActionsPenBarrelButton;
            // Assert
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void TestAdvancedActions()
        {
            var result = Actions.AdvancedActions;
            // Assert
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void TestAdvancedActionsPen()
        {
            var result = Actions.AdvancedActionsPen;
            // Assert
            Assert.That(result, Is.Not.Null);
        }
    }
}