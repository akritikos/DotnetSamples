namespace Kritikos.Sample.HostedConsole
{
	using System;
	using System.Threading.Tasks;

	using Kritikos.Sample.HostedServices;
	using Kritikos.Sample.HostedServices.Abstractions;
	using Kritikos.Sample.HostedServices.Configuration;
	using Kritikos.Sample.HostedServices.Implementations;
	using Microsoft.Extensions.Configuration;
	using Microsoft.Extensions.DependencyInjection;
	using Microsoft.Extensions.Hosting;
	using Microsoft.Extensions.Options;
	using MQTTnet;
	using MQTTnet.Client.Options;
	using MQTTnet.Extensions.ManagedClient;
	using RabbitMQ.Client;
	using Serilog;
	using Serilog.Core;

	public static class Program
	{
		private static string[] _args;
		private static Logger _logger;
		private static IHostBuilder _hostBuilder;
		private static IConfigurationBuilder _configurationBuilder;
		private static IConfiguration _configuration;
		private static IConfiguration _hostConfiguration;

		private static IConfigurationBuilder ConfigurationBuilder
			=> _configurationBuilder??=new ConfigurationBuilder()
				.AddEnvironmentVariables()
				.AddCommandLine(_args)
				.AddJsonFile(
					"appsettings.Serilog.json",
					false,
					true)
				.AddJsonFile(
					"appsettings.json",
					false,
					true)
				.AddJsonFile(
					"appsettings.Local.json",
					true,
					true);

		private static IConfiguration Configuration
			=> _configuration ??= ConfigurationBuilder
				.Build();

		private static IConfiguration HostConfiguration
			=> _hostConfiguration??=ConfigurationBuilder
				.AddEnvironmentVariables()
				.AddCommandLine(_args)
				.Build();

		private static Logger Logger
			=> _logger??=new LoggerConfiguration()
				.ReadFrom.Configuration(Configuration)
				.CreateLogger();

		private static IHostBuilder HostBuilder
			=> _hostBuilder ??= new HostBuilder()
				.ConfigureHostConfiguration(configuration =>
				{
					configuration.AddConfiguration(HostConfiguration);
				})
				.ConfigureAppConfiguration((hostContext, configuration) =>
				{
					configuration.AddConfiguration(Configuration);
					configuration.AddJsonFile(
						$"appsettings.{hostContext.HostingEnvironment.EnvironmentName}.json",
						true,
						true);
				})
				.ConfigureServices((hostContext, services) =>
				{
					services.AddOptions();
					services.Configure<MqttConfiguration>(
						hostContext.Configuration
							.GetSection("MqttConfiguration"));
					services.Configure<AmqpConfiguration>(
						hostContext.Configuration
							.GetSection("AmqpConfiguration"));

					services.AddSingleton<IManagedMqttClient>((sp) =>
					{
						var protocolOptions = sp.GetRequiredService<IOptions<MqttConfiguration>>().Value;
						var clientOptions = new ManagedMqttClientOptionsBuilder()
							.WithAutoReconnectDelay(TimeSpan.FromSeconds(protocolOptions.ReconnectDelay))
							.WithClientOptions(new MqttClientOptionsBuilder()
								.WithClientId(protocolOptions.ClientId)
								.WithTcpServer(protocolOptions.BrokerServer)
								.UseTls(protocolOptions.UseTls)
								.WithCredentials(protocolOptions.Username, protocolOptions.Password)
								.Build())
							.Build();

						var client = new MqttFactory()
							.CreateManagedMqttClient();
						client.StartAsync(clientOptions);
						return client;
					});
					services.AddSingleton<IConnectionFactory>((sp) =>
					{
						var options = sp.GetRequiredService<IOptions<AmqpConfiguration>>().Value;
						var factory = new ConnectionFactory
						{
							AmqpUriSslProtocols = System.Security.Authentication.SslProtocols.Tls12,
							HostName = options.HostName,
							VirtualHost = options.VirtualHost,
							Port = options.Port,
							UserName = options.UserName,
							Password = options.Password,
							Ssl = new SslOption("amqp.kritikos.io") { Enabled = options.UseTls },
							Protocol = Protocols.DefaultProtocol,
						};
						return factory;
					});

					services.AddSingleton<IInMemoryBackgroundTaskQueue, InMemoryBackgroundTaskQueue>();
					services.AddScoped<IScopedProcessingService, ScopedProcessingService>();
					services.AddHostedService<MqttService>();
					services.AddHostedService<InMemoryQueueService>();
					services.AddHostedService<TimerService>();
					services.AddHostedService<ConsumeScopedServiceHostedService>();
					services.AddHostedService<AmqpService>();
				})
				.UseSerilog();

		private static async Task<int> Main(string[] args)
		{
			_args = args;
			Console.WriteLine("Hello World!");
			Log.Logger = Logger;

			try
			{
				Log.Information("Starting hosting service");
				Log.Debug("Initiating host");
				await HostBuilder.RunConsoleAsync();
				Log.Debug("Host terminated");
				return 0;
			}
			catch (Exception e)
			{
				Log.Fatal("Host terminated unexpectedly: {@Exception}", e);
				return 1;
			}
			finally
			{
				Log.CloseAndFlush();
			}
		}
	}
}
