using System.Text.Json.Serialization;

namespace StarlingRoundUpChallenge.Response;

public class CurrencyAndAmount
{
    public string currency { get; set; }
    public int minorUnits { get; set; }
}