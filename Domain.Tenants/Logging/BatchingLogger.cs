using System;
using System.Text;
using Domain.Tenants.Multitenancy;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Domain.Tenants.Logging
{
	/// <summary>
	/// https://github.com/andrewlock/NetEscapades.Extensions.Logging
	/// </summary>
	public class BatchingLogger : ILogger
	{
		private readonly BatchingLoggerProvider _provider;
		private readonly string _category;
		private readonly IHttpContextAccessor _httpContextAccessor;

		public BatchingLogger(BatchingLoggerProvider loggerProvider, string categoryName, IHttpContextAccessor httpContextAccessor)
		{
			_provider = loggerProvider;
			_category = categoryName;
			_httpContextAccessor = httpContextAccessor;
		}

		public IDisposable BeginScope<TState>(TState state)
		{
			// NOTE: Differs from source
			return _provider.ScopeProvider?.Push(state);
		}

		public bool IsEnabled(LogLevel logLevel)
		{
			return _provider.IsEnabled;
		}

		public void Log<TState>(DateTimeOffset timestamp, LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
		{
			if (!IsEnabled(logLevel))
			{
				return;
			}

			var builder = new StringBuilder();
			builder.Append(timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff zzz"));
			builder.Append(" [");
			builder.Append(logLevel.ToString());
			builder.Append("] ");
			builder.Append(_category);

			var scopeProvider = _provider.ScopeProvider;
			if (scopeProvider != null)
			{
				scopeProvider.ForEachScope((scope, stringBuilder) =>
				{
					stringBuilder.Append(" => ").Append(scope);
				}, builder);

				builder.AppendLine(":");
			}
			else
			{
				builder.Append(": ");
			}

			builder.AppendLine(formatter(state, exception));

			if (exception != null)
			{
				builder.AppendLine(exception.ToString());
			}

			var tenant = _httpContextAccessor.HttpContext?.GetTenant()?.Name ?? string.Empty;
			_provider.AddMessage(timestamp, tenant, builder.ToString());
		}

		public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
		{
			Log(DateTimeOffset.Now, logLevel, eventId, state, exception, formatter);
		}
	}
}
