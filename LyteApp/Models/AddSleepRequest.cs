namespace LyteApp.Models;

public class AddSleepRequest
{
    public int PlanId { get; set; }
    public int DayNumber { get; set; }
    public decimal Hours { get; set; }
}
