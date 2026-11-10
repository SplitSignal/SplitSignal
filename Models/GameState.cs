namespace CollaborativeEscapeRoom.web.Models;
public sealed class GameState { public string SessionId { get; set; } = ""; public string PuzzleName { get; set; } = "Winter Maze"; public string Instructions { get; set; } = "Find the hidden symbol and share it with the Unity player."; public string Status { get; set; } = "In progress"; public int RemainingSeconds { get; set; } = 300; }
