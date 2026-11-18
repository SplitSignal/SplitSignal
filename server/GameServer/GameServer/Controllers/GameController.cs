using GameServer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Linq.Expressions;
using System.Security.AccessControl;
using System.Text.Json;
using static System.Net.Mime.MediaTypeNames;


namespace GameServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class GameController : ControllerBase
    {
        private readonly IHubContext<GameHub> _hubContext;
        private readonly GameState _gameState;

        public GameController(IHubContext<GameHub> hubContext, GameState gameState)
        {
            _hubContext = hubContext;
            _gameState = gameState;
        }

        // just for testing
        // remove when db is integrated
        public static List<string> TestList = new List<string>();

        [HttpGet(Name = "GetList")]
        public IEnumerable<string> Get()
        {
            return GameController.TestList;
        }
        // end remove when db is integrate

        [HttpPost("Update")]
        public async Task<IActionResult> Update([FromBody] UpdateRequest req)
        {
            Console.WriteLine($"Connected to session {req.SessionCode} updating object {req.Object} with status {req.Status} to notify {req.Notify}");
            
            // remove when db is integrated
            GameController.TestList.Add(req.Object);

            Console.WriteLine(TestList.Count);
            // end remove when db is integrated

            await _gameState.UpdateState
            (
                req.SessionCode,
                req.Object,
                req.Status
            );

            Console.WriteLine($"This connection is using Session ID: {req.SessionCode}");

            // send to client if notify requires it
            if(req.Notify == "Both")
            {
                UpdateClient msg = new UpdateClient
                {
                    Object = req.Object,
                    Status = req.Status
                };

                Console.WriteLine($"Confirming message content for SignalR to Both: {msg.Object} and {msg.Status}");

                await _hubContext.Clients.Group($"{req.SessionCode}_Unity").SendAsync(req.Object, msg);
                await _hubContext.Clients.Group($"{req.SessionCode}_Web").SendAsync(req.Object, msg);

                Console.WriteLine($"Confirming message was sent for SignalR to Both: {msg.Object} and {msg.Status}");
            }
            else if(req.Notify == "Unity")
            {
                UpdateClient msg = new UpdateClient
                {
                    Object = req.Object,
                    Status = req.Status
                };

                Console.WriteLine($"Confirming message content for SignalR to Unity: {msg.Object} and {msg.Status}");

                await _hubContext.Clients.Group($"{req.SessionCode}_Unity").SendAsync(req.Object, msg);

                Console.WriteLine($"Confirming message was sent for SignalR to Unity: {msg.Object} and {msg.Status}");
            }
            else if(req.Notify == "Web")
            {
                UpdateClient msg = new UpdateClient
                {
                    Object = req.Object,
                    Status = req.Status
                };

                Console.WriteLine($"Confirming message content for SignalR to Web: {msg.Object} and {msg.Status}");

                await _hubContext.Clients.Group($"{req.SessionCode}_Web").SendAsync(req.Object, msg);

                Console.WriteLine($"Confirming message was sent for SignalR to Web: {msg.Object} and {msg.Status}");
            }
            else
            {
                Console.WriteLine($"Notify value: {req.Notify}. Either not valid or not specified.");
            }

            return Ok("Received");
        }

        // if an action requires validating state of one or more items
        [HttpGet("Check")]
        public async Task<IActionResult> Check([FromBody] CheckRequest req)
        {
            Console.WriteLine($"Connected to session {req.SessionCode} with message {req.Object}");

            Console.WriteLine($"This connection is using Session ID: {req.SessionCode}");

            UpdateClient result = await _gameState.CheckState(req.SessionCode, req.Object);

            Console.WriteLine($"Back in the controller, result: {result.Object} and {result.Status}.");

            return Ok(result);
        }
    }
}
