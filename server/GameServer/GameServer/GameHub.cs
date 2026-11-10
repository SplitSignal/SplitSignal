using Microsoft.AspNetCore.SignalR;
using System.Text.RegularExpressions;

namespace GameServer
{
    public class GameHub : Hub
    {
        private readonly GameState _gameState;

        public GameHub(GameState gameState)
        {
            _gameState = gameState;
        }

        // used by the web part to join an existing session
        public async Task JoinSession(string sessionCode, string clientType)
        {
            Console.WriteLine($"JoinSession called with {sessionCode} from {clientType}");

            if (_gameState.GetSession(sessionCode) == null)
            {
                Console.WriteLine($"JoinSession failed due to invalid session code.");
            }

            await Groups.AddToGroupAsync(
                Context.ConnectionId,
                $"{sessionCode}_{clientType}"
            );

            Console.WriteLine("JoinSession added web to groups");


        }

        // used by Unity to create a session
        public async Task CreateSession(string sessionCode, string clientType)
        {
            await Groups.AddToGroupAsync(
                Context.ConnectionId,
                sessionCode
            );

            await Groups.AddToGroupAsync(
                Context.ConnectionId,
                $"{sessionCode}_{clientType}"
            );

            GameSession temp = new GameSession
            {
                SessionCode = sessionCode
            };

            await _gameState.CreateSession(temp);

        }
    }
}
