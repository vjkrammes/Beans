using Beans.Common.Enumerations;
using Beans.Common.Interfaces;

using Microsoft.Extensions.Configuration;

using System.Globalization;

namespace Beans.Common;
public class BreakpointManager : IBreakpointManager
{
    private readonly List<Breakpoint> _breakpoints = new();
    private readonly Random _random = new();
    public double Range { get; }
    public Dictionary<string, double>? Multipliers { get; } = new(StringComparer.OrdinalIgnoreCase);
    public IEnumerable<Breakpoint> Breakpoints => _breakpoints;

    public BreakpointManager(Random random) => _random = random;

    public BreakpointManager(IConfiguration configuration, string breakpointsection = "Breakpoints", string multipliersection = "Multipliers")
    {
        if (configuration is null)
        {
            throw new ArgumentNullException(nameof(configuration));
        }
        if (string.IsNullOrWhiteSpace(breakpointsection))
        {
            throw new ArgumentException("Breakpoint section name is required", nameof(breakpointsection));
        }
        if (string.IsNullOrWhiteSpace(multipliersection))
        {
            throw new ArgumentException("Multiplier section name is required", nameof(multipliersection));
        }
        var bpsection = configuration.GetSection(breakpointsection);
        if (bpsection is null)
        {
            throw new InvalidOperationException($"Breakpoint section '{breakpointsection}' not found");
        }
        var mpsection = configuration.GetSection(multipliersection);
        if (mpsection is null)
        {
            throw new InvalidOperationException($"Multiplier section '{multipliersection}' not found");
        }
        var breakpoints = bpsection.Get<BreakpointCollection>();
        if (breakpoints is null || breakpoints.Range <= 0.0)
        {
            throw new InvalidOperationException("Breakpoint section is malformed or the range is invalid");
        }
        Range = breakpoints.Range;
        foreach (var breakpoint in breakpoints.Breakpoints)
        {
            var ix = 0;
            while (ix < _breakpoints.Count && _breakpoints[ix].Value < breakpoint.Value)
            {
                ix++;
            }
            _breakpoints.Insert(ix, breakpoint);
        }
        var multipliers = mpsection.Get<Multiplier[]>();
        if (multipliers is null)
        {
            throw new InvalidOperationException("Multipliers section is malformed");
        }
        foreach (var multiplier in multipliers)
        {
            if (Multipliers.ContainsKey(multiplier.Breakpoint))
            {
                throw new InvalidOperationException($"Duplicate multiplier for breakpoint '{multiplier.Breakpoint}'");
            }
            Multipliers[multiplier.Breakpoint] = multiplier.Value;
        }
        foreach (var breakpoint in _breakpoints)
        {
            if (!Multipliers.ContainsKey(breakpoint.Name))
            {
                Multipliers[breakpoint.Name] = 1.0;
            }
        }
    }

    public string GenerateBreakpoint()
    {
        var rnd = _random.NextDouble() * Range;
        var ix = 0;
        while (ix < _breakpoints.Count && rnd > _breakpoints[ix].Value)
        {
            ix++;
        }
        if (ix >= _breakpoints.Count)
        {
            return _breakpoints.Last().Name;
        }
        return _breakpoints[ix].Name;
    }

    public double GetMultiplier(string breakpoint) => Multipliers!.ContainsKey(breakpoint) ? Multipliers[breakpoint] : 1.0;

    public MovementType GetMovementType(string breakpoint) => breakpoint.ToLower(CultureInfo.CurrentCulture) switch
    {
        "normal" => MovementType.Normal,
        "rare" => MovementType.Rare,
        "epic" => MovementType.Epic,
        "heroic" => MovementType.Heroic,
        _ => MovementType.Unspecified
    };
}
