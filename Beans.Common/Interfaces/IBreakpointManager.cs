
using Beans.Common.Enumerations;

namespace Beans.Common.Interfaces;
public interface IBreakpointManager
{
    double Range { get; }
    string GenerateBreakpoint();
    double GetMultiplier(string breakpoint);
    MovementType GetMovementType(string breakpoint);
    Dictionary<string, double>? Multipliers { get; }
    IEnumerable<Breakpoint>? Breakpoints { get; }
}
