namespace Kritikos.Sample.HostedServices
{
	using System;
	using System.Threading;
	using System.Threading.Tasks;

	using Kritikos.Sample.HostedServices.Abstractions;
	using Microsoft.Extensions.Hosting;
	using Microsoft.Extensions.Logging;

	public class InMemoryQueueService : BackgroundService
	{
		#region Overrides of BackgroundService

		public InMemoryQueueService(ILogger<InMemoryQueueService> logger, IInMemoryBackgroundTaskQueue queue)
		{
			Logger = logger;
			TaskQueue = queue;
		}

		private ILogger Logger { get; }

		private IInMemoryBackgroundTaskQueue TaskQueue { get; }

		/// <inheritdoc />
		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			Logger.LogDebug("Starting queue service");

			while (!stoppingToken.IsCancellationRequested)
			{
				var workItem = await TaskQueue.DequeueAsync(stoppingToken);

				try
				{
					await workItem(stoppingToken);
					Logger.LogInformation("Sucessfully dequeued {workItem}", nameof(workItem));
				}
				catch (Exception e)
				{
					Logger.LogError("Unexpected error {error} while dequeuing {workItem}", e, workItem);
				}
			}

			Logger.LogDebug("Stopping queue service");
		}

		#endregion
	}
}
