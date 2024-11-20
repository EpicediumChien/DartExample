using System.Windows.Controls;
using System.Windows;
using DDPM.UI.Resources.Helper;

namespace DDPM.UI.Resources.Tests
{
    [Apartment(ApartmentState.STA)]
    public class LangHelperTests
    {

        private LangHelper? langHelper;

        [SetUp]
        public void Setup()
        {
            langHelper = new LangHelper();
        }

        [Test]
        public void TestConstructor_LangHelper()
        {
            // Assert
            Assert.That(langHelper, Is.Not.Null);
        }

        [Test]
        public void Testthis()
        {
            // Act
            var result = langHelper["a"];
            // Assert
            Assert.That(result, Is.EqualTo(string.Empty));
        }

        [Test]
        public void TestChangeLanguage()
        {
            try
            {
                langHelper.ChangeLanguage(new System.Globalization.CultureInfo(2));
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }

        }

    }
}