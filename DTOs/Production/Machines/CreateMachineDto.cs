namespace NavetraERP.DTOs;

public class CreateMachineDto
{
    public string Name { get; set; } = String.Empty;
    public string Code { get; set; } = String.Empty;
    public string? Description { get; set; } = null;
    public bool Active { get; set; }
}