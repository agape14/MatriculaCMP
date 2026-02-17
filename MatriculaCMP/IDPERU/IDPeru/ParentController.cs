using IDPeru;
using IDPeru.common;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
/**
 * @author David PAXI
 */
namespace SuitecmpWeb.Controllers
{
    public class ParentController : Controller
    {
        protected String baseUrl = $"{System.Configuration.ConfigurationManager.AppSettings["UrlRedirect"]}";
        protected Random random = new Random();

        protected ReniecIdaasClient getClient()
        {
            String jsonConfig = $"{System.Configuration.ConfigurationManager.AppSettings["RutaJSONReniec"]}";
            ReniecIdaasClient oReniecClient = new ReniecIdaasClient(jsonConfig);

            oReniecClient.acr = Constants.ACR_FACE_MOBILE;
            oReniecClient.redirectUri = baseUrl;
            oReniecClient.state = RandomString(10);
            return oReniecClient;
        }

        protected String RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

            return new String(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}