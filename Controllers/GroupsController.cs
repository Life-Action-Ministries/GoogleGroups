using Google.Apis.Auth.AspNetCore3;
using Google.Apis.PeopleService.v1;
using GoogleGroups.App_Code;
using GoogleGroups.Extensions;
using GoogleGroups.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using NLog;

namespace GoogleGroups.Controllers
{
    [GoogleScopedAuthorize(PeopleServiceService.ScopeConstants.UserinfoProfile)]
    public class GroupsController : Controller
    {
        private Logger log = LogManager.GetLogger("databaseLogger");
        private readonly IDistributedCache _cache;
        public GroupsController(IDistributedCache cache)
        {
            _cache = cache;
        }



        public async Task<IActionResult> Index()
        {
            
            string username = HttpContext.Session.GetObject<UserModel>("User").UserName;
            if (username == null) { return Redirect("/Home"); }
            List<GroupModel> groups = new List<GroupModel>();
            groups = await GoogleApi.GetGroups(username, _cache);
            log.Info(groups.Count + " groups returned");
            return View(groups);
        }
        [HttpGet]
        public async Task<IActionResult> Members(string email, string name)
        {
            string username = HttpContext.Session.GetObject<UserModel>("User").UserName;
            if (username == null) { return Redirect("/Home"); }
            List<ContactModel> contacts = new List<ContactModel>();
            List<GroupUserModel> users = new List<GroupUserModel>();
            List<string> superAdmins = new List<string>();
            superAdmins = SQL.GetSuperAdmins();
            int access = MyAuthorizeAttribute.GetUserPermission(username, email);

            contacts = await GoogleApi.GetGroupMembers(email, _cache);
            users = SQL.GetGroupUsers(email);

            GroupModel group = new GroupModel()
            {
                Id = email,
                Name = name,
                AccessLevel = access,
                Members = contacts,
                GroupUsers = users,
                SuperAdmins = superAdmins
            };
            //ViewData["output"] = contacts;
            //ViewData["email"] = email;
            return View(group);
        }


        [HttpPost]
        public async Task<IActionResult> Members(string email, string group, int submit)
        {
            string username = HttpContext.Session.GetObject<UserModel>("User").UserName;
            if (username == null) { return Redirect("/Home"); }
            try
            {
                if (submit == 0)
                {
                    await GoogleApi.AddMember(group, email, username, _cache);
                }
                else if (submit == 1)
                {
                    await GoogleApi.DeleteMember(group, email, username, _cache);
                }
            }
            catch { return Redirect("/Home/Error"); }

            return Redirect("/Groups/Members?email=" + group);
        }

        [HttpPost]
        public IActionResult AddManager(string email, string group, int accessLevel, int submit)
        {
            if (submit == 2)
            {
                SQL.AddManager(email, group, accessLevel);
            }

            return Redirect("/Groups/Members?email=" + group);
        }

        [HttpPost]
        public IActionResult UpdateManager(string groupId, int accessLevel, string userName)
        {
            if (accessLevel == 0)
            {
                SQL.DeleteManager(userName, groupId);
            }
            else
            {
                SQL.UpdateManager(userName, groupId, accessLevel);
            }

            return Redirect("/Groups/Members?email=" + groupId);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteGroup(string groupId, int submit)
        {
            if (submit == 4)
            {
                await GoogleApi.DeleteGroup(groupId, _cache);
            }
            return Redirect("/Groups");
        }

        [HttpPost]
        public async Task<IActionResult> Index(string groupId, string groupName, string description)
        {
            await GoogleApi.CreateGroup(groupId, groupName, _cache, description);
            return Redirect("/Groups");
        }

    }
}
