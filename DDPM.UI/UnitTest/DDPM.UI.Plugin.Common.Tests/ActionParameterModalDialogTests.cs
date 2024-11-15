using DDPM.SA.Common;
using DDPM.UI.Common.Interfaces;
using DDPM.UI.Common;
using DDPM.UI.Plugin.ViewModels;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.UX.WPF;
using Moq;
using NGA.UnitTest.PrivateObject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DDPM.UI.Plugin.Common.Tests
{
    [Apartment(ApartmentState.STA)]
    public class ActionParameterModalDialogTests
    {
        private ActionParameterModalDialog? actionParameterModalDialog;

        [SetUp]
        public void Setup()
        {

        }

        [Test]
        public void TestConstructor_ActionParameterModalDialog()
        {
            actionParameterModalDialog = new ActionParameterModalDialog(AdvancedAction.AssignKeystroke, 1, 1, "");
            Assert.That(actionParameterModalDialog, Is.Not.Null);
            actionParameterModalDialog = new ActionParameterModalDialog(AdvancedAction.OpenFile, 1, 1, "");
            Assert.That(actionParameterModalDialog, Is.Not.Null);
            actionParameterModalDialog = new ActionParameterModalDialog(AdvancedAction.OpenFolder, 1, 1, "");
            Assert.That(actionParameterModalDialog, Is.Not.Null);
            actionParameterModalDialog = new ActionParameterModalDialog(AdvancedAction.OpenWebPage, 1, 1, "");
            Assert.That(actionParameterModalDialog, Is.Not.Null);
        }
    }
}
