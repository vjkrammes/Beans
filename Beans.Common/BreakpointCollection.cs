namespace Beans.Common;

public class BreakpointCollection
{
    public double Range { get; set; }
    public Breakpoint[] Breakpoints { get; set; }

    public BreakpointCollection()
    {
        Range = 1000.0;
        Breakpoints = Array.Empty<Breakpoint>();
    }
}
