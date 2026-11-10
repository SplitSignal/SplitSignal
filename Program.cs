using CollaborativeEscapeRoom.web.Services;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorPages();
builder.Services.AddSingleton<DemoGameService>();
builder.Services.AddHttpClient<GameApiClient>();
var app = builder.Build();
if (!app.Environment.IsDevelopment()) { app.UseExceptionHandler("/Error"); app.UseHsts(); }
app.UseHttpsRedirection(); app.UseStaticFiles(); app.UseRouting(); app.MapRazorPages();
app.MapPost("/api-proxy/game-action", async (GameActionRequest request, IConfiguration config, DemoGameService demo, GameApiClient api) => {
    if (config.GetValue<bool>("GameApi:DemoMode", true)) { demo.RecordAction(request); return Results.Ok(new { success = true, message = $"Demo action recorded: {request.ActionId}" }); }
    try { var ok = await api.SendActionAsync(request); return ok ? Results.Ok(new { success = true, message = "Action forwarded to game server." }) : Results.BadRequest(new { success = false, message = "Game server rejected the action." }); }
    catch (HttpRequestException) { return Results.Problem(title: "Game server unavailable", detail: "The backend API could not be reached.", statusCode: 503); }
});
app.MapGet("/api-proxy/demo-state", (DemoGameService demo) => Results.Ok(demo.GetSnapshot()));
app.Run();
public sealed class GameActionRequest { public string ActionId { get; set; } = ""; public string PuzzleId { get; set; } = "WINTER-MAZE"; public string SessionId { get; set; } = ""; public string ObjectId { get; set; } = ""; public string Value { get; set; } = ""; }
