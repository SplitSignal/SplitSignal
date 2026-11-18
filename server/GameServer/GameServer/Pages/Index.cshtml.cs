using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR.Client;
using System.Diagnostics;
using System.Security.Cryptography;

namespace GameServer.Pages
{
    public class IndexModel : PageModel
    {

        public string SessionCodeCreate { get; private set; } = "";

        public void OnGet()
        {
            SessionCodeCreate = GenerateSessionCode();
        }

        private static string GenerateSessionCode()
        {
            const string chars =
                "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
            const int length = 8;

            return new string(Enumerable.Range(0, length)
                .Select(_ => chars[
                    RandomNumberGenerator.GetInt32(chars.Length)])
                .ToArray());
        }
        

    }
}
