using DDPM.UI.Common.UserControls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DDPM.UI.Plugin.Common.Tests
{
    [Apartment(ApartmentState.STA)]
    public class MessageModalDialogTests
    {
        private MessageModalDialog? messageModalDialog;

        [SetUp]
        public void Setup()
        {
            messageModalDialog = new MessageModalDialog("Caption", "Message", "", "","", 417.0);
        }

        [Test]
        public void TestConstructor_MessageModalDialog()
        {
            // Assert
            Assert.That(messageModalDialog, Is.Not.Null);
            Assert.That(messageModalDialog.Width, Is.EqualTo(417.0));
        }

        [Test]
        public void TestCloseByCaller()
        {
            try
            {
                messageModalDialog.CloseByCaller();
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }

        }
    }
}
