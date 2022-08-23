using InfluxDB.Client;

namespace ExampleBlazor.Data;

// public static class ClientSettings
// {
//     public static string? Url = "http://localhost:8086";
//     public static string? Username = "my-user";
//     public static string? Password = "my-password";
//     public static string? Token = "my-token";
//     public static string? Org = "my-org";
//
//     public static InfluxDBClient GetClient()
//     {
//         return InfluxDBClientFactory.Create(Url, Username, Password.ToCharArray());
//     }
// }

public class Client
{
    public string? Url;
    public string? Token;
    public string? Org;

    public Client()
    {
    }

    public Client(string? url, string? token, string? org)
    {
        Url = url;
        Token = token;
        Org = org;
    }

    public InfluxDBClient GetClient()
    {
        var options = new InfluxDBClientOptions.Builder()
            .Url(Url)
            .AuthenticateToken(Token)
            .TimeOut(TimeSpan.FromSeconds(10))
            .Build();

        return InfluxDBClientFactory.Create(options);
    }

    public InfluxDBClient GetClient(double timespanSeconds)
    {
        var options = new InfluxDBClientOptions.Builder()
            .Url(Url)
            .AuthenticateToken(Token)
            .TimeOut(TimeSpan.FromSeconds(timespanSeconds))
            .Build();

        return InfluxDBClientFactory.Create(options);
    }
}