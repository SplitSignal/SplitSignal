using MongoDB.Bson.Serialization.Attributes;

namespace GameServer
{
    public class GameSession
    {
        [BsonId]
        public string SessionCode { get; set; }

        public Dictionary<string, GameObjectState> State { get; set; } = new();
    }

    public class GameObjectState
    {
        public string Status { get; set; }
    }
}