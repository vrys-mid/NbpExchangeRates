namespace NbpImporter.Nbp;

public class NbpTableResponse
{
    public string Table { get; set; } = null!;
    public string EffectiveDate { get; set; } = null!;
    public List<NbpRate> Rates { get; set; } = new();
}

public class NbpRate
{
    public string Currency { get; set; } = null!;
    public string Code { get; set; } = null!;
    public decimal Mid { get; set; }
}