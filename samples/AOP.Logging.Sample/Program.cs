using AOP.Logging.DependencyInjection;
using AOP.Logging.Sample.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AOP.Logging.Sample;

/// <summary>
/// Sample application demonstrating the AOP Logging framework.
/// </summary>
public class Program
{
    public static async Task Main(string[] args)
    {
        var host = Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
            {
                // Add AOP logging with custom configuration
                services.AddAopLogging(options =>
                {
                    options.DefaultLogLevel = LogLevel.Information;
                    options.LogExecutionTime = true;
                    options.LogParameters = true;
                    options.LogReturnValues = true;
                    options.LogExceptions = true;
                    options.MaxStringLength = 500;
                    options.MaxCollectionSize = 5;

                    // Configure which namespaces to log
                    options.IncludedNamespaces.Add("AOP.Logging.Sample");
                });

                // Register services with logging
                services.AddTransientWithLogging<ICalculatorService, CalculatorService>();
                services.AddScopedWithLogging<IUserService, UserService>();
                services.AddSingletonWithLogging<IDataService, DataService>();
            })
            .ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddConsole();
                logging.SetMinimumLevel(LogLevel.Trace);
            })
            .Build();

        // Run examples
        await RunExamples(host.Services);
    }

    private static async Task RunExamples(IServiceProvider services)
    {
        using var scope = services.CreateScope();

        Console.WriteLine("=== AOP Logging Framework Demo ===\n");

        // Example 1: Basic method logging
        Console.WriteLine("--- Example 1: Basic Calculator Operations ---");
        var calculator = scope.ServiceProvider.GetRequiredService<ICalculatorService>();
        var addResult = calculator.Add(5, 3);
        Console.WriteLine($"Result: {addResult}\n");

        var multiplyResult = calculator.Multiply(4, 7);
        Console.WriteLine($"Result: {multiplyResult}\n");

        // Example 2: Async method logging
        Console.WriteLine("--- Example 2: Async Operations ---");
        var asyncResult = await calculator.CalculateAsync(10, 2);
        Console.WriteLine($"Result: {asyncResult}\n");

        // Example 3: Exception logging
        Console.WriteLine("--- Example 3: Exception Handling ---");
        try
        {
            calculator.Divide(10, 0);
        }
        catch (DivideByZeroException)
        {
            Console.WriteLine("Exception was logged and re-thrown\n");
        }

        // Example 4: User service with sensitive data
        Console.WriteLine("--- Example 4: Sensitive Data Handling ---");
        var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
        var user = await userService.CreateUserAsync("john.doe@example.com", "MySecretPassword123!");
        Console.WriteLine($"User created: {user.Email}\n");

        var authenticated = await userService.AuthenticateAsync("john.doe@example.com", "MySecretPassword123!");
        Console.WriteLine($"Authentication result: {authenticated}\n");

        // Example 5: Data service with collections
        Console.WriteLine("--- Example 5: Collection Handling ---");
        var dataService = scope.ServiceProvider.GetRequiredService<IDataService>();
        var data = await dataService.GetDataAsync(new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 });
        Console.WriteLine($"Data items received: {data.Count}\n");

        // Example 6: Custom log levels
        Console.WriteLine("--- Example 6: Custom Log Levels ---");
        calculator.PerformComplexCalculation(100);
        Console.WriteLine();

        Console.WriteLine("=== Demo Complete ===");
    }
}
