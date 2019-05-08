namespace Kritikos.Sample.HostedServices.Implementations
{
	using Kritikos.Sample.HostedServices.Abstractions;
	using Microsoft.Extensions.Logging;

	public class ScopedProcessingService : IScopedProcessingService
	{
		public ScopedProcessingService(ILogger<ScopedProcessingService> logger)
			=> Logger = logger;

		private ILogger Logger { get; }

		#region Implementation of IScopedProcessingService

		/// <inheritdoc />
		public void DoWork()
			=> Logger.LogDebug("Fulfilling scope request");

		#endregion
	}
}
