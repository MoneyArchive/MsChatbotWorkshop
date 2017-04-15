using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Routing;

namespace Bot_Application6
{
    using System.Web.SessionState;

    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);

            System.Web.HttpContext.Current.SetSessionStateBehavior(
        SessionStateBehavior.Required);
        }
    }
}
