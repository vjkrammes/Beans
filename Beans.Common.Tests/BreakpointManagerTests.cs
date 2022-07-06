
using Beans.Common.Interfaces;

namespace Beans.Common.Tests;

[TestClass]
public class BreakpointManagerTests
{
    private static readonly IConfigurationFactory _configurationFactory = new ConfigurationFactory();
    private IBreakpointManager? _breakpointManager;

    [TestMethod]
    public void TestBreakpointManager()
    {
        _breakpointManager = new BreakpointManager(_configurationFactory.Create(Constants.ConfigurationFilename));
        Assert.IsNotNull(_breakpointManager);
        var bp = _breakpointManager.GenerateBreakpoint();
        Assert.IsFalse(string.IsNullOrWhiteSpace(bp));
        var multiplier = _breakpointManager.GetMultiplier(bp);
        Assert.IsTrue(multiplier > 0.0);
    }
}
