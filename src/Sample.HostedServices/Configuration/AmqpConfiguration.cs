namespace Kritikos.Sample.HostedServices.Configuration
{
	public class AmqpConfiguration
	{
		public bool UseTls { get; set; } = true;

		public string VirtualHost { get; set; } = "/";

		public string HostName { get; set; } = "localhost";

		public int Port { get; set; } = 5672;

		public string UserName { get; set; } = string.Empty;

		public string Password { get; set; } = string.Empty;
	}
}
