using Microsoft.AspNetCore.Mvc;
using RequestResponseModification.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RequestResponseModification.Controllers
{
    [Route("api/[controller]")]
    public class HomeController : Controller
    {
        //for object it works
        [HttpPost("HelloFromClient1")]
        public IActionResult HelloFromClient([FromBody] Master message)
        {
            Console.WriteLine("\ncontroller message : ");
            //var decryptedMsg = _customCryptography.Decrypt(message);
            //Console.WriteLine(decryptedMsg);


            //var responseMsg = _customCryptography.Encrypt("Your Message Received");

            //return Ok(responseMsg);
            return Ok(message);
        }

        [HttpPost("HelloFromClient2")]
        public IActionResult HelloFromClient2([FromBody] string message)
        {
            Console.WriteLine("\ncontroller message : ");
            //var decryptedMsg = _customCryptography.Decrypt(message);
            //Console.WriteLine(decryptedMsg);

            Console.WriteLine(message);

            //var responseMsg = _customCryptography.Encrypt("Your Message Received");

            //return Ok(responseMsg);
            return Ok(message);
        }
    }
}
