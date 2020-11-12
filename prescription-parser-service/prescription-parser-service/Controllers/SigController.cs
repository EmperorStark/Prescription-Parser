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
using prescription_parser_service.Cache;
using prescription_parser_service.TaggedResultParser;

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
        private ICacheProvider cache;
        public SigController(ICacheProvider cache)
        {
            client = new HttpClient();
            client.BaseAddress = new Uri("http://127.0.0.1:8001/");
            this.cache = cache;
        }

        // GET: api/<SigController>
        [HttpGet]
        public async Task<IActionResult> Get(string sigText, string drugName)
        {
            var jsonInput = new JObject
            {
                ["text"] = sigText
            };
            var response = await client.PostAsJsonAsync("api", jsonInput);
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
            var parsedResult = new Whole(responses);
            await this.cache.SetAsync("CurrentList", parsedResult.days);
            var ress = await cache.TryGetValueAsync<List<DrugTime>>("CurrentList");
            Console.WriteLine(ress);
            Console.WriteLine("REDIS: " + ress.result[0].time);
            return Ok(responses);
        }

        [HttpGet]
        [Route("drugTime")]
        public async Task<IActionResult> GetDrugTime(int year, int month, int day)
        {
            Console.WriteLine("Reached here. " + year + "  " + month + "  " + day);
            var drugTimes = await cache.TryGetValueAsync<List<DrugTime>>("CurrentList");
            var drugTime = new DrugTime();
            foreach (var time in drugTimes.result)
            {
                if (time.time.Date.Month == month && time.time.Date.Year == year && time.time.Date.Day == day)
                {
                    drugTime = time;
                }
            }
            if(drugTime.drug != null)
            {
                Console.WriteLine(drugTime.drug.dose);
                return Ok(drugTime);
            }
            else
            {
                Console.WriteLine("resutl is null!");
                return Ok(drugTime);
            }
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
