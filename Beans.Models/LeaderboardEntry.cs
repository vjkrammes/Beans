namespace Beans.Models;
public class LeaderboardEntry
{
    public string UserId { get; set; }
    public string DisplayName { get; set; }
    public long Quantity { get; set; }
    public decimal Basis { get; set; }
    public decimal Value { get; set; }
    public decimal GainOrLoss { get; set; }
    public double Change { get; set; }
    public decimal SalesBasis { get; set; }
    public decimal SalesTotal { get; set; }
    public decimal SalesGainOrLoss { get; set; }
    public double SalesChange { get; set; }
    public double Score { get; set; }

    public LeaderboardEntry()
    {
        UserId = string.Empty;
        DisplayName = string.Empty;
        Quantity = 0;
        Basis = 0M;
        Value = 0M;
        GainOrLoss = 0M;
        Change = 0.0;
        SalesBasis = 0M;
        SalesTotal = 0M;
        SalesGainOrLoss = 0M;
        SalesChange = 0.0;
        Score = 0.0;
    }
}
