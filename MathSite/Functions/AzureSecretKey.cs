using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MathSite.Functions
{
    public class AzureSecretKey
    {
        public string TakeSecretKey(string name, string code)
        {
            var KeyVaultUrl = $"https://task4vault.vault.azure.net/secrets/" + name + "/" + code;
            AzureServiceTokenProvider azureServiceTokenProvider = new AzureServiceTokenProvider();
            KeyVaultClient keyVaultClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));
            var secret = keyVaultClient.GetSecretAsync(KeyVaultUrl).Result.Value;
            return secret;
        }
    }
}
