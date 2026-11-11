using GameServer;
using MongoDB.Driver;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

var mongoClient = new MongoClient(
    builder.Configuration["MongoDB:ConnectionString"]);

var database = mongoClient.GetDatabase(
    builder.Configuration["MongoDB:DatabaseName"]);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();
builder.Services.AddSignalR();
builder.Services.AddSingleton<GameState>();
builder.Services.AddSingleton<IMongoDatabase>(database);
builder.Services.AddCors(options =>
{
    options.AddPolicy("WebClient", policy =>
    {
        policy
            .WithOrigins(["https://localhost:7069", "https://localhost:32769"])
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = true;
});
builder.Logging.AddFilter(
    "Microsoft.AspNetCore.SignalR",
    LogLevel.Trace
);
builder.Logging.AddFilter(
    "Microsoft.AspNetCore.Http.Connections",
    LogLevel.Trace
);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi("/openapi/v1.json");
    app.MapScalarApiReference();
}

app.UseStaticFiles();

app.UseHttpsRedirection();

app.UseCors("WebClient");

app.UseAuthorization();

app.MapControllers();

app.MapHub<GameHub>("/gamehub");

app.MapRazorPages();

app.MapGet("/debug/routes", (EndpointDataSource source) =>
    source.Endpoints
        .OfType<RouteEndpoint>()
        .Select(e => e.RoutePattern.RawText)
);

app.Run();
