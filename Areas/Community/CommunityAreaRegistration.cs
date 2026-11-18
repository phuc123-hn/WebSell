using System.Web.Mvc;

namespace web1.Areas.Community
{
    public class CommunityAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "Community";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                name: "Community_default",
                url: "Community/{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional },
                namespaces: new[] { "web1.Areas.Community.Controllers" }
            );
        }
    }
}
