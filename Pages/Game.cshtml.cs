using CollaborativeEscapeRoom.web.Models;
using CollaborativeEscapeRoom.web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
namespace CollaborativeEscapeRoom.web.Pages;
public sealed class GameModel(GameApiClient api, IConfiguration configuration) : PageModel
{
    public string SessionId { get; private set; } = "";
    public string PlayerName { get; private set; } = "Web player";
    public GameState? GameState { get; private set; }
    public bool DemoMode { get; private set; } = true;
    public async Task<IActionResult> OnGetAsync(string? sessionId, string? playerName)
    {
        if (string.IsNullOrWhiteSpace(sessionId)) return RedirectToPage("/Index");
        SessionId = sessionId.Trim(); PlayerName = string.IsNullOrWhiteSpace(playerName) ? "Web player" : playerName.Trim();
        DemoMode = configuration.GetValue<bool>("GameApi:DemoMode", true);
        if (!DemoMode) { try { GameState = await api.GetGameStateAsync(SessionId); } catch (HttpRequestException) { GameState = null; } }
        return Page();
    }
}
