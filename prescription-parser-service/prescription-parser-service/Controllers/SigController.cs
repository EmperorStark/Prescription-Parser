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
            var cacheObject = await cache.TryGetValueAsync<Whole>("CurrentDrugDate");
            var drugTimes = cacheObject.result;
            if (cacheObject.keyExists)
            {
                drugTimes.addParse(responses);
            }
            else
            {
                drugTimes = new Whole(responses, drugName);
            }
            await cache.SetAsync("CurrentDrugDate", drugTimes);
            var ress = await cache.TryGetValueAsync<Whole>("CurrentDrugDate");
            Console.WriteLine("REDIS: " + ress.result.drugByDate[0].drugTimes[0].drug.dose);
            return Ok(responses);
        }

        [HttpGet]
        [Route("drugTime")]
        public async Task<IActionResult> GetDrugTime(int year, int month, int day)
        {
            Console.WriteLine("Reached here. " + year + "  " + month + "  " + day);
            var drugTimes = await cache.TryGetValueAsync<Whole>("CurrentDrugDate");
            var drugTime = new List<DrugTime>();
            if (drugTimes.keyExists)
            {
                foreach (var time in drugTimes.result.drugByDate)
                {
                    if (time.theDate.Date.Month == month && time.theDate.Date.Year == year && time.theDate.Date.Day == day)
                    {
                        drugTime = time.drugTimes;
                    }
                }
                if (drugTime.Count != 0)
                {
                    return Ok(drugTime);
                }
                else
                {
                    Console.WriteLine("resutl is null!");
                    return Ok(drugTime);
                }
            }
            return Ok(drugTime);
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
