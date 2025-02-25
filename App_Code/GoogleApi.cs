using GoogleGroups.Models;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using NLog;
using static GoogleGroups.Extensions.DistributedCacheExtensions;


namespace GoogleGroups.App_Code
{
    public class GoogleApi
    {
        private static string? clientId = Helper.EnvVars("ServiceAccountClientID");
        private static string? domain = Helper.EnvVars("Domain");
        private static Logger log = LogManager.GetCurrentClassLogger();
        //private IDistributedCache _cache;

        private static async Task<string> GetToken(string scope)
        {
            GoogleJWT jwt = new GoogleJWT(clientId, scope);
            var token = await jwt.GetAccessToken("./wwwroot/Data/credentials.p12");
            return token;
        }
        private static async Task GroupMembersEmailList(string groupId, List<string> emails, string? pageToken = "")
        {
            string token = await GetToken("https://www.googleapis.com/auth/admin.directory.group");
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Get, "https://admin.googleapis.com/admin/directory/v1/groups/" + groupId + "/members?pageToken=" + pageToken);
            request.Headers.Add("Authorization", "Bearer " + token);
            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            string json = await response.Content.ReadAsStringAsync();
            dynamic? members = JsonConvert.DeserializeObject(json);

            string nextPageToken = members.nextPageToken;
            try
            {
                foreach (var member in members.members)
                {
                    emails.Add(member.email.ToString());
                }
            }
            catch (Exception ex) { log.Error(ex.Message); }
            if (nextPageToken != null)
            {
                await GroupMembersEmailList(groupId, emails, nextPageToken);
            }
        }

        public static async Task<List<ContactModel>> GetContacts(IDistributedCache cache)
        {
            string recordKey = "Contacts_" + DateTime.Now.ToString("yyyyMMdd_hh");
            List<ContactModel> contacts = new List<ContactModel>();

            contacts = await cache.GetRecordAsync<List<ContactModel>>(recordKey);

            if (contacts is null)
            {
                contacts = new List<ContactModel>();
                string token = await GetToken("http://www.google.com/m8/feeds/contacts/ https://www.googleapis.com/auth/admin.directory.user");

                var client = new HttpClient();
                var request = new HttpRequestMessage(HttpMethod.Get, "https://www.google.com/m8/feeds/contacts/" + domain + "/full?alt=json&max-results=200");
                request.Headers.Add("Authorization", "Bearer " + token);
                var content = new StringContent("", null, "text/plain");
                request.Content = content;
                var response = await client.SendAsync(request);
                response.EnsureSuccessStatusCode();
                string json = await response.Content.ReadAsStringAsync();
                Root? myDeserializedClass = JsonConvert.DeserializeObject<Root>(json);


                try{
                    foreach (var contactObject in myDeserializedClass.feed.entry)
                    {
                        List<string> emails = new List<string>();
                        string primaryEmail = "";

                        foreach (var gmail in contactObject.gdemail)
                        {
                            if (gmail.primary == "true")
                            {
                                primaryEmail = gmail.address;
                            }
                            else
                            {
                                emails.Add(gmail.address);
                            }
                        }
                        string phone = "";
                        try
                        {
                            phone = contactObject.gdphoneNumber[0].t.ToString();
                        }
                        catch (Exception ex) { log.Error(ex.Message); }

                        string? url = null;
                        string id = contactObject.id.t.Replace("http://www.google.com/m8/feeds/contacts/"+domain+"/base/", "");

                        try
                        {
                            url = await GetPhoto("https://www.google.com/m8/feeds/photos/media/"+domain+"/" + id, token);
                        }
                        catch
                        {
                            url = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAOEAAADhCAMAAAAJbSJIAAAAPFBMVEXk5ueutLepsLPo6uursbXJzc/p6+zj5ea2u76orrKvtbi0ubzZ3N3O0dPAxcfg4uPMz9HU19i8wcPDx8qKXtGiAAAFTElEQVR4nO2d3XqzIAyAhUD916L3f6+f1m7tVvtNINFg8x5tZ32fQAIoMcsEQRAEQRAEQRAEQRAEQRAEQRAEQRAEQRAEQRAEQTghAJD1jWtnXJPP/54IgNzZQulSmxvTH6oYXX4WS+ivhTbqBa1r26cvCdCu6i0YXbdZ0o4A1rzV+5IcE3YE+z58T45lqo7g1Aa/JY5tgoqQF3qb382x7lNzBLcxft+O17QUYfQI4IIeklKsPSN4i6LKj/7Zm8n99RbHJpEw9gEBXNBpKIYLJqKYRwjOikf//r+J8ZsVuacbqCMNleI9TqGLGqMzhnVdBOdd6F/RlrFijiCoVMk320CBIahUxTWI0KKEcJqKbMdpdJb5QvdHq6wCI5qhKlgGMS/RBHkubWDAE+QZxB4xhCyDiDkLZxgGEVdQldzSKbTIhmZkFkSEPcVvmBn2SMuZB9od7fQDsMiDdKJjFUSCQarM5WirZ3C2TT/htYnyPcPfgrFHWz0BI74gr6J/IZiGUxAZGQLqmvQLTrtE/Go4YxhVRIpEw+sww1IIcqr5NKmUUzLF3d4/qPkYIp2T/obPuemlojFUR4t9Q2Vojhb7BmgElWHzLPH8hucfpefPNFTVgs9h1AdU/Pin96vwWbWdf+X9Absn3OdO34aMdsDnP8WgKYisTqI6CkNGqZQo1XA6Ef6AU32SJzOcBukHPF07/xNSgmHKa5BOhtezv6mA/rYJpwXNAnbRZ1XuF3BzDcO3vpA3+ny2909gbqE4hhD3LIPhLLyBNhPZvbZ3B+3tPYa18A7auSlXQayKwTPNLKDcuOB0xPYKDPFTkWsevQPRZ1J8Hji9I1KQ34r7hZhrwNwOZ97QxNx0drwn4QI0wQk1DcEsfKCWKdxVvxPSNUIp/knmAXT+nT+Ko3+0H96rcNb3m1fx7MBTJdeBJ7uFcWsc0wvgAsC4pROW0l2inbAmIBv/7GZmuhQH6API2rr8T0e6yuZJ+80A9LZeG62T3tik31XwxtwZcizKuTHkMjB1WdZde4Kmic/A5ZI3rr1ae21d08PlVHYfAaxw9G9CYRbJ+8ZdbTcMRV1XM3VdF0M32vtoTdZ0+u29s0OttJ5bz64UwinjaFMVY9vkqc3KKSxN21Xl+0L4Q3Vuv1tYl0pqnX6ms4XetFz7gdZVAgUEoJntfOUe4ZwsHd9FzqQ3Vv6xe41l0XJcqcKl6TZvlv7ClAW3BsqQW4X7ypApB8dmTgK4IX5wvqIVj33HtD2qSG4BqznxdIefL27Y4sahi0MdIdvUsDva8agGGbCtITmCY31MHD2O0uIdh/0rJDQ1VX5Zdxz3rR2QDbv6qXl9vudzqQtGm1Jv9LDXOsfvvB7VcZ8PDKD0mQ1VHPYQ9O+Yj4hR1IUD8rBnn3ho2m8oQMxbCFiKlL2ioSW5heeJqegED52CzxCtcGD3Kv8Wms9EYLyUhwaFIhSMBClevWEmiK/Iaogu4H7sg6ppQhQG8RUqivuTGOAJOg6FfgW0q0M0PQMRMEgXaeNf3SYDZ8PIMI0+wHgr/MgN7wYwpiLjCCqM6ydUDZLQiB6nDdNC8SDyig3jPPpFXGcC9O8BUBDVmgBY59E7Md/35Loe/UVEECEJwYggJjELZ4J71SaQSBeC02n4Da29CayJNA28SAhd2CQyC1Xw6pSmGSINQVuMhAZp4DClan9MgmkDDNmezqwS8sgtlXK/EPBhoaSmYVC/F7IO1jQEdHOlabpKh3+jzLQSTUiq4X2I+Ip/zU8rlaqAvkS21ElR+gqu3zbjjL+hIAiCIAiCIAiCIAiCsCf/AKrfVhSbvA+DAAAAAElFTkSuQmCC";
                        }
                        ContactModel contact = new ContactModel()
                        {
                            Name = contactObject.title.t,
                            Email = primaryEmail,
                            SecondaryEmails = emails,
                            Phone = phone,
                            PhotoUrl = url
                        };
                        contacts.Add(contact);
                    }
                }
                catch (Exception ex) { log.Info("No Contacts found - " + ex.Message);}

                //Get domain users and add to contacts list
                List<ContactModel> domainUsers = new List<ContactModel>();
                domainUsers = await GetDomainUsers(token);

                contacts = contacts.Concat(domainUsers).ToList();

                await cache.SetRecordAsync(recordKey, contacts);
            }


            return contacts;
        }

        public static async Task<List<ContactModel>> GetGroupMembers(string groupId, IDistributedCache cache)
        {
            string recordKey = "EmailList_" + groupId + "_" + DateTime.Now.ToString("yyyyMMdd_hh");
            List<ContactModel> groupMembers = new List<ContactModel>();

            groupMembers = await cache.GetRecordAsync<List<ContactModel>>(recordKey);

            if (groupMembers is null)
            {
                groupMembers = new List<ContactModel>();
                List<string> emails = new List<string>();
                await GroupMembersEmailList(groupId, emails);
                List<ContactModel> contacts = await GetContacts(cache);



                foreach (var email in emails)
                {
                    ContactModel? contact = new ContactModel();
                    contact = contacts.Find(x => x.Email == email);
                    if (contact != null)
                    {
                        groupMembers.Add(contact);
                    }
                    else
                    {
                        groupMembers.Add(new ContactModel()
                        {
                            Email = email
                        });
                    }
                }
                await cache.SetRecordAsync(recordKey, groupMembers);
            }


            return groupMembers;
        }

        public static async Task<string?> GetPhoto(string? url, string? token = null)
        {
            if (url == null)
            {
                return "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAOEAAADhCAMAAAAJbSJIAAAAPFBMVEXk5ueutLepsLPo6uursbXJzc/p6+zj5ea2u76orrKvtbi0ubzZ3N3O0dPAxcfg4uPMz9HU19i8wcPDx8qKXtGiAAAFTElEQVR4nO2d3XqzIAyAhUD916L3f6+f1m7tVvtNINFg8x5tZ32fQAIoMcsEQRAEQRAEQRAEQRAEQRAEQRAEQRAEQRAEQRAEQTghAJD1jWtnXJPP/54IgNzZQulSmxvTH6oYXX4WS+ivhTbqBa1r26cvCdCu6i0YXbdZ0o4A1rzV+5IcE3YE+z58T45lqo7g1Aa/JY5tgoqQF3qb382x7lNzBLcxft+O17QUYfQI4IIeklKsPSN4i6LKj/7Zm8n99RbHJpEw9gEBXNBpKIYLJqKYRwjOikf//r+J8ZsVuacbqCMNleI9TqGLGqMzhnVdBOdd6F/RlrFijiCoVMk320CBIahUxTWI0KKEcJqKbMdpdJb5QvdHq6wCI5qhKlgGMS/RBHkubWDAE+QZxB4xhCyDiDkLZxgGEVdQldzSKbTIhmZkFkSEPcVvmBn2SMuZB9od7fQDsMiDdKJjFUSCQarM5WirZ3C2TT/htYnyPcPfgrFHWz0BI74gr6J/IZiGUxAZGQLqmvQLTrtE/Go4YxhVRIpEw+sww1IIcqr5NKmUUzLF3d4/qPkYIp2T/obPuemlojFUR4t9Q2Vojhb7BmgElWHzLPH8hucfpefPNFTVgs9h1AdU/Pin96vwWbWdf+X9Absn3OdO34aMdsDnP8WgKYisTqI6CkNGqZQo1XA6Ef6AU32SJzOcBukHPF07/xNSgmHKa5BOhtezv6mA/rYJpwXNAnbRZ1XuF3BzDcO3vpA3+ny2909gbqE4hhD3LIPhLLyBNhPZvbZ3B+3tPYa18A7auSlXQayKwTPNLKDcuOB0xPYKDPFTkWsevQPRZ1J8Hji9I1KQ34r7hZhrwNwOZ97QxNx0drwn4QI0wQk1DcEsfKCWKdxVvxPSNUIp/knmAXT+nT+Ko3+0H96rcNb3m1fx7MBTJdeBJ7uFcWsc0wvgAsC4pROW0l2inbAmIBv/7GZmuhQH6API2rr8T0e6yuZJ+80A9LZeG62T3tik31XwxtwZcizKuTHkMjB1WdZde4Kmic/A5ZI3rr1ae21d08PlVHYfAaxw9G9CYRbJ+8ZdbTcMRV1XM3VdF0M32vtoTdZ0+u29s0OttJ5bz64UwinjaFMVY9vkqc3KKSxN21Xl+0L4Q3Vuv1tYl0pqnX6ms4XetFz7gdZVAgUEoJntfOUe4ZwsHd9FzqQ3Vv6xe41l0XJcqcKl6TZvlv7ClAW3BsqQW4X7ypApB8dmTgK4IX5wvqIVj33HtD2qSG4BqznxdIefL27Y4sahi0MdIdvUsDva8agGGbCtITmCY31MHD2O0uIdh/0rJDQ1VX5Zdxz3rR2QDbv6qXl9vudzqQtGm1Jv9LDXOsfvvB7VcZ8PDKD0mQ1VHPYQ9O+Yj4hR1IUD8rBnn3ho2m8oQMxbCFiKlL2ioSW5heeJqegED52CzxCtcGD3Kv8Wms9EYLyUhwaFIhSMBClevWEmiK/Iaogu4H7sg6ppQhQG8RUqivuTGOAJOg6FfgW0q0M0PQMRMEgXaeNf3SYDZ8PIMI0+wHgr/MgN7wYwpiLjCCqM6ydUDZLQiB6nDdNC8SDyig3jPPpFXGcC9O8BUBDVmgBY59E7Md/35Loe/UVEECEJwYggJjELZ4J71SaQSBeC02n4Da29CayJNA28SAhd2CQyC1Xw6pSmGSINQVuMhAZp4DClan9MgmkDDNmezqwS8sgtlXK/EPBhoaSmYVC/F7IO1jQEdHOlabpKh3+jzLQSTUiq4X2I+Ip/zU8rlaqAvkS21ElR+gqu3zbjjL+hIAiCIAiCIAiCIAiCsCf/AKrfVhSbvA+DAAAAAElFTkSuQmCC";
            }
            if (token == null)
            {
                token = await GetToken("http://www.google.com/m8/feeds/contacts/");
            }

            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("Authorization", "Bearer " + token);
            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var photo = await response.Content.ReadAsByteArrayAsync();

            string image = "data:image/jpg;base64," + Convert.ToBase64String(photo);
            return image;

        }

        private static async Task<List<ContactModel>> GetDomainUsers(string? token = null)
        {
            if (token == null)
            {
                token = await GetToken("http://www.google.com/m8/feeds/contacts/ https://www.googleapis.com/auth/admin.directory.user");
            }
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Get, "https://admin.googleapis.com/admin/directory/v1/users?domain=" + domain);
            request.Headers.Add("Authorization", "Bearer " + token);
            var content = new StringContent("", null, "text/plain");
            request.Content = content;
            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            string json = await response.Content.ReadAsStringAsync();
            DomainRoot? domainUsers = JsonConvert.DeserializeObject<DomainRoot>(json);


            List<ContactModel> contacts = new List<ContactModel>();
            foreach (var user in domainUsers.users)
            {

                ContactModel contact = new ContactModel()
                {
                    Name = user.name.fullName,
                    Email = user.primaryEmail,
                    PhotoUrl = user.thumbnailPhotoUrl
                };
                contacts.Add(contact);

            }
            return contacts;
        }

        public static async Task CreateContact(string firstName, string lastName, string email, string? phone = null)
        {
            string token = await GetToken("http://www.google.com/m8/feeds/contacts/ https://www.googleapis.com/auth/admin.directory.user");
            string? phoneNumber = null;
            if (phone != null)
            {
                phoneNumber =
                    $$"""
                    <gd:phoneNumber rel='http://schemas.google.com/g/2005#work'
                        primary='true'>
                        {{phone}}
                      </gd:phoneNumber>
                    """;
            }

            string requestBody =
                $$"""
                <atom:entry xmlns:atom='http://www.w3.org/2005/Atom'
                    xmlns:gd='http://schemas.google.com/g/2005' >
                  <atom:category scheme='http://schemas.google.com/g/2005#kind'
                    term = 'http://schemas.google.com/contact/2008#contact' />
                  <gd:name>
                     <gd:givenName>{{firstName}}</gd:givenName>
                     <gd:familyName>{{lastName}}</gd:familyName>
                     <gd:fullName>{{firstName + " " + lastName}}</gd:fullName>
                  </gd:name>
                  <atom:content type='text'>NOTES</atom:content>
                  <gd:email rel='http://schemas.google.com/g/2005#work'
                    primary='true'
                    address='{{email}}' displayName='{{firstName + " " + lastName}}' />
                  {{phoneNumber}}
                </atom:entry>
                """;



            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, "https://www.google.com/m8/feeds/contacts/" + domain + "/full");
            request.Headers.Add("Authorization", "Bearer " + token);
            var content = new StringContent(requestBody, null, "application/xml");
            request.Content = content;
            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            Console.WriteLine(await response.Content.ReadAsStringAsync());

        }

        public static async Task AddMember(string group, string email, string userName, IDistributedCache cache)
        {


            int permission = MyAuthorizeAttribute.GetUserPermission(userName, group);

            if (permission >= 2)
            {
                string requestBody =
                    $$"""
                {
                    "kind": "admin#directory#member",
                    "email": "{{email}}",
                    "role": "MEMBER",
                    "type": "USER",
                    "status": "ACTIVE",
                    "delivery_settings": "ALL_MAIL"
                }
                """;


                string token = await GetToken("https://www.googleapis.com/auth/admin.directory.group");
                var client = new HttpClient();
                var request = new HttpRequestMessage(HttpMethod.Post, "https://admin.googleapis.com/admin/directory/v1/groups/" + group + "/members");
                request.Headers.Add("Authorization", "Bearer " + token);
                var content = new StringContent(requestBody, null, "application/json");
                request.Content = content;
                var response = await client.SendAsync(request);
                response.EnsureSuccessStatusCode();
                Console.WriteLine(await response.Content.ReadAsStringAsync());

                string recordKey = "EmailList_" + group + "_" + DateTime.Now.ToString("yyyyMMdd_hh");
                await cache.RemoveAsync(recordKey);
            }

        }

        public static async Task DeleteMember(string group, string email, string userName, IDistributedCache cache)
        {
            int permission = MyAuthorizeAttribute.GetUserPermission(userName, group);



            if (permission >= 2)
            {
                try
                {
                    string token = await GetToken("https://www.googleapis.com/auth/admin.directory.group");
                    var client = new HttpClient();
                    var request = new HttpRequestMessage(HttpMethod.Delete, "https://admin.googleapis.com/admin/directory/v1/groups/" + group + "/members/" + email);
                    request.Headers.Add("Authorization", "Bearer " + token);
                    var response = await client.SendAsync(request);
                    response.EnsureSuccessStatusCode();
                    Console.WriteLine(await response.Content.ReadAsStringAsync());

                    string recordKey = "EmailList_" + group + "_" + DateTime.Now.ToString("yyyyMMdd_hh");
                    await cache.RemoveAsync(recordKey);
                }
                catch (Exception ex) { log.Error(ex); }
            }



        }

        public static async Task<List<GroupModel>> GetGroups(string userName, IDistributedCache cache)
        {
            string recordKey = "Groups_" + DateTime.Now.ToString("yyyyMMdd_hh");
            List<GroupModel> groups = new List<GroupModel>();

            groups = await cache.GetRecordAsync<List<GroupModel>>(recordKey);

            if (groups is null)
            {
                groups = new List<GroupModel>();
                string token = await GetToken("https://www.googleapis.com/auth/admin.directory.group");

                var client = new HttpClient();
                var request = new HttpRequestMessage(HttpMethod.Get, "https://admin.googleapis.com/admin/directory/v1/groups?domain=" + domain);
                request.Headers.Add("Authorization", "Bearer " + token);
                var response = await client.SendAsync(request);
                response.EnsureSuccessStatusCode();
                string json = await response.Content.ReadAsStringAsync();
                dynamic? output = JsonConvert.DeserializeObject(json);



                foreach (var item in output.groups)
                {
                    int permission = 0;
                    try
                    {
                        permission = MyAuthorizeAttribute.GetUserPermission(userName, (string)item.email);
                    }
                    catch (Exception ex) { log.Error(ex.Message); }
                    if (permission >= 1)
                    {
                        GroupModel group = new GroupModel()
                        {
                            Id = item.email,
                            Name = item.name,
                            MembersCount = item.directMembersCount
                        };
                        groups.Add(group);
                    }
                }
                await cache.SetRecordAsync(recordKey, groups);
            }
            return groups;
        }

        public static async Task DeleteGroup(string groupId, IDistributedCache cache)
        {
            string token = await GetToken("https://www.googleapis.com/auth/admin.directory.group");
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Delete, "https://admin.googleapis.com/admin/directory/v1/groups/" + groupId);
            request.Headers.Add("Authorization", "Bearer " + token);
            var response = await client.SendAsync(request);
            try
            {
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex) { log.Error(ex.Message); }

            string recordKey = "Groups_" + DateTime.Now.ToString("yyyyMMdd_hh");
            await cache.RemoveAsync(recordKey);

        }

        public static async Task CreateGroup(string groupId, string groupName, IDistributedCache cache, string? description = "")
        {
            string requestBody =
                $$"""
                {
                  "email": "{{groupId}}",
                  "name": "{{groupName}}",
                  "description": "{{description}}",
                  "kind": "admin#directory#group"
                }
                """;

            string token = await GetToken("https://www.googleapis.com/auth/admin.directory.group");
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, "https://admin.googleapis.com/admin/directory/v1/groups");
            request.Headers.Add("Authorization", "Bearer " + token);
            var content = new StringContent(requestBody, null, "application/json");
            request.Content = content;
            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            Console.WriteLine(await response.Content.ReadAsStringAsync());

            string recordKey = "Groups_" + DateTime.Now.ToString("yyyyMMdd_hh");
            await cache.RemoveAsync(recordKey);
        }

        public static async Task<List<GroupModel>> GetContactGroups(IDistributedCache cache, string email, UserModel user)
        {
            string recordKey = "ContactGroups_" + email + "_" + DateTime.Now.ToString("yyyyMMdd_hh");
            List<GroupModel> contactGroups = new List<GroupModel>();

            contactGroups = await cache.GetRecordAsync<List<GroupModel>>(recordKey);

            if (contactGroups is null)
            {
                contactGroups = new List<GroupModel>();
                List<GroupModel> groups = new List<GroupModel>();
                groups = await GetGroups(user.UserName, cache);

                foreach (var group in groups)
                {
                    List<ContactModel> members = new List<ContactModel>();
                    members = await GetGroupMembers(group.Id, cache);
                    bool isMember = false;

                    if (members.Exists(x => x.Email == email))
                    {
                        isMember = true;
                    }

                    GroupModel contactGroup = new GroupModel()
                    {
                        Id = group.Id,
                        Name = group.Name,
                        IsMember = isMember
                    };
                    contactGroups.Add(contactGroup);
                }
                await cache.SetRecordAsync(recordKey, contactGroups);
            }

            return contactGroups;
        }

        public static async Task DeleteContact(string email)
        {
            string token = await GetToken("https://www.googleapis.com/auth/admin.directory.group http://www.google.com/m8/feeds/contacts/");

            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Get, "https://www.google.com/m8/feeds/contacts/"+domain+"/full?alt=json");
            request.Headers.Add("Authorization", "Bearer " + token);
            var content = new StringContent("", null, "text/plain");
            request.Content = content;
            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            string json = await response.Content.ReadAsStringAsync();

            Root? myDeserializedClass = JsonConvert.DeserializeObject<Root>(json);

            Entry contact = myDeserializedClass.feed.entry.Find(x => x.gdemail[0].address == email);
            string requestBody = JsonConvert.SerializeObject(contact);
            string url = contact.link.Find(x => x.rel == "edit").href;

            client = new HttpClient();
            request = new HttpRequestMessage(HttpMethod.Delete, url);
            request.Headers.Add("Authorization", "Bearer " + token);
            content = new StringContent(requestBody, null, "application/json");
            request.Content = content;
            response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            Console.WriteLine(await response.Content.ReadAsStringAsync());


        }

        public static async Task UpdateEmail(string oldEmail, string newEmail, string groupId, IDistributedCache cache, UserModel user)
        {
            await DeleteMember(groupId, oldEmail, user.UserName, cache);
            await AddMember(groupId, newEmail, user.UserName, cache);
        }

        public static async Task UpdateContact(ContactModel oldContact, ContactModel newContact)
        {
            string token = await GetToken("https://www.googleapis.com/auth/admin.directory.group http://www.google.com/m8/feeds/contacts/");

            // No more boilerplate needed with top level statements (https://docs.microsoft.com/en-us/dotnet/core/tutorials/top-level-templates)
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Get, "https://www.google.com/m8/feeds/contacts/"+domain+"/full?alt=json");
            request.Headers.Add("Authorization", "Bearer " + token);
            var content = new StringContent("", null, "text/plain");
            request.Content = content;
            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            string json = await response.Content.ReadAsStringAsync();

            Root? myDeserializedClass = JsonConvert.DeserializeObject<Root>(json);

            Entry contact = myDeserializedClass.feed.entry.Find(x => x.gdemail[0].address == oldContact.Email);
            string requestBody = JsonConvert.SerializeObject(contact);
            string url = contact.link.Find(x => x.rel == "edit").href;

            string oldFirst = oldContact.Name.Split(" ")[0];
            string oldLast = oldContact.Name.Split(" ")[1];

            string newFirst = newContact.Name.Split(" ")[0];
            string newLast = newContact.Name.Split(" ")[1];
            Console.WriteLine(requestBody);

            requestBody = requestBody.Replace(oldContact.Email, newContact.Email);
            requestBody = requestBody.Replace(oldFirst, newFirst);
            requestBody = requestBody.Replace(oldLast, newLast);
            requestBody = requestBody.Replace(oldContact.Phone, newContact.Phone);
            requestBody = requestBody.Replace("\"content\":{\"$t\":\"NOTES\",\"type\":\"text\"},", "\"content\":{\"$t\":\"NOTES\",\"type\":\"text\"},\"gd$name\": {\"givenName\": \""+newFirst+"\",\"familyName\": \""+newLast+"\",\"fullName\": \""+newFirst+" "+newLast+"\"},");
            Console.WriteLine(requestBody);


            client = new HttpClient();
            request = new HttpRequestMessage(HttpMethod.Put, url + "?alt=json");
            request.Headers.Add("Accept", "application/atom+xml");
            request.Headers.Add("Authorization", "Bearer " + token);
            content = new StringContent(requestBody, null, "application/json");
            request.Content = content;
            response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            Console.WriteLine(await response.Content.ReadAsStringAsync());

        }
    }
}
