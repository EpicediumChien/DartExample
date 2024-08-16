using static VcpCore.Plugins.VcpCodeList;

namespace VcpCore.Plugins.Test
{
    public class TestVcpCodeList
    {
        [Test]
        public void TestVcpValueTostring()
        {
            var vcpval = new VcpCodeList.VcpValue();
            var VcpValueTostring = "VCP: 0x0, value:0";
            var result = vcpval.ToString();
            Assert.That(VcpValueTostring, Is.EqualTo(result));
        }

        [Test]
        public void TestgetVcpAndValue()
        {
            string presetName = "Standard";
            VcpValue retValue = new VcpValue() { Vcp = 0xDC, Value = 0 };
            var result = VcpCodeList.getVcpAndValue(presetName);
            Assert.That(retValue, Is.EqualTo(result));

            string presetName2 = "SPORTS Game";
            VcpValue retValu2 = new VcpValue() { Vcp = 0xF0, Value = 19 };
            var result2 = VcpCodeList.getVcpAndValue(presetName2);
            Assert.That(retValu2, Is.EqualTo(result2));

            string presetName3 = "Warm";
            VcpValue retValu3 = new VcpValue() { Vcp = 0x14, Value = 11 };
            var result3 = VcpCodeList.getVcpAndValue(presetName3);
            Assert.That(retValu3, Is.EqualTo(result3));

            string presetName4 = "Noset";
            var result4 = VcpCodeList.getVcpAndValue(presetName4);
            Assert.IsNull(result4);
        }
    }
}