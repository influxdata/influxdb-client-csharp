using InfluxDB.Client;

namespace ExampleBlazor.Data;

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

    public InfluxDBClient GetClient(double timespanSeconds = 10)
    {
        var options = new InfluxDBClientOptions.Builder()
            .Url(Url)
            .AuthenticateToken(Token)
            .TimeOut(TimeSpan.FromSeconds(timespanSeconds))
            .Build();

        return InfluxDBClientFactory.Create(options);
    }
}