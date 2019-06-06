using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace S2T4Bank.Controllers
{
    [ApiController]
    public class HelperController : ControllerBase
    {
        public class LocaleFromClient
        {
            public string Locale { get; set; }
        }

        IConfiguration configuration;

        public HelperController(IConfiguration Configuration)
        {
            configuration = Configuration;
        }

        [HttpPost]
        [Route("params")]
        public async Task<JsonResult> GetSpeechServiceToken([FromBody] LocaleFromClient LocaleFromClient)
        {
            var url = $"https://{configuration.GetSection("Region").Value}.api.cognitive.microsoft.com/sts/v1.0/issueToken";

            using (var client = new HttpClient())
            {
                var request = new HttpRequestMessage()
                {
                    RequestUri = new Uri(url),
                    Method = HttpMethod.Post
                };

                request.Headers.Add("Ocp-Apim-Subscription-Key", configuration.GetSection("SpeechKey").Value);

                var response = await client.SendAsync(request);
                string responseBody = await response.Content.ReadAsStringAsync();

                // Get locale to retreive correct endpoint if for speach recognition
                var endPointId = configuration.GetSection($"SpeechEndPointId-{LocaleFromClient.Locale}");

                if (endPointId.Value is null)
                    endPointId = configuration.GetSection($"SpeechEndPointId-en-US");

                return new JsonResult(new 
                {
                    SpeechToken = responseBody,
                    EndpointId = endPointId.Value,
                    Region = configuration.GetSection("Region").Value,
                    DirectlineToken = configuration.GetSection("DirectlineToken").Value
                });
            }
        }
    }
}