using idaas_sdk_csharp;
using idaas_sdk_csharp.common;
using idaas_sdk_csharp.dto;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

/**
 * @author David PAXI
 */
namespace integration_csharp_example.Controllers
{
    public class HomeController : ParentController
    {
        private String redirectUri = "http://localhost:54142/auth-endpoint";

        public ActionResult Index()
        {
            User oUser = (User)Session["oUser"];

            return View(oUser);
        }

        public ActionResult Logout()
        {
            ReniecIdaasClient oReniecClient = getClient();
            String uriLogout = oReniecClient.getLogoutUri(baseUrl);
            Console.WriteLine(uriLogout.ToString());
            return Redirect(uriLogout);
        }
    }
}