using System.Net;

namespace POSTxns.Service
{
  public class RabbitMQOptions
  {
    public const string RabbitMQ = "RabbitMQ";

    public string Host { get; set; } = IPAddress.Loopback.ToString();
    public int StreamPort { get; set; } = 5552;
    public string Username { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public string StreamName { get; set; } = "my-reliable-pos-txns";
  }
}