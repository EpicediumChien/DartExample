using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VcpCore.Common;

//namespace VcpCore.Plugins.ParameterTypeTest
namespace VcpCore.Plugins.Test
{
    public class TestParameterType
    {
        public class ParameterTypeTest
        {

            [Test]
            public void TestParametertype()
            {
                //Queue_CommandType commandType = new Queue_CommandType();
                //object parameter=new object();
                Queue_CommandType ActualcommandType = Queue_CommandType.GetCapabilitiesString;
                object Actualparameter = "Test Parameter";
                ParameterType parameterType = new ParameterType(ActualcommandType, Actualparameter);
                Assert.That(ActualcommandType, Is.EqualTo(parameterType.CommandType));
                Assert.That(Actualparameter, Is.EqualTo(parameterType.Parameter));

            }

        }

        public class Type_Initialize0x52toEmptyTest
        {

            [Test]
            public void TestType_Initialize0x52toEmpty()
            {
                Guid ActualdGuid = Guid.NewGuid();
                Type_Initialize0x52toEmpty expectGUID = new Type_Initialize0x52toEmpty(ActualdGuid);
                Assert.That(ActualdGuid, Is.EqualTo(expectGUID.guid));

            }

        }


        public class Type_Watcher0x52Test
        {

            [Test]
            public void TestType_Watcher0x52()
            {
                Guid ActualdGuid = Guid.NewGuid();
                Type_Watcher0x52 expectGUID = new Type_Watcher0x52(ActualdGuid);
                Assert.That(ActualdGuid, Is.EqualTo(expectGUID.guid));

            }

        }

        public class Type_GetCapabilitiesStringTest
        {

            [Test]
            public void TestType_GetCapabilitiesString()
            {
                Guid ActualGuid = Guid.NewGuid();
                MonitorInfo_complex ActualmonitorInfoX = new MonitorInfo_complex() { AliasDeviceName = "D2424H", FwVersion = "1.3", series = "1234567", CapabilityString = "DS1346425" };
                Type_GetCapabilitiesString type_GetCapabilitiesString = new Type_GetCapabilitiesString(ActualGuid, ActualmonitorInfoX);
                Assert.That(ActualGuid, Is.EqualTo(type_GetCapabilitiesString.guid));
                Assert.That(ActualmonitorInfoX, Is.EqualTo(type_GetCapabilitiesString.monitorInfoX));
            }

        }

        public class Type_GetVCPCapabilitiesTest
        {

            [Test]
            public void TestType_GetVCPCapabilities()
            {
                Guid ActualGuid = Guid.NewGuid();
                MonitorInfo_complex ActualmonitorInfoX = new MonitorInfo_complex() { AliasDeviceName = "AW2724H", CapabilityString = "HH1346425", series = "1234567" };
                Type_GetVCPCapabilities type_GetVCPCapabilities = new Type_GetVCPCapabilities(ActualGuid, ActualmonitorInfoX);
                Assert.That(ActualGuid, Is.EqualTo(type_GetVCPCapabilities.guid));
                Assert.That(ActualmonitorInfoX, Is.EqualTo(type_GetVCPCapabilities.monitorInfoX));
            }

        }


        public class Type_GetVCPCapability_ITest
        {

            [Test]
            public void TestType_GetVCPCapability_I()
            {
                Guid ActualGuid = Guid.NewGuid();
                MonitorInfo_complex ActualmonitorInfoX = new MonitorInfo_complex() { AliasDeviceName = "D2424H", FwVersion = "1.3", series = "1234567", CapabilityString = "DS1346425" };
                byte ActualCode = 0x11;
                int ActualOpt = 10;

                Type_GetVCPCapability_I type_GetVCPCapability_I = new Type_GetVCPCapability_I(ActualGuid, ActualmonitorInfoX, ActualCode, ActualOpt);
                Assert.That(ActualGuid, Is.EqualTo(type_GetVCPCapability_I.guid));
                Assert.That(ActualmonitorInfoX, Is.EqualTo(type_GetVCPCapability_I.monitorInfoX));
                Assert.That(ActualCode, Is.EqualTo(type_GetVCPCapability_I.code));
                Assert.That(ActualOpt, Is.EqualTo(type_GetVCPCapability_I.opt));
            }

        }


        public class Type_GetVCPCapability_IITest
        {

            [Test]
            public void TestType_GetVCPCapability_II()
            {
                Guid ActualGuid = Guid.NewGuid();
                MonitorInfo_complex ActualmonitorInfoX = new MonitorInfo_complex() { AliasDeviceName = "AW2724H", CapabilityString = "HH1346425", series = "1234567" };
                string ActualFunctionName = "GetVCPCapability_II";
                int Actualopt = 20;

                Type_GetVCPCapability_II type_GetVCPCapability_II = new Type_GetVCPCapability_II(ActualGuid, ActualmonitorInfoX, ActualFunctionName, Actualopt);
                Assert.That(ActualGuid, Is.EqualTo(type_GetVCPCapability_II.guid));
                Assert.That(ActualmonitorInfoX, Is.EqualTo(type_GetVCPCapability_II.monitorInfoX));
                Assert.That(ActualFunctionName, Is.EqualTo(type_GetVCPCapability_II.FunctionName));
                Assert.That(Actualopt, Is.EqualTo(type_GetVCPCapability_II.opt));
            }

        }


        public class Type_SetVCPCapability_ITest
        {

            [Test]
            public void TestType_SetVCPCapability_I()
            {
                Guid ActualGuid = Guid.NewGuid();
                MonitorInfo_complex ActualmonitorInfoX = new MonitorInfo_complex() { AliasDeviceName = "D2424H", FwVersion = "1.3", series = "1234567", CapabilityString = "DS1346425" };
                byte ActualCode = 0x11;
                uint ActualVal = 20;

                Type_SetVCPCapability_I type_SetVCPCapability_I = new Type_SetVCPCapability_I(ActualGuid, ActualmonitorInfoX, ActualCode, ActualVal);
                Assert.That(ActualGuid, Is.EqualTo(type_SetVCPCapability_I.guid));
                Assert.That(ActualmonitorInfoX, Is.EqualTo(type_SetVCPCapability_I.monitorInfoX));
                Assert.That(ActualCode, Is.EqualTo(type_SetVCPCapability_I.code));
                Assert.That(ActualVal, Is.EqualTo(type_SetVCPCapability_I.val));
            }

        }


        public class Type_SetVCPCapability_IITest
        {

            [Test]
            public void TestType_SetVCPCapability_II()
            {
                Guid ActualGuid = Guid.NewGuid();
                MonitorInfo_complex ActualmonitorInfoX = new MonitorInfo_complex() { AliasDeviceName = "AW2724H", CapabilityString = "HH1346425", series = "1234567" };
                string ActualFunctionName = "GetVCPCapability_II";
                string ActualVal = "0x22";

                Type_SetVCPCapability_II type_SetVCPCapability_II = new Type_SetVCPCapability_II(ActualGuid, ActualmonitorInfoX, ActualFunctionName, ActualVal);
                Assert.That(ActualGuid, Is.EqualTo(type_SetVCPCapability_II.guid));
                Assert.That(ActualmonitorInfoX, Is.EqualTo(type_SetVCPCapability_II.monitorInfoX));
                Assert.That(ActualFunctionName, Is.EqualTo(type_SetVCPCapability_II.FunctionName));
                Assert.That(ActualVal, Is.EqualTo(type_SetVCPCapability_II.val));
            }

        }
    }
}
