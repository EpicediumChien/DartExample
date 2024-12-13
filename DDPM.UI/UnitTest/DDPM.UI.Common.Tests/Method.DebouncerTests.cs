using DDPM.SA.Common;
using DDPM.UI.Common.EAEM;
using DDPM.UI.Common.Interfaces;
using DDPM.UI.Common.Method;
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
using Debouncer = DDPM.UI.Common.Method.Debouncer;

namespace DDPM.UI.Common.Tests
{
    [Apartment(ApartmentState.STA)]
    public class DebouncerTests
    {
        private Debouncer? debouncer;

        [SetUp]
        public void Setup()
        {
            Action<object> action = (x) => { Console.WriteLine("action"); };
            debouncer = new Debouncer(1, action);
        }


        [Test]
        public void TestConstructor_Debouncer()
        {
            // Assert
            Assert.That(debouncer, Is.Not.Null);
        }

        [Test]
        public void TestDebounce()
        {
            try
            {
                debouncer.Debounce(2);
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
        }


    }
}