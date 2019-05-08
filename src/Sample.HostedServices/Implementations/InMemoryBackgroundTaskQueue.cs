namespace Kritikos.Sample.HostedServices.Implementations
{
	using System;
	using System.Collections.Concurrent;
	using System.Threading;
	using System.Threading.Tasks;

	using Microsoft.Extensions.Logging;

	public class InMemoryBackgroundTaskQueue
	{
		public InMemoryBackgroundTaskQueue(ILogger<InMemoryBackgroundTaskQueue> logger)
			=> Logger = logger;

		private ILogger Logger { get; }

		private ConcurrentQueue<Func<CancellationToken, Task>> WorkItems { get; } =
			new ConcurrentQueue<Func<CancellationToken, Task>>();

		private SemaphoreSlim Signal { get; } = new SemaphoreSlim(0);

		public void QueueBackgroundWorkItem(Func<CancellationToken, Task> workItem)
		{
			if (workItem == null)
			{
				throw new ArgumentException($"Queue can't process null {nameof(workItem)} object!");
			}

			WorkItems.Enqueue(workItem);
			Signal.Release();
		}

		public async Task<Func<CancellationToken, Task>> DequeueAsync(CancellationToken cancellation)
		{
			await Signal.WaitAsync(cancellation);
			var dequeued = WorkItems.TryDequeue(out var workItem);
			if (!dequeued)
			{
				Logger.LogWarning("Dequeuing failed for item: {workItem}", workItem);
			}

			return workItem;
		}
	}
}
