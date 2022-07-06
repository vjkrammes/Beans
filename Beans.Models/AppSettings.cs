namespace Beans.Models;
public class AppSettings
{
    public string ApiBase { get; set; }
    public string ApiKey { get; set; }
    public string ImageBase { get; set; }
    public string ImageDirectory { get; set; }
    public string InternalApiKey { get; set; }
    public string LongTermSpan { get; set; }
    public decimal MinimumValue { get; set; }
    public double Mu { get; set; } // median for normal distribution random number generator
    public bool OldestFirst { get; set; }
    public double Sigma { get; set; } // standard deviation for random
    public bool UpdateDatabase { get; set; }

    public AppSettings()
    {
        ApiBase = string.Empty;
        ApiKey = string.Empty;
        ImageBase = string.Empty;
        ImageDirectory = string.Empty;
        InternalApiKey = string.Empty;
        LongTermSpan = "1y";
        MinimumValue = 0.01M;
        Mu = 0.0;
        OldestFirst = true;
        Sigma = 2.0;
        UpdateDatabase = false;
    }
}
