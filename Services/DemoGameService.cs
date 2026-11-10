using System.Collections.Concurrent;
namespace CollaborativeEscapeRoom.web.Services;
public sealed class DemoGameService { private readonly ConcurrentQueue<object> _actions = new(); public void RecordAction(GameActionRequest request) { _actions.Enqueue(new { request.ActionId, request.PuzzleId, request.SessionId, request.ObjectId, request.Value, recordedAt = DateTimeOffset.Now.ToString("HH:mm:ss") }); while (_actions.Count > 30) _actions.TryDequeue(out _); } public object GetSnapshot() => new { mode = "Demo", actions = _actions.ToArray() }; }
