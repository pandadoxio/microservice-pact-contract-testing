using Microsoft.OpenApi;
using OrderService.Api.Endpoints;
using OrderService.Application;
using OrderService.Infrastructure;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer()
    .AddOpenApi(options =>
    {
        options.AddDocumentTransformer((document, context, ct) =>
        {
            document.Info = new OpenApiInfo
            {
                Title = "Order Service API",
                Version = "v1",
                Description = "Places orders for products in the catalogue"
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
app.MapOrderEndpoints();

app.Run();
