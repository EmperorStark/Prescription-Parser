using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace prescription_parser_service.Controllers
{
    [Route("api/parse")]
    [ApiController]
    public class SigController : ControllerBase
    {
        // GET: api/<SigController>
        [HttpGet]
        public string Get(string sigText)
        {
            Console.WriteLine("here");
            Console.WriteLine(sigText);
            return sigText;
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
