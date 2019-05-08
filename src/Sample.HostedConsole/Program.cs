namespace Kritikos.Sample.HostedConsole
{
	using System;
	using System.Threading.Tasks;

	using Kritikos.Sample.HostedServices;
	using Kritikos.Sample.HostedServices.Abstractions;
	using Kritikos.Sample.HostedServices.Implementations;
	using Microsoft.Extensions.Configuration;
	using Microsoft.Extensions.DependencyInjection;
	using Microsoft.Extensions.Hosting;
	using Serilog;
	using Serilog.Core;

	public static class Program
	{
		private static string[] _args;
		private static IConfigurationBuilder _configurationBuilder;
		private static Logger _logger;
		private static IHostBuilder _hostBuilder;

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
					true);

		private static Logger Logger
			=> _logger??=new LoggerConfiguration()
				.ReadFrom.Configuration(ConfigurationBuilder.Build())
				.CreateLogger();

		private static IHostBuilder HostBuilder
			=> _hostBuilder??=new HostBuilder()
				.ConfigureHostConfiguration(configuration =>
				{
					configuration.AddEnvironmentVariables();
					configuration.AddCommandLine(_args);
				})
				.ConfigureAppConfiguration((hostContext, configuration) =>
				{
					configuration.AddConfiguration(ConfigurationBuilder.Build());
					configuration.AddJsonFile(
						$"appsettings.{hostContext.HostingEnvironment.EnvironmentName}.json",
						true,
						true);
				})
				.ConfigureServices((hostContext, services) =>
				{
					services.AddSingleton<IInMemoryBackgroundTaskQueue, InMemoryBackgroundTaskQueue>();
					services.AddScoped<IScopedProcessingService, ScopedProcessingService>();
					services.AddHostedService<InMemoryQueueService>();
					services.AddHostedService<TimerService>();
					services.AddHostedService<ConsumeScopedServiceHostedService>();
				})
				.UseConsoleLifetime()
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
				Log.Fatal("Host terminated unexpectedly: {Exception}", e);
				return 1;
			}
			finally
			{
				Log.CloseAndFlush();
			}
		}
	}
}
