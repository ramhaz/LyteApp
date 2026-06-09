namespace LyteApp.Models;

public class AddRunRequest
{
    public int PlanId { get; set; }
    public int DayNumber { get; set; }
    public decimal AmountKm { get; set; }
}
