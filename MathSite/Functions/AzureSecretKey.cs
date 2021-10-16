using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;

namespace MathSite.Functions
{
    public class AzureSecretKey
    {
        public string TakeSecretKey(string Name, string Code)
        {
            var KeyVaultUrl = $"https://mathsitevault.vault.azure.net/secrets/" + Name + "/" + Code;
            AzureServiceTokenProvider AzureServiceTokenProvider = new AzureServiceTokenProvider();
            KeyVaultClient KeyVaultClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(AzureServiceTokenProvider.KeyVaultTokenCallback));
            var SecretCode = KeyVaultClient.GetSecretAsync(KeyVaultUrl).Result.Value;
            return SecretCode;
        }
    }
}
