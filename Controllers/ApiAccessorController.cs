using Microsoft.Extensions.Options;
using Xero.NetStandard.OAuth2.Client;
using Xero.NetStandard.OAuth2.Config;

namespace CarrotSystem.Controllers
{
    public abstract class ApiAccessorController<T> : BaseXeroOAuth2Controller where T : IApiAccessor, new()
    {
        protected readonly T Api;

        protected ApiAccessorController(IOptions<XeroConfiguration> xeroConfig) : base(xeroConfig)
        {
            Api = new T();
        }
    }
}
