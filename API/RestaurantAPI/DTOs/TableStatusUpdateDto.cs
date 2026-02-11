public class TableStatusUpdateDto
{
    public bool Status { get; set; }
    public int? EmployeeId { get; set; } // ✅ Nullable, removed when table is free
}