namespace NavetraERP.DTOs;

public class CreateProductionOrderDto
{
    public int ProductId { get; set; }
    public decimal PlannedQuantity { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Status { get; set; } = String.Empty;
    public int ResponsibleEmployeeId { get; set; }
    public int FromWarehouseId { get; set; }
    public List<ProductionConsumptionDto> Components { get; set; } = new List<ProductionConsumptionDto>();
    public List<ProductionOrderMachineDto> Machines { get; set; } = new List<ProductionOrderMachineDto>();
}