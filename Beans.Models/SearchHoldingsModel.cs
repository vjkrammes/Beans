namespace Beans.Models;
public class SearchHoldingsModel
{
    public string Id { get; set; }
    public string BeanId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    public SearchHoldingsModel()
    {
        Id = string.Empty;
        BeanId = string.Empty;
        StartDate = default;
        EndDate = default;
    }
}
