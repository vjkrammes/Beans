using Beans.Common.Attributes;

using System.ComponentModel;

namespace Beans.Common.Enumerations;

public enum Level
{
    [Description("None")]
    [TextColor("Black")]
    NoLevel = 0,
    [Description("Debug")]
    [TextColor("Black")]
    Debug = 1,
    [Description("Info")]
    [TextColor("Black")]
    Information = 2,
    [Description("Warn")]
    [TextColor("Yellow")]
    Warning = 3,
    [Description("Error")]
    [TextColor("Red")]
    Error = 4,
    [Description("Crit")]
    [TextColor("Red")]
    Critical = 5,
    [Description("Fatal")]
    [TextColor("Red", "Yellow")]
    Fatal = 6
}
