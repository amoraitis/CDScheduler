using Microsoft.VisualStudio.TestTools.UnitTesting;
using CDScheduler;

namespace CDScheduler.Test
{
    [TestClass]
    public class MonitorServiceTest
    {

        [TestMethod]
        public void GetIdFromCsprojTest()
        {
            MonitorService monitorService = new MonitorService();
            monitorService.ImportSettings();
            Assert.AreEqual("EclassApi.1.1.10" , monitorService.GetIdFromCsproj());
        }

        [TestMethod]
        public void CreatePackageTest()
        {
            MonitorService monitorService = new MonitorService();
            monitorService.ImportSettings();
            monitorService.CreatePackage();
        }
    }
}
