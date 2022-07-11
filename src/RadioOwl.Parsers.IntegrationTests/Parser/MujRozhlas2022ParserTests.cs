using Microsoft.VisualStudio.TestTools.UnitTesting;
using RadioOwl.Parsers.Parser;
using System.Threading.Tasks;

namespace RadioOwl.Parsers.IntegrationTests.Parser
{
    /// <summary>
    /// Integracni testy proti ostremu API - pravdepodobne budou fungovat vzdy jen chvili
    /// </summary>
    [TestClass]
    public class MujRozhlas2022ParserTests
    {
        /// <summary>
        /// Testy pro zjisteni seznamu jednotlivych dilu poradu
        /// </summary>
        [TestMethod]
        public async Task GetShowPartsTestsAsync()
        {
            const string RID = "1862045";
    
            var parser = new MujRozhlas2022Parser();

            // vsechny dily
            var nodeCollection =  await parser.GetShowPartsAsync(RID, 0, 99);
            Assert.IsNotNull(nodeCollection);
            Assert.AreEqual(14, nodeCollection.Count);

            // pouze 1. dil
            nodeCollection = await parser.GetShowPartsAsync(RID, 0, 1);
            Assert.IsNotNull(nodeCollection);
            Assert.AreEqual(1, nodeCollection.Count);

            nodeCollection = await parser.GetShowPartsAsync(RID, 0, 2);
            Assert.IsNotNull(nodeCollection);
            Assert.AreEqual(2, nodeCollection.Count);
        }
    }
}
