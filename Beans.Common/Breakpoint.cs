namespace Beans.Common;

public class Breakpoint
{
    public double Value { get; set; }
    public string Name { get; set; }

    public Breakpoint()
    {
        Value = 0.0;
        Name = string.Empty;
    }
}
