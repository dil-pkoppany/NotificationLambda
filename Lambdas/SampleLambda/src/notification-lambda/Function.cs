using Amazon.Lambda.Core;
using Newtonsoft.Json.Linq;
using Amazon.Lambda.Serialization.Json;
using Amazon.SecretsManager.Extensions.Caching;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace notification_lambda;

public class Function
{    
    private SecretsManagerCache cache = new SecretsManagerCache();

    public async Task<string> FunctionHandler(JObject input, ILambdaContext context)
    {
        string secret = await cache.GetSecretString(Environment.GetEnvironmentVariable("DbSecretObjectKey"));
        return input.ToObject<TestInput>().key1.ToUpper() + " " + secret;
    }
}

public class TestInput
{
    public string key1;
}
