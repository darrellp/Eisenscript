using Eisenscript;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestEisenscript
{
    [TestClass]
    public class RgbaTests
    {
        [TestMethod]
        public void TestRGBAToHSV()
        {
            TestConversion(0, 0, 0);
            TestConversion(255, 0, 0);
            TestConversion(255, 255, 255);
            TestConversion(200, 100, 38);
        }

        void TestConversion(byte r, byte g, byte b)
        {
            var rgbaTest = new RGBA(r, g, b);
            var (h, s, v) = rgbaTest.HsvFromRgb();
            var rgbaComputed = RGBA.RgbFromHsv(h, s, v);
            Assert.AreEqual(rgbaTest, rgbaComputed);
        }
    }
}