namespace Kritikos.Sample.HostedServices
{
	using System;
	using System.Threading;
	using System.Threading.Tasks;

	using Kritikos.Sample.HostedServices.Abstractions;
	using Microsoft.Extensions.Hosting;

	using static Serilog.Log;

	public class InMemoryQueueService : BackgroundService
	{
		#region Overrides of BackgroundService

		public InMemoryQueueService(IInMemoryBackgroundTaskQueue queue)
			=> TaskQueue = queue;

		private IInMemoryBackgroundTaskQueue TaskQueue { get; }

		/// <inheritdoc />
		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			Logger.Verbose("Starting up queue service");

			while (!stoppingToken.IsCancellationRequested)
			{
				var workItem = await TaskQueue.DequeueAsync(stoppingToken);

				try
				{
					await workItem(stoppingToken);
					Logger.Warning("Sucessfully dequeued {@workItem}", nameof(workItem));
				}
				catch (Exception e)
				{
					Logger.Fatal("Unexpected error {@error} while dequeuing {@workItem}", e, workItem);
				}
			}

			Logger.Verbose("Stopping queue service");
		}

		#endregion
	}
}
