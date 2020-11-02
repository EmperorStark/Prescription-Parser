using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Web.Http;
using System.Net.Http;
using HttpPostAttribute = Microsoft.AspNetCore.Mvc.HttpPostAttribute;
using HttpGetAttribute = Microsoft.AspNetCore.Mvc.HttpGetAttribute;
using RouteAttribute = Microsoft.AspNetCore.Mvc.RouteAttribute;

namespace prescription_parser_service.Controllers
{
    [Route("api/parse")]
    [ApiController]
    public class SigController : ControllerBase
    {

        public class SigResponses
        {
            public List<SigResponse> Pair { get; set; }
        }

        public class SigResponse
        {
            public string Token { get; set; }
            public string Tag { get; set; }
        }

        private HttpClient client;
        public SigController()
        {
            client = new HttpClient();
            client.BaseAddress = new Uri("http://127.0.0.1:8001/");
        }

        // GET: api/<SigController>
        [HttpGet]
        public async Task<IActionResult> Get(string sigText)
        {
            Console.WriteLine("here");
            var jsonInput = new JObject
            {
                ["text"] = sigText
            };
            var response = await client.PostAsJsonAsync("api", jsonInput);
            //var r = await response.Content.ReadAsAsync<SigResponses>();
            var r = await response.Content.ReadAsStringAsync();
            var json = JObject.Parse(r);
            var responses = new List<SigResponse>();
            foreach(var obj in json["pairs"])
            {
                responses.Add(new SigResponse
                {
                    Token = obj["token"].ToString(),
                    Tag = obj["tag"].ToString()
                });
            }
            foreach(var res in responses)
            {
                Console.WriteLine(res.Tag);
                Console.WriteLine(res.Token);
            }
            return Ok(responses);
        }

        // POST api/<SigController>
        [HttpPost]
        public string Post(string value)
        {
            Console.WriteLine(value);
            return value;
        }
    }
}
