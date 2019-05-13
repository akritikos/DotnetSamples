#pragma warning disable SA1402 // File may only contain a single type
namespace Kritikos.Sample.HostedServices.Configuration
{
	using System.Reflection;
	using System.Text;

	using MQTTnet;
	using MQTTnet.Client.Options;

	public static class MqttExtensions
	{
		public static MqttClientOptionsBuilder UseTls(
			this MqttClientOptionsBuilder builder,
			bool useTls)
			=> useTls ? builder.WithTls() : builder;

		public static string DecodePayload(this MqttApplicationMessageReceivedEventArgs source)
			=> Encoding.UTF8.GetString(source.ApplicationMessage.Payload);
	}

	public class MqttConfiguration
	{
		public double ReconnectDelay { get; set; } = 5;

		public bool UseTls { get; set; } = true;

		public string ClientId { get; set; } = Assembly.GetCallingAssembly().FullName;

		public string BrokerServer { get; set; } = "localhost";

		public string Username { get; set; } = string.Empty;

		public string Password { get; set; } = string.Empty;
	}
}
#pragma warning restore SA1402 // File may only contain a single type
