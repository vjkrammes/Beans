
using Beans.Common.Enumerations;

namespace Beans.Common.Tests;

[TestClass]
public class EnumerationTests
{
    [TestMethod]
    public void TestSortField() => Assert.IsTrue(SortField.Date.HasFlag(SortField.DateAscending));
}
