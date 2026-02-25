namespace NavetraERP.DTOs;

public class ProductionOrderMachineDto
{
    public int Id { get; set; }
    public int ProductionOrderId { get; set; }
    public int MachineId { get; set; }
    public string MachineName { get; set; } = String.Empty;
    public string MachineCode { get; set; } = String.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}