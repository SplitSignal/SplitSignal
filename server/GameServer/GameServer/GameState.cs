using GameServer;
using MongoDB.Driver;

namespace GameServer
{
    public class GameState
    {
        private readonly IMongoCollection<GameSession> _sessions;

        public GameState(IMongoDatabase database)
        {
            _sessions =
                database.GetCollection<GameSession>("Sessions");
        }

        public async Task CreateSession(GameSession session)
        {

            await _sessions.InsertOneAsync(session);
        }

        public async Task<GameSession?> GetSession(string sessionCode)
        {
            return await _sessions
                .Find(x => x.SessionCode == sessionCode)
                .FirstOrDefaultAsync();
        }

        public async Task UpdateState(string sessionCode, string objectName, string status)
        {
            var filter = Builders<GameSession>.Filter
                .Eq(x => x.SessionCode, sessionCode);

            var update = Builders<GameSession>.Update
                .Set($"State.{objectName}.Status", status);

            await _sessions.UpdateOneAsync(filter, update);
        }

        public async Task<UpdateClient> CheckState(string sessionCode, string objectName)
        {
            var filter = Builders<GameSession>.Filter.And(
                Builders<GameSession>.Filter.Eq("_id", sessionCode),
                Builders<GameSession>.Filter.Exists($"status.{objectName}")
                );

            if (filter != null)
            {
                var state = _sessions.Find(filter).FirstOrDefaultAsync();

                UpdateClient result = new UpdateClient();
                
                result.Object = objectName;
                result.Status = "Temp";

                return result;
            }

            return null;
        }

    }
}