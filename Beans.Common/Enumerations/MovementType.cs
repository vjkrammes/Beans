using Beans.Common.Attributes;

using System.ComponentModel;

namespace Beans.Common.Enumerations;
public enum MovementType
{
    [Description("Unspecified")]
    Unspecified = 0,
    [Description("Normal")]
    [Color("Black")]
    Normal = 1,
    [Description("Rare")]
    [Color("Green")]
    Rare = 2,
    [Description("Epic")]
    [Color("Purple")]
    Epic = 3,
    [Description("Heroic")]
    [Color("Orange")]
    Heroic = 4
}
