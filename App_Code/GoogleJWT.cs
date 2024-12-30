using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace GoogleGroups.App_Code
{

    //This class was taken and modified from https://medium.com/falafel-software/using-google-services-in-uwp-c-apps-part-2-3901f8154c5
    public class GoogleJWT
    {
        private readonly string clientID;
        private readonly string scope;
        public GoogleJWT(string clientID, string scope)
        {
            this.clientID = clientID;
            this.scope = scope;
        }

        public async Task<string> GetAccessToken(string certificateFilePath)
        {
            // certificate            
            var certificate = new X509Certificate2(certificateFilePath, Helper.CredVal("CredPass"), X509KeyStorageFlags.Exportable);
            return await GetAccessTokenInternal(certificate);
        }

        public async Task<string> GetAccessToken(byte[] certificateBytes)
        {
            // certificate            
            var certificate = new X509Certificate2(certificateBytes, Helper.CredVal("CredPass"), X509KeyStorageFlags.Exportable);
            return await GetAccessTokenInternal(certificate);
        }

        private async Task<string> GetAccessTokenInternal(X509Certificate2 certificate)
        {
            // header            
            var header = new { typ = "JWT", alg = "RS256" };
            // claimset            
            var times = GetExpiryAndIssueDate();
            var claimset = new
            {
                iss = clientID,
                sub = Helper.EnvVars("AdminEmail"),
                scope = scope,
                aud = "https://www.googleapis.com/oauth2/v4/token",
                iat = times[0],
                exp = times[1],
            };

            // encoded header            
            var headerSerialized = JsonConvert.SerializeObject(header);
            var headerBytes = Encoding.UTF8.GetBytes(headerSerialized);
            var headerEncoded = Convert.ToBase64String(headerBytes);

            // encoded claimset            
            var claimsetSerialized = JsonConvert.SerializeObject(claimset);
            var claimsetBytes = Encoding.UTF8.GetBytes(claimsetSerialized);
            var claimsetEncoded = Convert.ToBase64String(claimsetBytes);
            // input            
            var input = headerEncoded + "." + claimsetEncoded;
            var inputBytes = Encoding.UTF8.GetBytes(input);

            // signature            
            string signatureEncoded;
            try
            {
                var signatureBytes = certificate.GetRSAPrivateKey().SignData(inputBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
                signatureEncoded = Convert.ToBase64String(signatureBytes);
            }
            catch (Exception ex)
            {
                signatureEncoded = string.Empty;
            }

            // jwt            
            var jwt = headerEncoded + "." + claimsetEncoded + "." + signatureEncoded;
            // wrap parameters in a Form object            
            var content = new FormUrlEncodedContent(new[] {
                new KeyValuePair<string, string>("assertion", jwt),
                new KeyValuePair<string, string>("grant_type", "urn:ietf:params:oauth:grant-type:jwt-bearer")
            });

            // the url to send the POST request (from the google docs)            
            var postUrl = "https://www.googleapis.com/oauth2/v4/token";
            // submit the request            
            var client = new HttpClient();
            var result = await client.PostAsync(postUrl, content);
            var str = await result.Content.ReadAsStringAsync();
            dynamic parsedResult = JsonConvert.DeserializeObject(str);
            return result == null ? string.Empty : parsedResult.access_token;
        }

        private int[] GetExpiryAndIssueDate()
        {
            var utc0 = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            var issueTime = DateTime.UtcNow;
            var iat = (int)issueTime.Subtract(utc0).TotalSeconds;
            var exp = (int)issueTime.AddMinutes(55).Subtract(utc0).TotalSeconds;
            return new[] { iat, exp };
        }

    }
}
