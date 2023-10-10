using Amazon.SecretsManager.Extensions.Caching;
using System.Text.Json;

namespace NotificationTablePopulationLambda.Data
{
    public static class DbConnectionStringProvider
    {
        public static async Task<string> Get()
        {
            var isDevelopment = Environment.GetEnvironmentVariable("IsDevelopment") == "true";

            if (isDevelopment)
            {
                return Environment.GetEnvironmentVariable("DevelopmentDbConnectionString");
            }
            else
            { 
                return await GetFromSecretsManager();
            }
        }

        public static async Task<string> GetFromSecretsManager()
        {
            var key = Environment.GetEnvironmentVariable("DbSecretObjectKey");
            var cache = new SecretsManagerCache();

            string dbSecretObjectJson = await cache.GetSecretString(key);

            RdsDbSecret dbSecretObject = JsonSerializer.Deserialize<RdsDbSecret>(dbSecretObjectJson);

            var connectionString = $"Data Source={dbSecretObject.Host};" +
                $"Initial Catalog={dbSecretObject.DbName};" +
                $"User ID={dbSecretObject.UserName};" +
                $"Password={dbSecretObject.Password};";

            return connectionString;
        }
    }
}
