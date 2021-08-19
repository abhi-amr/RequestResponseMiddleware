using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RequestResponseModification.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace RequestResponseModification.Middleware
{
    public class MyMiddleware : IMiddleware
    {
        private readonly CryptographyService _cryptoService;

        public MyMiddleware(CryptographyService cryptoService)
        {
            _cryptoService = cryptoService;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            await ModifyStream(context, next);
        }

        private async Task ModifyStream(HttpContext context, RequestDelegate next)
        {
            var request = context.Request;
            var stream = request.Body;// currently holds the original stream                    
            var originalReader = new StreamReader(stream);
            var originalContent = await originalReader.ReadToEndAsync();
            var notModified = true;
            Console.WriteLine($"incoming request in middleware : {originalContent}");
            try
            {

                if (originalContent != null)
                {
                    var decryptedData = _cryptoService.Decrypt(originalContent);
                    Console.WriteLine($"outgoing request from middleware : {decryptedData}");
                    //convert string to jsontype
                    //var json = JsonConvert.SerializeObject(decryptedData); //uncomment for string
                    //modified stream
                    var requestData = Encoding.UTF8.GetBytes(decryptedData);
                    //var requestData = Encoding.UTF8.GetBytes(json);
                    stream = new MemoryStream(requestData);
                    notModified = false;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            if (notModified)
            {
                //putting original data
                var requestData = Encoding.UTF8.GetBytes(originalContent);
                stream = new MemoryStream(requestData);
            }

            request.Body = stream;

            //------------response---------------

            var reponseContent = string.Empty;

            // Store the "pre-modified" response stream.
            var existingBody = context.Response.Body;

            using (var newBody = new MemoryStream())
            {
                // We set the response body to our stream so we can read after the chain of middlewares have been called.
                context.Response.Body = newBody;

                await next(context);

                // Set the stream back to the original.
                context.Response.Body = existingBody;

                newBody.Seek(0, SeekOrigin.Begin);

                //reponseContent will be `Hello Server!...`
                //reponseContent = new StreamReader(newBody).ReadToEnd();

                //check this---------
                var contentReader = new StreamReader(newBody);
                reponseContent = await contentReader.ReadToEndAsync();

                //----------

                //Console.WriteLine($"incoming response in middleware : {newContent}");
                //string[] splitted = newContent.Split('"');
                //var encryptedData = _cryptoService.Encrypt(splitted[1]);

                var encryptedData = _cryptoService.Encrypt(reponseContent);

                Console.WriteLine($"outgoing response from in middleware : {encryptedData}");

                // Send our modified content to the response body.
                await context.Response.WriteAsync(encryptedData);

            }
        }
    
    
    }
}
