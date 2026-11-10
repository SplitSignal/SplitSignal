using System.Net.Http.Json;
using CollaborativeEscapeRoom.web.Models;
namespace CollaborativeEscapeRoom.web.Services;
public sealed class GameApiClient(HttpClient httpClient, IConfiguration configuration)
{
    public async Task<JoinSessionResponse?> JoinSessionAsync(JoinSessionRequest request) { using var response = await httpClient.PostAsJsonAsync(Required("GameApi:JoinSessionEndpoint"), request); if (!response.IsSuccessStatusCode) return new() { Success = false, Message = "The game server could not accept this session." }; return await response.Content.ReadFromJsonAsync<JoinSessionResponse>(); }
    public async Task<GameState?> GetGameStateAsync(string sessionId) { var endpoint = Required("GameApi:GetGameStateEndpoint").Replace("{sessionId}", Uri.EscapeDataString(sessionId), StringComparison.OrdinalIgnoreCase); return await httpClient.GetFromJsonAsync<GameState>(endpoint); }
    public async Task<bool> SendActionAsync(GameActionRequest request) { using var response = await httpClient.PostAsJsonAsync(Required("GameApi:SendActionEndpoint"), request); return response.IsSuccessStatusCode; }
    private string Required(string key) { var value = configuration[key]; if (string.IsNullOrWhiteSpace(value)) throw new InvalidOperationException($"Missing setting: {key}"); if (Uri.TryCreate(configuration["GameApi:BaseUrl"], UriKind.Absolute, out var baseUri)) httpClient.BaseAddress = baseUri; return value; }
}
