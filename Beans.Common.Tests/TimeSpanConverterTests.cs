
using Beans.Common.Interfaces;

namespace Beans.Common.Tests;

[TestClass]
public class TimeSpanConverterTests
{
    [TestMethod]
    public void TestTimeSpanConverter()
    {
        var msecSpec = "15msec";
        var secSpec = "12 seconds";
        var minSpec = "3m";
        var hourSpec = "2 h";
        var daySpec = "3 day";
        var weekSpec = "1w";
        var yearSpec = "2 years";

        ITimeSpanConverter converter = new TimeSpanConverter();

        var msec = converter.Convert(msecSpec);
        var sec = converter.Convert(secSpec);
        var min = converter.Convert(minSpec);
        var hour = converter.Convert(hourSpec);
        var day = converter.Convert(daySpec);
        var week = converter.Convert(weekSpec);
        var year = converter.Convert(yearSpec);

        Assert.AreEqual(TimeSpan.FromMilliseconds(15), msec);
        Assert.AreEqual(TimeSpan.FromSeconds(12), sec);
        Assert.AreEqual(TimeSpan.FromMinutes(3), min);
        Assert.AreEqual(TimeSpan.FromHours(2), hour);
        Assert.AreEqual(TimeSpan.FromDays(3), day);
        Assert.AreEqual(TimeSpan.FromDays(1 * 7), week);
        Assert.AreEqual(TimeSpan.FromDays(2 * 365), year);
    }
}
