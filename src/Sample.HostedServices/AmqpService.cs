namespace Kritikos.Sample.HostedServices
{
	using System.Text;
	using System.Threading;
	using System.Threading.Tasks;

	using Microsoft.Extensions.Hosting;
	using RabbitMQ.Client;
	using RabbitMQ.Client.Events;
	using Serilog;

	public class AmqpService : BackgroundService
	{
		public AmqpService(IConnectionFactory factory)
		{
			Connection = factory.CreateConnection();
			Channel = Connection.CreateModel();
			Channel.ExchangeDeclare("demo.exchange", ExchangeType.Topic);
			Channel.QueueDeclare("demo.queue", true, false, false, null);
			Channel.QueueBind("demo.queue", "demo.exchange", "demo.queue.*", null);
			Channel.BasicQos(0, 1, false);
		}

		private IConnection Connection { get; }

		private IModel Channel { get; }

		#region Overrides of BackgroundService

		/// <inheritdoc />
		protected override Task ExecuteAsync(CancellationToken stoppingToken)
		{
			stoppingToken.Register(Shutdown);

			var consumer = new EventingBasicConsumer(Channel);
			Channel.DefaultConsumer = consumer;
			consumer.HandleBasicConsumeOk("blah");
			consumer.Received += (ch, ea)
				=>
			{
				Log.Information(
					"Received {@content} as consumer {@consumer}",
					Encoding.UTF8.GetString(ea.Body),
					ea.ConsumerTag);
				Channel.BasicAck(ea.DeliveryTag, false);
			};

			var address = new PublicationAddress(ExchangeType.Fanout, "demo.exchange", "demo.queue.hello");

			Channel.BasicPublish(address, null, Encoding.UTF8.GetBytes("Client online!"));
			Channel.BasicConsume("demo.queue", false, consumer);

			return Task.CompletedTask;
		}

		private void Shutdown()
		{
			Log.Warning("Shutting down connection!");
			Channel.Dispose();
			Connection.Dispose();
		}

		#endregion
	}
}
