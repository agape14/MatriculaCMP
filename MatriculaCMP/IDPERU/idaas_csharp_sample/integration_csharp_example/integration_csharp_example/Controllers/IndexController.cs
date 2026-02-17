using idaas_sdk_csharp;
using idaas_sdk_csharp.dto;
using System;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace integration_csharp_example.Controllers
{
    public class IndexController : ParentController
    {
        public ActionResult Index()
        {
            ReniecIdaasClient oReniecClient = getClient();

            Session["state"] = oReniecClient.state;
            ViewData["url"] = oReniecClient.getLoginUrl();
            
            return View();
        }

        public async Task<ActionResult> Endpoint(String code, String error, String state)
        {
            if (error != null || code == null)
            {
                return Redirect("/");
            }

            if (!Session["state"].Equals(state))
            {
                return Redirect("/");
            }

            System.Net.ServicePointManager.Expect100Continue = false;

            ReniecIdaasClient oReniecClient = getClient();
            Console.WriteLine("respuesta client");
            Console.WriteLine(oReniecClient.ToString());
            TokenResponse tokenResponse = await oReniecClient.getTokens(code);
            Console.WriteLine("respuesta token");
            Console.WriteLine(tokenResponse.ToString());
            if (tokenResponse == null)
            {
                return Redirect("/");
            }

            User oUser = await oReniecClient.getUserInfo(tokenResponse.accessToken);

            if (oUser == null)
            {
                return Redirect("/");
            }

            Session["oUser"] = oUser;

            return Redirect("/home");
        }
    }
}