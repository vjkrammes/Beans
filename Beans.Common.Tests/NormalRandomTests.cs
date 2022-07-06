
using Beans.Common.Interfaces;

namespace Beans.Common.Tests;

[TestClass]
public class NormalRandomTests
{
    private const double _mu = 0.0;
    private const double _sigma = 2.0;

    private readonly INormalRandom _random = new NormalRandom(_mu, _sigma);

    [TestMethod]
    public void TestNormalRandom()
    {
        Dictionary<int, int> buckets = new();
        List<double> values = new();
        var lowestvalue = double.MaxValue;
        var highestvalue = double.MinValue;
        for (var i = 0; i < 100000; i++)
        {
            var r = _random.Next();
            if (r < lowestvalue)
            {
                lowestvalue = r;
            }
            if (r > highestvalue)
            {
                highestvalue = r;
            }
            values.Add(r);
            var ix = (int)Math.Floor(r * 100);
            if (!buckets.ContainsKey(ix))
            {
                buckets[ix] = 1;
            }
            else
            {
                buckets[ix]++;
            }
        }
        Console.WriteLine($"Highest value = {highestvalue}, lowest value = {lowestvalue}");
        var sorted = buckets
          .OrderBy(x => x.Value)
          .ThenByDescending(x => Math.Abs(x.Key));
        var highest = sorted.Last();
        var lowest = sorted.First();
        Console.WriteLine($"Lowest bucket = [{lowest.Key}] = {lowest.Value}");
        Console.WriteLine($"Highest bucket = [{highest.Key}] = {highest.Value}");
        var avg = values.Average();
        var std = values.StandardDeviation();
        Console.WriteLine($"Avg={avg}, Std={std}, Mu={_mu}, Sigma={_sigma}");
        Assert.IsTrue(Math.Abs(avg) - _mu < 0.01);
        Assert.IsTrue(Math.Abs(std) - _sigma < 0.01);
    }
}
