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
        private RSAParameters _publicKeyClient;
        private RSAParameters _privateKeyServer;
        private readonly RSACryptoServiceProvider _rsa = null;
        private readonly Database _dbClient = null;
        private readonly ILogger<MyMiddleware> _logger;

        public MyMiddleware(Database dbclient,
            ILogger<MyMiddleware> logger)
        {
            _dbClient = dbclient;
            _logger = logger;
            _rsa = new RSACryptoServiceProvider(2048);
            _rsa.PersistKeyInCsp = false; // for not storing the keys in container

            //temporary solution for storing public and private key...ignore
            _publicKeyClient = GetClientPublicKeyFromCosmos().GetAwaiter().GetResult();
            _privateKeyServer = GetServerPrivateKeyFromCosmos().GetAwaiter().GetResult();
        }
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            await ModifyRequest(context);
            await next(context);
            await ModifyResponse(context);
        }

        private async Task ModifyResponse(HttpContext context)
        {
            var originalResponseStream = context.Response.Body;

            using (var ms = new MemoryStream())
            {
                context.Response.Body = ms;


                ms.Position = 0;
                var responseReader = new StreamReader(ms);

                var responseContent = responseReader.ReadToEnd();
                Console.WriteLine($"PlainResponse Body: {responseContent}");

                var encryptedResponse = Encrypt(responseContent);
                var byteData = Encoding.UTF8.GetBytes(encryptedResponse);
                await ms.WriteAsync(byteData, 0, byteData.Length);

                ms.Position = 0;

                await ms.CopyToAsync(originalResponseStream);
                context.Response.Body = originalResponseStream;
            }
        }

        private async Task ModifyRequest(HttpContext context)
        {
            //context.Request.EnableBuffering();
            var request = context.Request;

            //get the request body and put it back for the downstream items to read
            var stream = request.Body; //currently holds the original stream                    
            var reader = new StreamReader(stream);
            var originalContent = await reader.ReadToEndAsync();
            var notModified = true;
            Console.WriteLine("\nmiddleware request : " + originalContent);
            try
            {
                /*------------------this part modify the request--------------------------*/
                //var dataSource = JsonConvert.DeserializeObject<Master>(originalContent);
                if (originalContent != null)
                {
                    var modifydata = new Master() { Id = "Changed in middleware", Email = "changed" };
                    var json = JsonConvert.SerializeObject(modifydata);
                    var requestData = Encoding.UTF8.GetBytes(json);
                    stream = new MemoryStream(requestData);
                    notModified = false;
                }
                /*--------------------------------------------*/



                /*------------------this part sends null to the controller--------------------------*/
                /*---------and this is required for encryption decryption to change the whole string content-------------------*/
                //var dataSource = originalContent;
                //if (originalContent != null)
                //{
                //    //replace request stream to downstream handlers
                //    var requestData = Encoding.UTF8.GetBytes("Hola Server. Modified Request");
                //    stream = new MemoryStream(requestData);
                //    notModified = false;
                //}

                /*------------------------------------------------*/


            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
            }
            if (notModified)
            {
                //put original data back for the downstream to read
                var requestData = Encoding.UTF8.GetBytes(originalContent);
                stream = new MemoryStream(requestData);
            }
            //stream.Position = 0;
            request.Body = stream;
        }

        #region Encryption/Decryption

        private string Encrypt(string data)
        {
            _rsa.ImportParameters(_publicKeyClient);
            var byteData = Encoding.UTF8.GetBytes(data);
            var encryptedMessage = _rsa.Encrypt(byteData, false);
            return Convert.ToBase64String(encryptedMessage);
        }

        private string Decrypt(string input)
        {
            _rsa.ImportParameters(_privateKeyServer);
            var byteData = Convert.FromBase64String(input);
            var plainMessage = _rsa.Decrypt(byteData, false);
            return Encoding.UTF8.GetString(plainMessage);
        }

        #endregion

        #region Cosmos

        private async Task<RSAParameters> GetClientPublicKeyFromCosmos()
        {
            var cryptoKeyContainerClient = _dbClient.GetContainer("cryptoKey");
            var resp = await cryptoKeyContainerClient.ReadItemAsync<CryptoKey>("ClientPublicKey", new PartitionKey("Client"));

            return resp.Resource.Key;
        }

        private async Task<RSAParameters> GetServerPrivateKeyFromCosmos()
        {
            var cryptoKeyContainerClient = _dbClient.GetContainer("cryptoKey");
            var resp = await cryptoKeyContainerClient.ReadItemAsync<CryptoKey>("ServerPrivateKey", new PartitionKey("Server"));

            return resp.Resource.Key;
        }

        #endregion
    }
}
