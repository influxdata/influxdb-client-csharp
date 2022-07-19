using InfluxDB.Client;
namespace ExampleBlazor.Data;

public static class ClientSettings
{
    public static string Url = "http://localhost:8086";
    public static string Username = "my-user";
    public static string Password = "my-password";
    public static string Token = "my-token";
    public static string Org = "my-org";
    
    public static InfluxDBClient GetClient()
    {
        return InfluxDBClientFactory.Create(Url, Username, Password.ToCharArray());
    }
}