using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Owin;
using System.Data.Entity;
using AltLanDS.Beeline.DpcProxy.Client;

[assembly: OwinStartup(typeof(OAuthProvidersWithoutIdentity.Startup))]

namespace OAuthProvidersWithoutIdentity
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            Database.SetInitializer<DpcProxyDbContext>(null);
        }
    }
}   