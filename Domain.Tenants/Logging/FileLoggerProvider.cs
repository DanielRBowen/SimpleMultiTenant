using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Domain.Tenants.Logging
{
	/// <summary>
	/// An <see cref="ILoggerProvider" /> that writes logs to a file
	/// </summary>
	[ProviderAlias("File")]
	public class FileLoggerProvider : BatchingLoggerProvider
	{
		private readonly string _path;
		private readonly string _fileName;
		private readonly string _extension;
		private readonly int? _maxFileSize;
		private readonly int? _maxRetainedFiles;
		private readonly PeriodicityOptions _periodicity;
		private readonly IHttpContextAccessor _httpContextAccessor;

		/// <summary>
		/// Creates an instance of the <see cref="FileLoggerProvider" /> 
		/// </summary>
		/// <param name="options">The options object controlling the logger</param>
		public FileLoggerProvider(IOptionsMonitor<FileLoggerOptions> options, IHttpContextAccessor httpContextAccessor) : base(options, httpContextAccessor)
		{
			var loggerOptions = options.CurrentValue;
			_path = loggerOptions.LogDirectory;
			_fileName = loggerOptions.FileName;
			_extension = loggerOptions.Extension;
			_maxFileSize = loggerOptions.FileSizeLimit;
			_maxRetainedFiles = loggerOptions.RetainedFileCountLimit;
			_periodicity = loggerOptions.Periodicity;
			_httpContextAccessor = httpContextAccessor; // The http context is going to be null on start up and this will not be ran througout the lifetime of the application.
		}

		/// <inheritdoc />
		protected override async Task WriteMessagesAsync(IEnumerable<LogMessage> messages, CancellationToken cancellationToken)
		{
			Directory.CreateDirectory(_path);

			foreach (var group in messages.GroupBy(GetGrouping))
			{
				var fullName = GetFullName(group.Key);
				var fileInfo = new FileInfo(fullName);
				if (_maxFileSize > 0 && fileInfo.Exists && fileInfo.Length > _maxFileSize)
				{
					return;
				}

				using (var streamWriter = File.AppendText(fullName))
				{
					foreach (var item in group)
					{
						await streamWriter.WriteAsync(item.Message);
					}
				}
			}

			RollFiles();
		}

		private string GetFullName((int Year, int Month, int Day, int Hour, int Minute, string tenant) group)
		{
			if (string.IsNullOrWhiteSpace(group.tenant) == false)
			{
				var path = Path.Combine(_path, group.tenant.ToString());

				Directory.CreateDirectory(path);

				switch (_periodicity)
				{
					case PeriodicityOptions.Minutely:
						return Path.Combine(path, $"{_fileName}{group.tenant}-{group.Year:0000}{group.Month:00}{group.Day:00}{group.Hour:00}{group.Minute:00}.{_extension}");
					case PeriodicityOptions.Hourly:
						return Path.Combine(path, $"{_fileName}{group.tenant}-{group.Year:0000}{group.Month:00}{group.Day:00}{group.Hour:00}.{_extension}");
					case PeriodicityOptions.Daily:
						return Path.Combine(path, $"{_fileName}{group.tenant}-{group.Year:0000}{group.Month:00}{group.Day:00}.{_extension}");
					case PeriodicityOptions.Monthly:
						return Path.Combine(path, $"{_fileName}{group.tenant}-{group.Year:0000}{group.Month:00}.{_extension}");
				}
			}
			else
			{
				switch (_periodicity)
				{
					case PeriodicityOptions.Minutely:
						return Path.Combine(_path, $"{_fileName}-{group.Year:0000}{group.Month:00}{group.Day:00}{group.Hour:00}{group.Minute:00}.{_extension}");
					case PeriodicityOptions.Hourly:
						return Path.Combine(_path, $"{_fileName}-{group.Year:0000}{group.Month:00}{group.Day:00}{group.Hour:00}.{_extension}");
					case PeriodicityOptions.Daily:
						return Path.Combine(_path, $"{_fileName}-{group.Year:0000}{group.Month:00}{group.Day:00}.{_extension}");
					case PeriodicityOptions.Monthly:
						return Path.Combine(_path, $"{_fileName}-{group.Year:0000}{group.Month:00}.{_extension}");
				}
			}


			throw new InvalidDataException("Invalid periodicity");
		}

		private (int Year, int Month, int Day, int Hour, int Minute, string tenant) GetGrouping(LogMessage message)
		{
			return (message.Timestamp.Year, message.Timestamp.Month, message.Timestamp.Day, message.Timestamp.Hour, message.Timestamp.Minute, message.Tenant);
		}

		/// <summary>
		/// Deletes old log files, keeping a number of files defined by <see cref="FileLoggerOptions.RetainedFileCountLimit" />
		/// </summary>
		protected void RollFiles()
		{
			if (_maxRetainedFiles > 0)
			{
				var directories = Directory.EnumerateDirectories(_path);

				foreach (var directory in directories)
				{
					var files = new DirectoryInfo(directory)
						.GetFiles(_fileName + "*")
						.OrderByDescending(f => f.Name)
						.Skip(_maxRetainedFiles.Value);

					foreach (var item in files)
					{
						item.Delete();
					}
				}
			}
		}
	}
}
