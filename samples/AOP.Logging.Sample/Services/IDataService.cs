using AOP.Logging.Sample.Models;

namespace AOP.Logging.Sample.Services;

/// <summary>
/// Data service interface.
/// </summary>
public interface IDataService
{
    Task<List<DataItem>> GetDataAsync(int[] ids);
    Task<DataItem> ProcessDataAsync(DataItem item);
}
