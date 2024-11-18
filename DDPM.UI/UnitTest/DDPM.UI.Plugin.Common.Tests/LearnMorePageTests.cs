using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace DDPM.UI.Plugin.Common.Tests
{
    [Apartment(ApartmentState.STA)]
    public class LearnMorePageTests
    {
        private LearnMorePage? learnMorePage;

        [SetUp]
        public void Setup()
        {
            learnMorePage = new LearnMorePage(1.0, 2.0, "parameter");
        }

        [Test]
        public void TestConstructor_LearnMorePage()
        {
            // Assert
            Assert.That(learnMorePage, Is.Not.Null);
            Assert.That(learnMorePage.Width, Is.EqualTo(1.0));
            Assert.That(learnMorePage.Height, Is.EqualTo(2.0));
        }

        [Test]
        public void TestParameter()
        {
            Assert.That(learnMorePage.Parameter, Is.EqualTo(""));
        }
    }
}
