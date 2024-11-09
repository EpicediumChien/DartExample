using DDPM.UI.Common.EAEM;
using DDPM.UI.Common.Interfaces;
using DDPM.UI.Common.UserControls;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.UX.WPF;

namespace DDPM.UI.Common.Tests
{
    [Apartment(ApartmentState.STA)]
    public class NoSroHashTableTests
    {
        private NoSroHashTable? noSroHashTable;

        [SetUp]
        public void Setup()
        {
            noSroHashTable = new NoSroHashTable();
        }

        [Test]
        public void TestConstructor_NoSroHashTable()
        {
            // Assert
            Assert.That(noSroHashTable, Is.Not.Null);
        }

        [Test]
        public void TestLeaveHoverState()
        {
            try
            {
                noSroHashTable.Add(0,"A");
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
        }

        [Test]
        public void TestClear()
        {
            try
            {
                noSroHashTable.Clear();
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
        }

        [Test]
        public void TestRemove()
        {
            try
            {
                noSroHashTable.Remove(0);
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
        }

        [Test]
        public void TestKeys()
        {
            var result = noSroHashTable.Keys;
            Assert.That (result, Is.Not.Null);
        }
    }
}