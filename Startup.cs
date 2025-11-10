using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Travel_Safe.Startup))]
namespace Travel_Safe
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
