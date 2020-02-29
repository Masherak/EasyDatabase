using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.Azure.KeyVault.KeyVaultClient;

namespace EasyDatabase.Repository.Tests.Helpers
{
    public static class KeyVaultHelper
    {
        public static async Task<string> GetSecret(string name)
        {
            var azureServiceTokenProvider = new AzureServiceTokenProvider();
            var keyVaultClient = new KeyVaultClient(new AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));
            var secret = await keyVaultClient.GetSecretAsync($"https://easy-database-kv.vault.azure.net/secrets/{name}");

            return secret.Value;
        }
    }
}
