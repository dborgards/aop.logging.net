using AOP.Logging.Core.Attributes;
using AOP.Logging.Sample.Models;

namespace AOP.Logging.Sample.Services;

/// <summary>
/// Data service demonstrating collection handling.
/// </summary>
[LogClass]
public partial class DataService : IDataService
{
    /// <summary>
    /// Gets data items by IDs (demonstrates collection logging).
    /// </summary>
    public async Task<List<DataItem>> GetDataAsync([LogParameter(MaxLength = 10)] int[] ids)
    {
        await Task.Delay(100); // Simulate async work

        var items = ids.Select(id => new DataItem
        {
            Id = id,
            Name = $"Item {id}",
            Value = id * 100,
            Timestamp = DateTime.UtcNow
        }).ToList();

        return items;
    }

    /// <summary>
    /// Processes a data item.
    /// </summary>
    [LogResult(Name = "ProcessedItem")]
    public async Task<DataItem> ProcessDataAsync(DataItem item)
    {
        await Task.Delay(50); // Simulate async work

        item.Value *= 2;
        item.Timestamp = DateTime.UtcNow;

        return item;
    }
}
