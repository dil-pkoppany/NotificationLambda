using System.Text.Json.Serialization;

namespace NotificationTablePopulationLambda.Data;

/// <summary>
/// Class for retrieving Amazon RDS Microsoft SQLServer secret. For more information refer 
/// <see href="https://docs.aws.amazon.com/secretsmanager/latest/userguide/reference_secret_json_structure.html#reference_secret_json_structure_RDS_sqlserver">JSON structure of AWS Secrets Manager secrets</see>
/// </summary>
public class RdsDbSecret
{
    private string? _dbName;

    public static string Engine => "sqlserver";

    [JsonPropertyName("host")]
    public string Host { get; set; }

    [JsonPropertyName("username")]
    public string UserName { get; set; }

    [JsonPropertyName("password")]
    public string Password { get; set; }

    [JsonPropertyName("dbname")]
    public string DbName
    {
        get => string.IsNullOrEmpty(_dbName) ? "master" : _dbName;
        set => _dbName = value;
    }

    [JsonPropertyName("port")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public int? Port { get; set; }
}
