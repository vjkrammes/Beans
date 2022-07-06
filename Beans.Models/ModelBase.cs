namespace Beans.Models;

public abstract class ModelBase
{
    public bool CanDelete { get; set; }

    public ModelBase() => CanDelete = true;
}
