namespace Kritikos.Sample.HostedServices
{
	using System;
	using System.Threading;
	using System.Threading.Tasks;

	using Kritikos.Sample.HostedServices.Abstractions;
	using Microsoft.Extensions.Hosting;
	using Microsoft.Extensions.Logging;

	/// <summary>
	/// Sample timer service that uses background queue.
	/// </summary>
	public class TimerService : IHostedService, IDisposable
	{
		public TimerService(ILogger<TimerService> logger, IInMemoryBackgroundTaskQueue queue)
		{
			Logger = logger;
			Queue = queue;
			Trigger = new Timer(DoWork, null, Timeout.Infinite, 0);
		}

		private Timer Trigger { get; }

		private ILogger Logger { get; }

		private IInMemoryBackgroundTaskQueue Queue { get; }

		#region Implementation of IHostedService

		/// <inheritdoc />
		public Task StartAsync(CancellationToken cancellationToken)
		{
			Logger.LogDebug("Starting timer service");

			Trigger.Change(TimeSpan.Zero, TimeSpan.FromSeconds(5));

			return Task.CompletedTask;
		}

		/// <inheritdoc />
		public Task StopAsync(CancellationToken cancellationToken)
		{
			Logger.LogDebug("Stopping timer service");

			Trigger.Change(Timeout.Infinite, 0);

			return Task.CompletedTask;
		}

		#endregion

		#region Implementation of IDisposable

		/// <inheritdoc />
		public void Dispose()
			=> Trigger.Dispose();

		#endregion

		private void DoWork(object state)
#pragma warning disable 1998 // Async is required by background queue
			=> Queue.QueueBackgroundWorkItem(async (token) => Logger.LogInformation("Ticking..."));
#pragma warning restore 1998
	}
}
