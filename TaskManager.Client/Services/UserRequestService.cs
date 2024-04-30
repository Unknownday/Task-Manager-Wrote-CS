using DryIoc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TaskManager.Client.Models;

namespace TaskManager.Client.Services
{
    public class UserRequestService
    {

        private static T ParseResponse<T>(HttpWebRequest req)
        {
            HttpWebResponse response = (HttpWebResponse)req.GetResponse();
            using (StreamReader reader = new StreamReader(response.GetResponseStream()))
            {
                string responseStr = reader.ReadToEnd();
                T parsedItem = JsonConvert.DeserializeObject<T>(responseStr);
                return parsedItem;

            }
        }

        private static HttpWebRequest GenerateRequest(string host, string json, string method)
        {
            string url = "https://localhost:7182/api/" + host;
            HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(url);

            req.Method = method;
            req.ContentType = "application/json";

            using (var streamWriter = new StreamWriter(req.GetRequestStream()))
            {
                streamWriter.Write(json);
                streamWriter.Flush();
                streamWriter.Close();
            }

            return req;
        }

        public AuthToken GetToken(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password)) return null;

            string jsonBody = "{\"email\":\"@UserEmail\",\"password\":\"@UserPassword\"}";

            jsonBody.Replace("@UserEmail", email);
            jsonBody.Replace("@UserPassword", password);

            return ParseResponse<AuthToken>(GenerateRequest("Account/token", jsonBody, "POST"));
        }
    }
}
