using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

namespace ProductService.ContractTests.Infrastructure;

/// <summary>
///     Injects ProviderStateMiddleware at the start of the pipeline
/// </summary>
public class ProviderStateStartupFilter : IStartupFilter
{
    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next) =>
        app =>
        {
            // Register provider state middleware first so it intercepts
            // POST /provider-states before routing sees the request
            app.UseMiddleware<ProviderStateMiddleware>();

            // Continue with the rest of the pipeline from Program.cs
            next(app);
        };
}
