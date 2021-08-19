using Microsoft.Azure.Cosmos;
using RequestResponseModification.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace RequestResponseModification.Middleware
{
    public class CryptographyService
    {
        private RSAParameters _publicKeyClient;
        private RSAParameters _privateKeyServer;
        private readonly RSACryptoServiceProvider _rsa = null;
        private readonly Database _dbClient = null;


        public CryptographyService(Database dbclient)
        {
            _dbClient = dbclient;
            _rsa = new RSACryptoServiceProvider(2048);
            _rsa.PersistKeyInCsp = false; // for not storing the keys in container

            //temporary sol for storing keys
            _publicKeyClient = GetClientPublicKeyFromCosmos().GetAwaiter().GetResult();
            _privateKeyServer = GetServerPrivateKeyFromCosmos().GetAwaiter().GetResult();
        }

        public string Encrypt(string data)
        {
            //string[] splitted = data.Split('"');   //because "\"HFJHFE"
            _rsa.ImportParameters(_publicKeyClient);
            var byteData = Encoding.UTF8.GetBytes(data);
            var encryptedMessage = _rsa.Encrypt(byteData, false);
            return Convert.ToBase64String(encryptedMessage);
        }

        public string Decrypt(string input)
        {
            string[] splitted = input.Split('"');   //because "\"HFJHFE"
            _rsa.ImportParameters(_privateKeyServer);
            var byteData = Convert.FromBase64String(splitted[1]);
            var plainMessage = _rsa.Decrypt(byteData, false);
            return Encoding.UTF8.GetString(plainMessage);
        }

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
    }
}
