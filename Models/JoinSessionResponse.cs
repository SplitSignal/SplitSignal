namespace CollaborativeEscapeRoom.web.Models;
public sealed class JoinSessionResponse { public string SessionId { get; set; } = ""; public string PlayerName { get; set; } = ""; public bool Success { get; set; } public string Message { get; set; } = ""; }
