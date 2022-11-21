using System.IO;
using Builder;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestEisenscript
{
    [TestClass]
    public class ExampleTests
    {

        [TestMethod]
        public void TestTransformDraw()
        {
        }

        void TestExample(string fileName)
        {
            var tr = File.OpenText(fileName);
            var builder = new SSBuilder();
            builder.DrawEvent += ((s, a) => { });
            var errors = builder.Build(tr);
            Assert.AreEqual(0, errors.Count);
        }
    }
}
