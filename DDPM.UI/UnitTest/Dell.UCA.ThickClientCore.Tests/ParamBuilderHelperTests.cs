using Dell.Client.Framework.Common;
using Dell.Client.Framework.UX.Common.DataModel;
using Dell.Client.Framework.UX.WPF;
using Moq;
using NGA.ThickClient.Interfaces;
using NGA.ThickClientCore;
using NGA.UnitTest.PrivateObject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dell.UCA.ThickClientCore.Tests
{
    [TestFixture, Apartment(ApartmentState.STA)]
    public class ParamBuilderHelperTests
    {
        private ShowPluginCommand? pluginCommand;
        private Mock<ILog>? logMock;
        private IConsole? console;
        private Mock<IShowPluginManager>? showPluginManagerMock;
        private PrivateObject? privateObject;
        private Guid? pluginId;


        [SetUp]
        public void Setup()
        {
            if (System.Windows.Application.Current == null)
            {
                new System.Windows.Application();
            }
            pluginId = new Guid("3c6863f9-d8d6-4045-9403-8c3ace7df488"); ;
            pluginCommand = new ShowPluginCommand(new Guid("3c6863f9-d8d6-4045-9403-8c3ace7df488"), null, null) ;
            logMock=new Mock<ILog>();
        }


        [Test]
        public void TestCreateConsoleWindowArguments()
        {
            var result = ParamBuilderHelper.CreateConsoleWindowArguments(null, logMock.Object);
            Assert.That(result.Count,Is.EqualTo(0));
        }

        [Test]
        public void TestCreateConsoleWindowArgumentsa()
        {
            var result = ParamBuilderHelper.CreateConsoleWindowArguments(pluginCommand, logMock.Object);
            Assert.That(result.Count,Is.EqualTo(1));
        }
    }
}
