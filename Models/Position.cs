namespace NavetraERP.Models;

public class Position
{
    public int Id { get; set; }
    public string PositionName { get; set; } = String.Empty;
    public string? Description { get; set; } = String.Empty;
}