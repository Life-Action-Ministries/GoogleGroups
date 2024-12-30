using Google.Apis.Auth.AspNetCore3;
using Google.Apis.PeopleService.v1;
using Google.Apis.Services;
using GoogleGroups.App_Code;
using GoogleGroups.Extensions;
using GoogleGroups.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;


namespace GoogleGroups.Controllers
{
    [GoogleScopedAuthorize(PeopleServiceService.ScopeConstants.UserinfoProfile)]
    //[Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public async Task<IActionResult> Index([FromServices] IGoogleAuthProvider auth)
        {
            if (!System.IO.File.Exists("./wwwroot/Data/credentials.p12"))
            {
                return Redirect("/Setup");
            }
            var cred = await auth.GetCredentialAsync();
            var service = new PeopleServiceService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = cred
            });

            var request = service.People.Get("people/me");
            request.PersonFields = "names,emailAddresses";
            var person = await request.ExecuteAsync();

            string name = person.Names.FirstOrDefault()?.DisplayName;
            string email = person.EmailAddresses.FirstOrDefault()?.Value;
            string username = email.Split("@")[0];
            int contactAccess = MyAuthorizeAttribute.GetUserPermission(username, "Contacts");

            UserModel user = new UserModel
            {
                Name = name,
                Email = email,
                UserName = username,
                ContactAccess = contactAccess,
            };
            HttpContext.Session.SetObject("User", user);


            return View(person);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public async Task<IActionResult> UserProfile([FromServices] IGoogleAuthProvider auth)
        {
            var cred = await auth.GetCredentialAsync();
            var service = new PeopleServiceService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = cred
            });

            var request = service.People.Get("people/me");
            request.PersonFields = "names,emailAddresses";
            var person = await request.ExecuteAsync();
            return View(person);

        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Unauthorized()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
