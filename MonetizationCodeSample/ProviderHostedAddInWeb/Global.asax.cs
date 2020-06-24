using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Autofac;
using Autofac.Integration.Mvc;
using ProviderHostedAddInWeb.Services;
using System.Reflection;

namespace ProviderHostedAddInWeb
{
    public class MvcApplication : System.Web.HttpApplication
    {
        public static IContainer container { get; set; }
        public void Register()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<AssignedUserService>().As<IAssignedUserService>();

            builder.RegisterControllers(Assembly.GetExecutingAssembly());
            container = builder.Build();

            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));
        }
        protected void Application_Start()
        {
            Register();
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }
    }
}
