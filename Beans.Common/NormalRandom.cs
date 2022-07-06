
using Beans.Common.Interfaces;

namespace Beans.Common;

// based on Box-Muller transform and the code in this question on StackOverflow: https://stackoverflow.com/questions/218060/random-gaussian-variables

public class NormalRandom : INormalRandom
{
    private readonly Random _random = new();
    private readonly double _mu;
    private readonly double _sigma;

    public NormalRandom(double mu = 0.0, double sigma = 1.0)
    {
        _mu = mu;
        _sigma = sigma;
    }

    public double Next()
    {
        var r1 = 1.0 - _random.NextDouble();    // uniform distribution over (0,1]
        var r2 = 1.0 - _random.NextDouble();    // same
        var normal = Math.Sqrt(-2.0 * Math.Log(r1)) * Math.Sin(2.0 * Math.PI * r2);   // normal distribution over (0,1]
        return _mu + _sigma * normal;
    }
}
