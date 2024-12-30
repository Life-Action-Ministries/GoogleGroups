using Google.Apis.Admin.Directory.directory_v1.Data;
using GoogleGroups.App_Code;
using Microsoft.AspNetCore.Mvc;

namespace GoogleGroups.Controllers
{
    public class SetupController : Controller
    {
        public IActionResult Index()
        {
            if (System.IO.File.Exists("./wwwroot/Data/credentials.p12") && Helper.CredVal("ClientID") != "")
            {
                return Redirect("/Home");
            }
            return View();
        }

        [HttpPost]
        public IActionResult UploadLogo(IFormFile file)
        {
            List<string> extensions = new List<string>() { ".jpg", ".png", ".gif" };
            UploadHandler.Upload(file, extensions, "logo", "wwwroot/Data/Icons");
            return Redirect("/Home");
        }

        [HttpPost]
        public IActionResult UploadFavicon(IFormFile favicon)
        {
            List<string> extensions = new List<string>() { ".png" };
            UploadHandler.Upload(favicon, extensions, "favicon", "wwwroot/Data/Icons");
            return Redirect("/Home");
        }

        [HttpPost]
        public IActionResult Index(string orgName, string domain, string superAdmin, string clientId, string clientSecret, string serviceAccount, IFormFile file, string credPass, string username)
        {
            Helper.UpdateSettings("OrganizationName", orgName);
            Helper.UpdateSettings("Domain", domain);
            Helper.UpdateSettings("AdminEmail", superAdmin);
            Helper.UpdateSettings("ClientID", clientId);
            Helper.UpdateSettings("ClientSecret", clientSecret);
            Helper.UpdateSettings("ServiceAccountClientID", serviceAccount);
            Helper.UpdateSettings("CredPass", credPass);
            SQL.AddSuperAdmin(username);

            List<string> extensions = new List<string>() { ".p12" };
            UploadHandler.Upload(file, extensions, "credentials", "wwwroot/Data");

            return View();
        }
    }
}
