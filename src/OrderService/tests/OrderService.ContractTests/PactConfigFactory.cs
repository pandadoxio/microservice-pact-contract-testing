using System.Text.Json;
using PactNet;
using PactNet.Infrastructure.Outputters;
using PactNet.Verifier;

namespace OrderService.ContractTests;

internal static class PactConfigFactory
{
    public static string PactDir
    {
        get
        {
            var directory = new DirectoryInfo(AppContext.BaseDirectory);
            while (directory is not null && !directory.GetFiles("*.slnx").Any())
            {
                directory = directory.Parent;
            }

            if (directory == null)
            {
                throw new InvalidOperationException("Solution root not found.");
            }

            return Path.Combine(directory.FullName, "pacts");
        }
    }

    public static PactConfig ConsumerConfig() => new()
    {
        PactDir = PactDir,
        DefaultJsonSettings = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }
    };

    public static PactVerifierConfig VerifierConfig() => new()
    {
        Outputters = new List<IOutput> { new ConsoleOutput() }
    };
}
