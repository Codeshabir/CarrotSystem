namespace CarrotSystem.Dto;

public class DashboardDto
{
    public int Purchase { get; set; }
    public int Sale { get; set; }
    public int Expense { get; set; }
    public int Stock { get; set; }
    public List<string> SalesLabels { get; set; }
    public List<int> SalesValues { get; set; }
    public List<string> PurchaseLabels { get; set; }
    public List<int> PurchaseValues { get; set; }
}
