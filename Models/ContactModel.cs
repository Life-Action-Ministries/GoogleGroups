using Newtonsoft.Json;

namespace GoogleGroups.Models
{
    public class ContactModel
    {
        public string? Email { get; set; }
        public List<string>? SecondaryEmails { get; set; }
        public string? Name { get; set; }
        public string? Phone { get; set; }
        public string? PhotoUrl { get; set; } = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAOEAAADhCAMAAAAJbSJIAAAAPFBMVEXk5ueutLepsLPo6uursbXJzc/p6+zj5ea2u76orrKvtbi0ubzZ3N3O0dPAxcfg4uPMz9HU19i8wcPDx8qKXtGiAAAFTElEQVR4nO2d3XqzIAyAhUD916L3f6+f1m7tVvtNINFg8x5tZ32fQAIoMcsEQRAEQRAEQRAEQRAEQRAEQRAEQRAEQRAEQRAEQTghAJD1jWtnXJPP/54IgNzZQulSmxvTH6oYXX4WS+ivhTbqBa1r26cvCdCu6i0YXbdZ0o4A1rzV+5IcE3YE+z58T45lqo7g1Aa/JY5tgoqQF3qb382x7lNzBLcxft+O17QUYfQI4IIeklKsPSN4i6LKj/7Zm8n99RbHJpEw9gEBXNBpKIYLJqKYRwjOikf//r+J8ZsVuacbqCMNleI9TqGLGqMzhnVdBOdd6F/RlrFijiCoVMk320CBIahUxTWI0KKEcJqKbMdpdJb5QvdHq6wCI5qhKlgGMS/RBHkubWDAE+QZxB4xhCyDiDkLZxgGEVdQldzSKbTIhmZkFkSEPcVvmBn2SMuZB9od7fQDsMiDdKJjFUSCQarM5WirZ3C2TT/htYnyPcPfgrFHWz0BI74gr6J/IZiGUxAZGQLqmvQLTrtE/Go4YxhVRIpEw+sww1IIcqr5NKmUUzLF3d4/qPkYIp2T/obPuemlojFUR4t9Q2Vojhb7BmgElWHzLPH8hucfpefPNFTVgs9h1AdU/Pin96vwWbWdf+X9Absn3OdO34aMdsDnP8WgKYisTqI6CkNGqZQo1XA6Ef6AU32SJzOcBukHPF07/xNSgmHKa5BOhtezv6mA/rYJpwXNAnbRZ1XuF3BzDcO3vpA3+ny2909gbqE4hhD3LIPhLLyBNhPZvbZ3B+3tPYa18A7auSlXQayKwTPNLKDcuOB0xPYKDPFTkWsevQPRZ1J8Hji9I1KQ34r7hZhrwNwOZ97QxNx0drwn4QI0wQk1DcEsfKCWKdxVvxPSNUIp/knmAXT+nT+Ko3+0H96rcNb3m1fx7MBTJdeBJ7uFcWsc0wvgAsC4pROW0l2inbAmIBv/7GZmuhQH6API2rr8T0e6yuZJ+80A9LZeG62T3tik31XwxtwZcizKuTHkMjB1WdZde4Kmic/A5ZI3rr1ae21d08PlVHYfAaxw9G9CYRbJ+8ZdbTcMRV1XM3VdF0M32vtoTdZ0+u29s0OttJ5bz64UwinjaFMVY9vkqc3KKSxN21Xl+0L4Q3Vuv1tYl0pqnX6ms4XetFz7gdZVAgUEoJntfOUe4ZwsHd9FzqQ3Vv6xe41l0XJcqcKl6TZvlv7ClAW3BsqQW4X7ypApB8dmTgK4IX5wvqIVj33HtD2qSG4BqznxdIefL27Y4sahi0MdIdvUsDva8agGGbCtITmCY31MHD2O0uIdh/0rJDQ1VX5Zdxz3rR2QDbv6qXl9vudzqQtGm1Jv9LDXOsfvvB7VcZ8PDKD0mQ1VHPYQ9O+Yj4hR1IUD8rBnn3ho2m8oQMxbCFiKlL2ioSW5heeJqegED52CzxCtcGD3Kv8Wms9EYLyUhwaFIhSMBClevWEmiK/Iaogu4H7sg6ppQhQG8RUqivuTGOAJOg6FfgW0q0M0PQMRMEgXaeNf3SYDZ8PIMI0+wHgr/MgN7wYwpiLjCCqM6ydUDZLQiB6nDdNC8SDyig3jPPpFXGcC9O8BUBDVmgBY59E7Md/35Loe/UVEECEJwYggJjELZ4J71SaQSBeC02n4Da29CayJNA28SAhd2CQyC1Xw6pSmGSINQVuMhAZp4DClan9MgmkDDNmezqwS8sgtlXK/EPBhoaSmYVC/F7IO1jQEdHOlabpKh3+jzLQSTUiq4X2I+Ip/zU8rlaqAvkS21ElR+gqu3zbjjL+hIAiCIAiCIAiCIAiCsCf/AKrfVhSbvA+DAAAAAElFTkSuQmCC";
        public List<GroupModel>? Groups { get; set; }
    }
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class Author
    {
        public Email email { get; set; }
        public Name name { get; set; }
    }

    public class Category
    {
        public string scheme { get; set; }
        public string term { get; set; }
    }

    public class Content
    {
        [JsonProperty("$t")]
        public string t { get; set; }
        public string type { get; set; }
    }

    public class Email
    {
        [JsonProperty("$t")]
        public string t { get; set; }
    }

    public class Entry
    {
        public Id id { get; set; }
        public Updated updated { get; set; }
        public List<Category> category { get; set; }
        public Title title { get; set; }
        public List<Link> link { get; set; }
        public Content content { get; set; }

        [JsonProperty("gd$email")]
        public List<GdEmail> gdemail { get; set; }

        [JsonProperty("gd$phoneNumber")]
        public List<GdPhoneNumber> gdphoneNumber { get; set; }
    }

    public class Feed
    {
        public string xmlns { get; set; }

        [JsonProperty("xmlns$openSearch")]
        public string xmlnsopenSearch { get; set; }

        [JsonProperty("xmlns$batch")]
        public string xmlnsbatch { get; set; }

        [JsonProperty("xmlns$gd")]
        public string xmlnsgd { get; set; }

        [JsonProperty("xmlns$gContact")]
        public string xmlnsgContact { get; set; }
        public Id id { get; set; }
        public Updated updated { get; set; }
        public List<Category> category { get; set; }
        public Title title { get; set; }
        public List<Link> link { get; set; }
        public List<Author> author { get; set; }
        public Generator generator { get; set; }

        [JsonProperty("openSearch$totalResults")]
        public OpenSearchTotalResults openSearchtotalResults { get; set; }

        [JsonProperty("openSearch$startIndex")]
        public OpenSearchStartIndex openSearchstartIndex { get; set; }

        [JsonProperty("openSearch$itemsPerPage")]
        public OpenSearchItemsPerPage openSearchitemsPerPage { get; set; }
        public List<Entry> entry { get; set; }
    }

    public class GdEmail
    {
        public string address { get; set; }
        public string primary { get; set; }
        public string rel { get; set; }
        public string displayName { get; set; }
    }

    public class GdPhoneNumber
    {
        public string rel { get; set; }
        public string primary { get; set; }

        [JsonProperty("$t")]
        public string t { get; set; }
    }

    public class Generator
    {
        [JsonProperty("$t")]
        public string t { get; set; }
        public string uri { get; set; }
        public string version { get; set; }
    }

    public class Id
    {
        [JsonProperty("$t")]
        public string t { get; set; }
    }

    public class Link
    {
        public string rel { get; set; }
        public string type { get; set; }
        public string href { get; set; }
    }

    public class Name
    {
        [JsonProperty("$t")]
        public string t { get; set; }
    }

    public class OpenSearchItemsPerPage
    {
        [JsonProperty("$t")]
        public string t { get; set; }
    }

    public class OpenSearchStartIndex
    {
        [JsonProperty("$t")]
        public string t { get; set; }
    }

    public class OpenSearchTotalResults
    {
        [JsonProperty("$t")]
        public string t { get; set; }
    }

    public class Root
    {
        public string version { get; set; }
        public string encoding { get; set; }
        public Feed feed { get; set; }
    }

    public class Title
    {
        [JsonProperty("$t")]
        public string t { get; set; }
        public string type { get; set; }
    }

    public class Updated
    {
        [JsonProperty("$t")]
        public DateTime t { get; set; }
    }


}
