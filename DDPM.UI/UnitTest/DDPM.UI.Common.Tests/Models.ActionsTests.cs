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
    public class PenActionsTests
    {
        private PenActions? penActions;

        [SetUp]
        public void Setup()
        {
            penActions = new PenActions();
        }

        [Test]
        public void TestConstructor_PenActions()
        {
            // Assert
            Assert.That(penActions, Is.Not.Null);
        }

        [Test]
        public void TestConstructor_PenActionsA()
        {
            var penActionsA = new PenActions(Strings.None);
            // Assert
            Assert.That(penActionsA, Is.Not.Null);
        }

        [Test]
        public void TestResetRadialMenu()
        {
            try
            {
                penActions.ResetRadialMenu();
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
        }

        //class KeyboardActions
        [Test]
        public void TestConstructor_KeyboardActions()
        {
            //Act
            var keyboardActions = new KeyboardActions();
            // Assert
            Assert.That(keyboardActions, Is.Not.Null);
        }

        [Test]
        public void TestConstructor_KeyboardActionsA()
        {
            //Act
            var keyboardActionsA = new KeyboardActions("KB7221W");
            // Assert
            Assert.That(keyboardActionsA, Is.Not.Null);
            Assert.That(keyboardActionsA.KeyActions.Count, Is.GreaterThan(0));
        }

        //class MouseActions
        [Test]
        public void TestConstructor_MouseActions()
        {
            //Act
            var mouseActions = new MouseActions();
            // Assert
            Assert.That(mouseActions, Is.Not.Null);
        }

        [Test]
        public void TestConstructor_MouseActionsA()
        {
            //Act
            var mouseActionsA = new MouseActions("MS900");
            // Assert
            Assert.That(mouseActionsA, Is.Not.Null);
            Assert.That(mouseActionsA.ButtonActions.Count, Is.GreaterThan(0));
        }


        //class SelectedAction
        [Test]
        public void TestConstructor_SelectedAction()
        {
            //Act
            var selectedAction = new SelectedAction();
            // Assert
            Assert.That(selectedAction, Is.Not.Null);
        }

        [Test]
        public void TestConstructor_SelectedActionA()
        {
            //Act
            var selectedActionA = new SelectedAction(1,new AssignedAction() { ID=-2});
            // Assert
            Assert.That(selectedActionA, Is.Not.Null);
            Assert.That(selectedActionA.DefaultActionID, Is.EqualTo(1));
            Assert.That(selectedActionA.AssignedAction.ID, Is.EqualTo(-2));
        }

        //class AssignedAction
        [Test]
        public void TestConstructor_AssignedAction()
        {
            //Act
            var assignedAction = new AssignedAction();
            // Assert
            Assert.That(assignedAction, Is.Not.Null);
        }

        [Test]
        public void TestConstructor_AssignedActionA()
        {
            //Act
            var assignedActionA = new AssignedAction(1, "");
            // Assert
            Assert.That(assignedActionA, Is.Not.Null);
            Assert.That(assignedActionA.ID, Is.EqualTo(1));
            Assert.That(assignedActionA.Parameter, Is.EqualTo(""));
        }

        //class ActionList
        [Test]
        public void TestConstructor_ActionList()
        {
            //Act
            var result = ActionList.ExportActionList(new PenActions(), "PEN");
            // Assert
            Assert.That(result, Is.EqualTo(true));
        }

        [Test]
        public void TestImportActionList()
        {
            // Setup

            // Execute and Verify
            Assert.IsNotNull(ActionList.ImportActionList(eDeviceCategory.KB, "KB"), $"ImportActionList() returns null");
            Assert.IsNotNull(ActionList.ImportActionList(eDeviceCategory.Mouse, "Mouse"), $"ImportActionList() returns null");
            Assert.IsNotNull(ActionList.ImportActionList(eDeviceCategory.Pen, "PEN"), $"ImportActionList() returns null");
        }
    }
}