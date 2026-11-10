using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR.Client;
using System.Diagnostics;
using System.Security.Cryptography;

namespace GameServer.Pages
{
    public class IndexModel : PageModel
    {
        public string sessionCode_create { get; set; }

        [BindProperty]
        public string sessionCode_join { get; set; }
        private HubConnection conn;

        public IndexModel()
        {
            conn = new HubConnectionBuilder()
                .WithUrl("/gamehub")
                .WithAutomaticReconnect()
                .Build();

        }

        private static string GenerateSessionCode()
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
            const int length = 8;

            return new string(Enumerable.Range(0, length)
                .Select(_ => chars[RandomNumberGenerator.GetInt32(chars.Length)])
                .ToArray());
        }
        public void OnGet()
        {
        }

        public async void OnPostCreateSession()
        {
            sessionCode_create = GenerateSessionCode();

            // open persistent conn
            await conn.StartAsync();

            await conn.InvokeAsync(
                "CreateSession",
                sessionCode_create,
                "Unity"
            );

            Console.WriteLine($"Joining as Unity with session code: {sessionCode_create}");
        }

        public async void OnPostJoinSession()
        {
            // open persistent conn
            await conn.StartAsync();

            await conn.InvokeAsync(
                "JoinSession",
                sessionCode_join,
                "Web"
            );
            Console.WriteLine($"Joining as Web with session code: {sessionCode_join}");
        }
    }
}
