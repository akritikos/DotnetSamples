namespace Kritikos.Sample.HostedServices
{
	using System;
	using System.Threading;
	using System.Threading.Tasks;

	using Kritikos.Sample.HostedServices.Abstractions;
	using Microsoft.Extensions.DependencyInjection;
	using Microsoft.Extensions.Hosting;

	using static Serilog.Log;

	public class ConsumeScopedServiceHostedService : IHostedService
	{
		public ConsumeScopedServiceHostedService(IServiceProvider services)
			=> Services = services;

		public IServiceProvider Services { get; }

		#region Implementation of IHostedService

		/// <inheritdoc />
		public Task StartAsync(CancellationToken cancellationToken)
		{
			Logger.Verbose("Consume Scoped Service Hosted Service is starting.");

			DoWork();

			return Task.CompletedTask;
		}

		/// <inheritdoc />
		public Task StopAsync(CancellationToken cancellationToken)
		{
			Logger.Verbose("Consume Scoped Service Hosted Service is stopping.");

			return Task.CompletedTask;
		}

		#endregion

		private void DoWork()
		{
			Logger.Information("Consume Scoped Service Hosted Service is working.");

			using var scope = Services.CreateScope();
			var scopedProcessingService =
				scope.ServiceProvider
					.GetRequiredService<IScopedProcessingService>();

			scopedProcessingService.DoWork();
		}
	}
}
