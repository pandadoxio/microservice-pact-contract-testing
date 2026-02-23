using Microsoft.OpenApi;
using ProductService.Api.Endpoints;
using ProductService.Application;
using ProductService.Infrastructure;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer()
                .AddOpenApi(options =>
                {
                    options.AddDocumentTransformer((document, context, ct) =>
                    {
                        document.Info = new OpenApiInfo
                        {
                            Title       = "Product Service API",
                            Version     = "v1",
                            Description = "Manages the product catalogue and stock levels"
                        };
                        return Task.CompletedTask;
                    });
                });

builder.Services.AddApplication()
    .AddInfrastructure();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();
app.MapProductEndpoints();

app.Run();
