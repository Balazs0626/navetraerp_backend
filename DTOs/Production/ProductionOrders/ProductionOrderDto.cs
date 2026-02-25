namespace NavetraERP.DTOs;

public class ProductionOrderDto
{
    public int Id { get; set; }
    public string ReceiptNumber { get; set; } = String.Empty;
    public int ProductId { get; set; }
    public string ProductName { get; set; } = String.Empty;
    public string ProductUnit { get; set; } = String.Empty;
    public decimal PlannedQuantity { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Status { get; set; } = String.Empty;
    public int ResponsibleEmployeeId { get; set; }
    public string ResponsibleEmployeeName { get; set; } = String.Empty;
    public List<ProductionConsumptionDto> Components { get; set; } = new List<ProductionConsumptionDto>();
    public List<ProductionOrderMachineDto> Machines { get; set; } = new List<ProductionOrderMachineDto>();
}