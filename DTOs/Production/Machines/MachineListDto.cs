namespace NavetraERP.DTOs;

public class MachineListDto
{
    public int Id { get; set; }
    public string Name { get; set; } = String.Empty;
    public string Code { get; set; } = String.Empty;
    public bool Active { get; set; }
}