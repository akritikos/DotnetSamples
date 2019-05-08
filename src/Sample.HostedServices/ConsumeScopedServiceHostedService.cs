namespace Kritikos.Sample.HostedServices
{
	using System;
	using System.Threading;
	using System.Threading.Tasks;

	using Kritikos.Sample.HostedServices.Abstractions;
	using Microsoft.Extensions.DependencyInjection;
	using Microsoft.Extensions.Hosting;
	using Microsoft.Extensions.Logging;

	public class ConsumeScopedServiceHostedService : IHostedService
	{
		public ConsumeScopedServiceHostedService(ILogger<ConsumeScopedServiceHostedService> logger, IServiceProvider services)
		{
			Logger = logger;
			Services = services;
		}

		public IServiceProvider Services { get; }

		private ILogger Logger { get; }

		#region Implementation of IHostedService

		/// <inheritdoc />
		public Task StartAsync(CancellationToken cancellationToken)
		{
			Logger.LogInformation(
				"Consume Scoped Service Hosted Service is starting.");

			DoWork();

			return Task.CompletedTask;
		}

		/// <inheritdoc />
		public Task StopAsync(CancellationToken cancellationToken)
		{
			Logger.LogInformation(
				"Consume Scoped Service Hosted Service is stopping.");

			return Task.CompletedTask;
		}

		#endregion

		private void DoWork()
		{
			Logger.LogInformation("Consume Scoped Service Hosted Service is working.");

			using var scope = Services.CreateScope();
			var scopedProcessingService =
				scope.ServiceProvider
					.GetRequiredService<IScopedProcessingService>();

			scopedProcessingService.DoWork();
		}
	}
}
