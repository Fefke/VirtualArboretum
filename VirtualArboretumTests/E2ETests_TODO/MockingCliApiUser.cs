using Microsoft.VisualStudio.TestPlatform.TestHost;
using VirtualArboretum;

namespace VirtualArboretumTests;

[TestClass]
public class MockingCliApiUser
{
    [TestMethod]
    public void TestBackendServerStartup()
    {
        var serverTask = VirtualArboretum.Program.Main(["serve"]);

        var clientTask = VirtualArboretum.Program.Main(["help"]);

    }
}
