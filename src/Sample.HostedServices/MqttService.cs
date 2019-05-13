namespace Kritikos.Sample.HostedServices
{
	using System;
	using System.Threading;
	using System.Threading.Tasks;

	using Kritikos.Sample.HostedServices.Configuration;
	using Microsoft.Extensions.Hosting;
	using MQTTnet;
	using MQTTnet.Extensions.ManagedClient;
	using Serilog;

	public class MqttService : BackgroundService
	{
		public MqttService(IManagedMqttClient client)
			=> Client = client;

		private IManagedMqttClient Client { get; }

		#region Implementation of IHostedService

		/// <inheritdoc />
		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			await Client.SubscribeAsync("samples/queue");
			Client.UseApplicationMessageReceivedHandler(ReceivedMessage);
		}

		#endregion

		private async Task Shutdown()
		{
			await Client.UnsubscribeAsync("samples/queue");
			await Client.StopAsync();

			Client.Dispose();
		}

		private static void ReceivedMessage(MqttApplicationMessageReceivedEventArgs args)
			=> Log.Information(
				"Received \"{@format}\" formatted message, with content: \"{@content}\" on topic \"{@topic}\" by client \"{@client}\"",
				args.ApplicationMessage.PayloadFormatIndicator,
				args.DecodePayload(),
				args.ApplicationMessage.Topic,
				args.ClientId);
	}
}
