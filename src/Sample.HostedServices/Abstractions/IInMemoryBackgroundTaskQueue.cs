namespace Kritikos.Sample.HostedServices.Abstractions
{
	using System;
	using System.Collections.Generic;
	using System.Text;
	using System.Threading;
	using System.Threading.Tasks;

	public interface IInMemoryBackgroundTaskQueue
	{
		void QueueBackgroundWorkItem(Func<CancellationToken, Task> workItem);

		Task<Func<CancellationToken, Task>> DequeueAsync(
			CancellationToken cancellationToken);
	}
}
