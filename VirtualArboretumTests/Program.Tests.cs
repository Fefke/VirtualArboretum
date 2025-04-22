using Microsoft.VisualStudio.TestPlatform.TestHost;

namespace VirtualArboretumTests
{
    [TestClass]
    public sealed class Test2
    {
        [TestMethod]
        public void ShouldPlantWebsite()
        {
            var args = new string[]
            {
                "plant",
                "tree",
                "https://de.wikipedia.org/wiki/Arboretum",
                "check",
                "daily"
            };

            Program.Main(args);

            // How to test? - should create tree in file, yet no return..

            //Assert.
        }

        [TestMethod]
        public void ShouldUpdateTrees()
        {
            var args = new string[]
            {
                "plant",
                "tree",
                "https://de.wikipedia.org/wiki/Arboretum",
                "check",
                "daily"
            };
        }
    }
}
