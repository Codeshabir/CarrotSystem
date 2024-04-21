using Microsoft.AspNetCore.Mvc;
using Xero.NetStandard.OAuth2.Client;
using Xero.NetStandard.OAuth2.Config;
using Xero.NetStandard.OAuth2.Token;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authorization;
using CarrotSystem.Services;
using System.Text.Json;

namespace CarrotSystem.Controllers
{
    public class AuthorizationController : BaseXeroOAuth2Controller
    {
        private const string StateFilePath = "./Data/Xero/state.json";

        private readonly XeroClient _client;

        public AuthorizationController(IOptions<XeroConfiguration> xeroConfig) : base(xeroConfig)
        {
            _client = new XeroClient(xeroConfig.Value);
        }

        public IActionResult Index()
        {
            var clientState = Guid.NewGuid().ToString();
            StoreState(clientState);

            return Redirect(_client.BuildLoginUri(clientState));
        }

        public async Task<IActionResult> Callback(string code, string state)
        {
            var clientState = GetCurrentState();
            if (state != clientState)
            {
                return Content("Cross site forgery attack detected!");
            }

            var xeroToken = (XeroOAuth2Token)await _client.RequestAccessTokenAsync(code);

            if (xeroToken.IdToken != null && !JwtUtils.validateIdToken(xeroToken.IdToken, xeroConfig.Value.ClientId))
            {
                return Content("ID token is not valid");
            }

            if (xeroToken.AccessToken != null && !JwtUtils.validateAccessToken(xeroToken.AccessToken))
            {
                return Content("Access token is not valid");
            }

            tokenIO.StoreToken(xeroToken);
            return RedirectToAction("ExportToXeroList", "Xero");
        }

        public async Task<IActionResult> Disconnect()
        {
            await _client.DeleteConnectionAsync(XeroToken, XeroToken.Tenants[0]);
            tokenIO.DestroyToken();

            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> Revoke()
        {
            await _client.RevokeAccessTokenAsync(XeroToken);
            tokenIO.DestroyToken();

            return RedirectToAction("Index", "Home");
        }

        private void StoreState(string state)
        {
            var serializedState = JsonSerializer.Serialize(new State { state = state });
            System.IO.File.WriteAllText(StateFilePath, serializedState);
        }

        private string GetCurrentState()
        {
            if (System.IO.File.Exists(StateFilePath))
            {
                var serializeState = System.IO.File.ReadAllText(StateFilePath);
                return JsonSerializer.Deserialize<State>(serializeState)?.state;
            }

            return null;
        }
    }

    internal class State
    {
        public string state { get; set; }
    }
}