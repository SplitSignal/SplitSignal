using CollaborativeEscapeRoom.web.Models;
using CollaborativeEscapeRoom.web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
namespace CollaborativeEscapeRoom.web.Pages;
public sealed class IndexModel(GameApiClient api, IConfiguration configuration) : PageModel
{
    [BindProperty] public string SessionCode { get; set; } = "";
    [BindProperty] public string PlayerName { get; set; } = "";
    public string ErrorMessage { get; set; } = "";
    public void OnGet() { }
    public async Task<IActionResult> OnPostAsync()
    {
        if (string.IsNullOrWhiteSpace(SessionCode) || string.IsNullOrWhiteSpace(PlayerName)) { ErrorMessage = "Enter both your session code and player name to continue."; return Page(); }
        var code = SessionCode.Trim(); var name = PlayerName.Trim();
        if (code.Length > 60 || name.Length > 60) { ErrorMessage = "Session code and player name must be 60 characters or fewer."; return Page(); }
        if (configuration.GetValue<bool>("GameApi:DemoMode", true)) return RedirectToPage("/Game", new { sessionId = code, playerName = name });
        try { var result = await api.JoinSessionAsync(new JoinSessionRequest { SessionCode = code, PlayerName = name }); if (result is null || !result.Success) { ErrorMessage = result?.Message ?? "Unable to join the game. Check the session code and try again."; return Page(); } return RedirectToPage("/Game", new { sessionId = result.SessionId, playerName = result.PlayerName }); }
        catch (HttpRequestException) { ErrorMessage = "The game server could not be reached. Check that the backend is running."; return Page(); }
    }
}
