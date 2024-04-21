using Xero.NetStandard.OAuth2.Token;
using Microsoft.AspNetCore.Mvc;
using Xero.NetStandard.OAuth2.Client;
using Microsoft.Extensions.Options;
using Xero.NetStandard.OAuth2.Config;
using CarrotSystem.IO;

namespace CarrotSystem.Controllers
{
    public abstract class BaseXeroOAuth2Controller : Controller
    {
        protected readonly ITokenIO tokenIO;
        protected readonly IOptions<XeroConfiguration> xeroConfig;

        protected XeroOAuth2Token XeroToken => GetXeroOAuth2Token().Result;
        protected string TenantId => tokenIO.GetTenantId();

        protected BaseXeroOAuth2Controller(IOptions<XeroConfiguration> xeroConfig)
        {
            this.xeroConfig = xeroConfig;
            tokenIO = LocalStorageTokenIO.Instance;
        }

        private async Task<XeroOAuth2Token> GetXeroOAuth2Token()
        {
            var xeroToken = tokenIO.GetToken();
            var utcTimeNow = DateTime.UtcNow;

            if (utcTimeNow > xeroToken.ExpiresAtUtc)
            {
                var client = new XeroClient(xeroConfig.Value);
                xeroToken = (XeroOAuth2Token)await client.RefreshAccessTokenAsync(xeroToken);
                tokenIO.StoreToken(xeroToken);
            }

            return xeroToken;
        }
    }
}
