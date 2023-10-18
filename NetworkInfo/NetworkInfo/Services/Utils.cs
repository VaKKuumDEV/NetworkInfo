using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Sockets;
using System.Net;
using System.Threading.Tasks;
using System.Web;

namespace NetworkInfo.Services
{
    public static class Utils
    {
        public const string API_URL = "https://test.gimly.su/api.php";

        public static async Task<string> PostRequest(string url)
        {
            return await PostRequest(url, new Dictionary<string, object>());
        }

        public static async Task<string> PostRequest(string url, Dictionary<string, object> pars)
        {
            string formDataBoundary = string.Format("----{0:N}", Guid.NewGuid());
            MultipartFormDataContent content = new MultipartFormDataContent(formDataBoundary);
            foreach (var kv in pars)
            {
                if (kv.Value.GetType() == typeof(FileInfo))
                {
                    FileInfo file = (FileInfo)kv.Value;
                    byte[] fileData = File.ReadAllBytes(file.FullName);

                    ByteArrayContent param = new ByteArrayContent(fileData);
                    content.Add(param, kv.Key, file.Name);
                }
                else if (kv.Value.GetType() == typeof(FileInfo[]))
                {
                    FileInfo[] files = (FileInfo[])kv.Value;
                    foreach (FileInfo file in files)
                    {
                        byte[] fileData = File.ReadAllBytes(file.FullName);

                        ByteArrayContent param = new ByteArrayContent(fileData);
                        content.Add(param, kv.Key, file.Name);
                    }
                }
                else
                {
                    StringContent param = new StringContent(kv.Value.ToString() ?? "");
                    content.Add(param, kv.Key);
                }
            }

            HttpClientHandler handler = new HttpClientHandler()
            {
                ClientCertificateOptions = ClientCertificateOption.Automatic,
                SslProtocols = System.Security.Authentication.SslProtocols.Tls12 | System.Security.Authentication.SslProtocols.Ssl3,
            };
            handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };

            HttpClient client = new HttpClient(handler)
            {
                Timeout = TimeSpan.FromSeconds(360)
            };

            string answer;

            try
            {
                HttpResponseMessage response = await client.PostAsync(url, content);
                answer = await response.Content.ReadAsStringAsync();
            }
            catch (SocketException socEx)
            {
                answer = socEx.Message;
            }
            return answer;
        }

        public static async Task<string> GetRequest(string url, Dictionary<string, string> urlParams, IWebProxy proxy = null)
        {
            List<string> stringParams = new List<string>();
            foreach (var kv in urlParams) stringParams.Add(HttpUtility.UrlEncode(kv.Key) + "=" + HttpUtility.UrlEncode(kv.Value));
            string parametrizedUrl = url;
            if (urlParams.Count > 0) parametrizedUrl += "?" + string.Join("&", stringParams);

            HttpClientHandler handler = new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                SslProtocols = System.Security.Authentication.SslProtocols.Tls12 | System.Security.Authentication.SslProtocols.Ssl3,
            };
            handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };

            if (proxy != null)
            {
                handler.Proxy = proxy;
                handler.UseProxy = true;
            }

            HttpClient client = new HttpClient(handler)
            {
                BaseAddress = new Uri(parametrizedUrl),
            };
            HttpResponseMessage response = await client.GetAsync(String.Empty);
            response.EnsureSuccessStatusCode();
            string result = await response.Content.ReadAsStringAsync();

            return result;
        }

        public static async Task<string> GetRequest(string url)
        {
            return await GetRequest(url, new Dictionary<string, string>());
        }

        public static async Task<JObject> ApiGet(Dictionary<string, string> pars)
        {
            JObject answer;
            try
            {
                string answerStr = await GetRequest(API_URL, pars);
                try
                {
                    answer = JObject.Parse(answerStr);
                    if (answer == null)
                    {
                        throw new ApiException("Пустой ответ", answerStr);
                    }

                    if (!answer.ContainsKey("code") || answer.Value<string>("code") == "0")
                    {
                        throw new ApiException(answer.Value<string>("message") ?? "", answerStr);
                    }

                    return answer;
                }
                catch (Exception parseEx)
                {
                    throw new ParseException(parseEx.Message, answerStr);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static async Task<JObject> ApiPost(Dictionary<string, object> pars)
        {
            JObject answer;
            try
            {
                string answerStr = await PostRequest(API_URL, pars);
                try
                {
                    answer = JObject.Parse(answerStr);
                    if (answer == null)
                    {
                        throw new ApiException("Пустой ответ", answerStr);
                    }

                    if (!answer.ContainsKey("code") || answer.Value<string>("code") == "0")
                    {
                        throw new ApiException(answer.Value<string>("message") ?? "", answerStr);
                    }

                    return answer;
                }
                catch (Exception parseEx)
                {
                    throw new ParseException(parseEx.Message, answerStr);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
    }

    public class ApiException : Exception
    {
        public string ServerAnswer { get; }

        public ApiException(string message, string serverAnswer) : base(message)
        {
            ServerAnswer = serverAnswer;
        }
    }

    public class ParseException : Exception
    {
        public string ServerAnswer { get; }

        public ParseException(string message, string serverAnswer) : base(message)
        {
            ServerAnswer = serverAnswer;
        }
    }
}
