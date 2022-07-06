namespace Beans.Common.Tests;

[TestClass]
public class MathSupportTests
{
    private readonly List<int> _list1 = new()
  {
    1,
    2,
    3,
    4,
    5,
    6,
    7
  };

    private readonly List<int> _list2 = new()
  {
    100,
    23,
    25,
    93,
    50
  };

    [TestMethod]
    public void MedianTest()
    {
        var result = _list1.Median();
        Assert.AreEqual(4, result);
        result = _list2.Median();
        Assert.AreEqual(50, result);
        var dresult = _list2.Median(x => (double)x);
        Assert.AreEqual(50.0, dresult);
    }
}
