using DDPM.SA.Common;
using DDPM.UI.Common.EAEM;
using DDPM.UI.Common.Interfaces;
using DDPM.UI.Common.Models;
using DDPM.UI.Common.ViewModels;
using DDPM.UI.Interfaces;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.UX.WPF;
using DPeMPublic.Common.Enums;
using Moq;
using System.Reflection.Metadata;
using System.Runtime.Intrinsics.X86;
using System.Security.Policy;
using System.Windows.Media;
using System.Xml.Linq;
using VcpCore.Common;
using Windows.Devices.Input;
using static DDPM.UI.Common.User32;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

namespace DDPM.UI.Common.Tests
{
    [Apartment(ApartmentState.STA)]
    public class TabHeaderTests
    {
        private TabHeader? tabHeader;

        [SetUp]
        public void Setup()
        {
            tabHeader = new TabHeader();
        }

        [Test]
        public void TestConstructor_TabHeader()
        {
            // Assert
            Assert.That(tabHeader, Is.Not.Null);
        }

        [Test]
        public void TestText()
        {
            // Act
            tabHeader.Text = "Text";
            // Assert
            Assert.That(tabHeader.Text, Is.EqualTo("Text"));
        }
    }
}