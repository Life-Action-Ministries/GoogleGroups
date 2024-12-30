using Google.Apis.Auth.AspNetCore3;
using Google.Apis.PeopleService.v1;
using GoogleGroups.App_Code;
using GoogleGroups.Extensions;
using GoogleGroups.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;

namespace GoogleGroups.Controllers
{
    [GoogleScopedAuthorize(PeopleServiceService.ScopeConstants.UserinfoProfile)]
    public class ContactsController : Controller
    {
        private readonly IDistributedCache _cache;
        public ContactsController(IDistributedCache cache)
        {
            _cache = cache;
        }
        public async Task<IActionResult> Index()
        {
            string username = HttpContext.Session.GetObject<UserModel>("User").UserName;
            if (username == null) { return Redirect("/Home"); }
            List<ContactModel> contacts = await GoogleApi.GetContacts(_cache);
            ViewData["contacts"] = contacts;
            return View();
        }
        [HttpGet]
        public async Task<IActionResult> Contact(string email)
        {
            string username = HttpContext.Session.GetObject<UserModel>("User").UserName;
            if (username == null) { return Redirect("/Home"); }
            List<ContactModel> contacts = await GoogleApi.GetContacts(_cache);
            ViewData["email"] = email;

            ContactModel? contact = contacts.Find(x => x.Email == email);
            List<GroupModel>? groups = await GoogleApi.GetContactGroups(_cache, email, HttpContext.Session.GetObject<UserModel>("User"));
            //ViewData["contact"] = contact;
            if (contact is not null)
            {
                contact.Groups = groups;
            }
            else
            {
                contact = new ContactModel()
                {
                    Email = email,
                    Groups = groups
                };
            }
            //ViewData["username"] = HttpContext.Session.GetString("Username");
            //ViewData["contactAccess"] = HttpContext.Session.GetString("ContactAccess").ToString();
            return View(contact);
        }

        [HttpGet]
        public IActionResult AddContact(string? email)
        {
            //List<GroupModel> groups = await GoogleApi.GetGroups(UserModel.UserName, _cache);
            ViewData["email"] = email;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddContact(string firstName, string lastName, string email, string phone)
        {
            string recordKey = "Contacts_" + DateTime.Now.ToString("yyyyMMdd_hh");
            await GoogleApi.CreateContact(firstName, lastName, email, phone);

            //clear cache
            await _cache.RemoveAsync(recordKey);


            return Redirect("/Contacts/Contact?email=" + email);
            //return View(Contact(email));
        }

        [HttpGet]
        public IActionResult UpdateContact(string? email, string? name, string? phone)
        {
            //List<GroupModel> groups = await GoogleApi.GetGroups(UserModel.UserName, _cache);
            ContactModel contact = new ContactModel()
            {
                Email = email,
                Phone = phone,
                Name = name
            };
            
            return View(contact);
        }
        [HttpPost]
        public async Task<IActionResult> UpdateContact(string email, string firstName, string lastName, string phone, string oldContact)
        {
            var contactString = JsonConvert.DeserializeObject<ContactModel>(oldContact);
            ContactModel newContact = new ContactModel()
            {
                Name = firstName + " " + lastName,
                Email = email,
                Phone = phone
            };

            await GoogleApi.UpdateContact(contactString, newContact);
            if(contactString.Email != email)
            {
                List<GroupModel>? groups = await GoogleApi.GetContactGroups(_cache, contactString.Email, HttpContext.Session.GetObject<UserModel>("User"));
                foreach(GroupModel group in groups)
                {
                    if(group.IsMember == true)
                    {
                        await GoogleApi.UpdateEmail(contactString.Email, email, group.Id, _cache, HttpContext.Session.GetObject<UserModel>("User"));
                    }
                }
            }
            

            string recordKey = "Contacts_" + DateTime.Now.ToString("yyyyMMdd_hh");
            await _cache.RemoveAsync(recordKey);

            return Redirect("/Contacts/Contact?email=" + email);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteContact(string email)
        {
            await GoogleApi.DeleteContact(email);
            return Redirect("/Contacts");
        }

        [Authorize]
        public IActionResult SuperAdmins()
        {
            if (!SQL.GetSuperAdmins().Contains(HttpContext.Session.GetObject<UserModel>("User").UserName))
            {
                return Redirect("/Home/Unauthorized");
            }
            return View(SQL.GetSuperAdmins());
        }

        [HttpPost]

        public IActionResult AddSuperAdmin(string username)
        {
            SQL.AddSuperAdmin(username);
            return Redirect("/Contacts/SuperAdmins");
        }

        [HttpPost]
        public IActionResult RemoveSuperAdmin(string username)
        {
            SQL.DeleteSuperAdmin(username);

            if (username == HttpContext.Session.GetString("Username"))
            {
                return Redirect("/");
            }
            else
            {
                return Redirect("/Contacts/SuperAdmins");
            }
        }

        [HttpPost]
        public IActionResult UploadLogo(IFormFile file)
        {
            List<string> extensions = new List<string>() { ".png" };
            UploadHandler.Upload(file, extensions, "logo", "wwwroot/Data/Icons");
            return Redirect("/Contacts/SuperAdmins");
        }

        [HttpPost]
        public IActionResult UploadFavicon(IFormFile favicon)
        {
            List<string> extensions = new List<string>() { ".png" };
            UploadHandler.Upload(favicon, extensions, "favicon", "wwwroot/Data/Icons");
            return Redirect("/Contacts/SuperAdmins");
        }

        [HttpPost]
        public IActionResult UploadCert(IFormFile cert)
        {
            List<string> extensions = new List<string>() { ".pfx" };
            UploadHandler.Upload(cert, extensions, "localhost", "wwwroot/Data");
            return Redirect("/Contacts/SuperAdmins");
        }

        [HttpPost]
        public IActionResult SuperAdmins(string orgName, string domain, string superAdmin, string clientId, string clientSecret, string serviceAccount, IFormFile file, string credPass)
        {
            Helper.UpdateSettings("OrganizationName", orgName);
            Helper.UpdateSettings("Domain", domain);
            Helper.UpdateSettings("AdminEmail", superAdmin);
            Helper.UpdateSettings("ClientID", clientId);
            Helper.UpdateSettings("ClientSecret", clientSecret);
            Helper.UpdateSettings("ServiceAccountClientID", serviceAccount);
            Helper.UpdateSettings("CredPass", credPass);

            List<string> extensions = new List<string>() { ".p12" };
            UploadHandler.Upload(file, extensions, "credentials", "wwwroot/Data");

            return View();
        }
    }
}
