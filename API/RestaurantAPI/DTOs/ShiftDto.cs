public class ShiftDto
{
    public int ReportId { get; set; }       
    public int EmployeeId { get; set; }  
    public DateTime StartTime { get; set; }  
    public DateTime? EndTime { get; set; }   // Nullable (until shift ends)
}