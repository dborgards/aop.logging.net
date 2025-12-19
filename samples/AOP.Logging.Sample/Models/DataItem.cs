namespace AOP.Logging.Sample.Models;

/// <summary>
/// Data item model.
/// </summary>
public class DataItem
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Value { get; set; }
    public DateTime Timestamp { get; set; }
}
