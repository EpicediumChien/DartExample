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
using Dell.Client.Framework.UX.WPF.ResourceManager;
namespace DDPM.UI.Plugin.Common.Tests
{
    [Apartment(ApartmentState.STA)]
    public class ActionParameterModalDialogTests
    {
        private ActionParameterModalDialog? actionParameterModalDialog;

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
