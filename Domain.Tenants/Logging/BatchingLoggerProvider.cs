using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Domain.Tenants.Logging
{
	public abstract class BatchingLoggerProvider : ILoggerProvider, ISupportExternalScope
	{
		private readonly List<LogMessage> _currentBatch = new List<LogMessage>();
		private readonly TimeSpan _interval;
		private readonly int? _queueSize;
		private readonly int? _batchSize;
		private readonly IDisposable _optionsChangeToken;

		private BlockingCollection<LogMessage> _messageQueue;
		private Task _outputTask;
		private CancellationTokenSource _cancellationTokenSource;

		private bool _includeScopes;
		private IExternalScopeProvider _scopeProvider;

		internal IExternalScopeProvider ScopeProvider => _includeScopes ? _scopeProvider : null;

		private readonly IHttpContextAccessor _httpContextAccessor;

		protected BatchingLoggerProvider(IOptionsMonitor<BatchingLoggerOptions> options, IHttpContextAccessor httpContextAccessor)
		{
			// NOTE: Only IsEnabled and IncludeScopes are monitored

			var loggerOptions = options.CurrentValue;
			if (loggerOptions.BatchSize <= 0)
			{
				throw new ArgumentOutOfRangeException(nameof(loggerOptions.BatchSize), $"{nameof(loggerOptions.BatchSize)} must be a positive number.");
			}
			if (loggerOptions.FlushPeriod <= TimeSpan.Zero)
			{
				throw new ArgumentOutOfRangeException(nameof(loggerOptions.FlushPeriod), $"{nameof(loggerOptions.FlushPeriod)} must be longer than zero.");
			}

			_interval = loggerOptions.FlushPeriod;
			_batchSize = loggerOptions.BatchSize;
			_queueSize = loggerOptions.BackgroundQueueSize;

			_optionsChangeToken = options.OnChange(UpdateOptions);
			UpdateOptions(options.CurrentValue);
			_httpContextAccessor = httpContextAccessor;
		}

		public bool IsEnabled { get; private set; }

		private void UpdateOptions(BatchingLoggerOptions options)
		{
			var oldIsEnabled = IsEnabled;
			IsEnabled = options.IsEnabled;
			_includeScopes = options.IncludeScopes;

			if (oldIsEnabled != IsEnabled)
			{
				if (IsEnabled)
				{
					Start();
				}
				else
				{
					Stop();
				}
			}

		}

		protected abstract Task WriteMessagesAsync(IEnumerable<LogMessage> messages, CancellationToken token);

		private async Task ProcessLogQueue()
		{
			while (!_cancellationTokenSource.IsCancellationRequested)
			{
				var limit = _batchSize ?? int.MaxValue;

				while (limit > 0 && _messageQueue.TryTake(out var message))
				{
					_currentBatch.Add(message);
					limit--;
				}

				if (_currentBatch.Count > 0)
				{
					try
					{
						await WriteMessagesAsync(_currentBatch, _cancellationTokenSource.Token);
					}
					catch
					{
						// ignored
					}

					_currentBatch.Clear();
				}

				await IntervalAsync(_interval, _cancellationTokenSource.Token);
			}
		}

		protected virtual Task IntervalAsync(TimeSpan interval, CancellationToken cancellationToken)
		{
			return Task.Delay(interval, cancellationToken);
		}

		internal void AddMessage(DateTimeOffset timestamp, string tenant, string message)
		{
			if (!_messageQueue.IsAddingCompleted)
			{
				try
				{
					_messageQueue.Add(new LogMessage { Message = message, Timestamp = timestamp, Tenant = tenant }, _cancellationTokenSource.Token);
				}
				catch
				{
					//cancellation token canceled or CompleteAdding called
				}
			}
		}

		private void Start()
		{
			_messageQueue = _queueSize == null ?
				new BlockingCollection<LogMessage>(new ConcurrentQueue<LogMessage>()) :
				new BlockingCollection<LogMessage>(new ConcurrentQueue<LogMessage>(), _queueSize.Value);

			_cancellationTokenSource = new CancellationTokenSource();
			_outputTask = Task.Run(ProcessLogQueue);
		}

		private void Stop()
		{
			_cancellationTokenSource.Cancel();
			_messageQueue.CompleteAdding();

			try
			{
				_outputTask.Wait(_interval);
			}
			catch (TaskCanceledException)
			{
			}
			catch (AggregateException ex) when (ex.InnerExceptions.Count == 1 && ex.InnerExceptions[0] is TaskCanceledException)
			{
			}
		}

		public void Dispose()
		{
			_optionsChangeToken?.Dispose();
			if (IsEnabled)
			{
				Stop();
			}
		}

		public ILogger CreateLogger(string categoryName)
		{
			return new BatchingLogger(this, categoryName, _httpContextAccessor);
		}

		void ISupportExternalScope.SetScopeProvider(IExternalScopeProvider scopeProvider)
		{
			_scopeProvider = scopeProvider;
		}
	}
}
