using AOP.Logging.Core.Attributes;
using AOP.Logging.Sample.Models;

namespace AOP.Logging.Sample.Services;

/// <summary>
/// Data service demonstrating collection handling.
/// The Source Generator creates public wrapper methods that call these private Core methods.
/// </summary>
[LogClass]
public partial class DataService : IDataService
{
    /// <summary>
    /// Core implementation: Gets data items by IDs (demonstrates collection logging).
    /// </summary>
    private async Task<List<DataItem>> GetDataAsyncCore([LogParameter(MaxLength = 10)] int[] ids)
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
    /// Core implementation: Processes a data item.
    /// </summary>
    [LogResult(Name = "ProcessedItem")]
    private async Task<DataItem> ProcessDataAsyncCore(DataItem item)
    {
        await Task.Delay(50); // Simulate async work

        item.Value *= 2;
        item.Timestamp = DateTime.UtcNow;

        return item;
    }
}
