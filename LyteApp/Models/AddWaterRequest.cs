namespace LyteApp.Models;


    public class AddWaterRequest
    {
        public int PlanId { get; set; }
        public int DayNumber { get; set; }
        public int AmountMl { get; set; }
    }
