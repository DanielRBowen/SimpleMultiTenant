using System;

namespace Domain.Tenants.Logging
{
	public struct LogMessage
	{
		public DateTimeOffset Timestamp { get; set; }
		public string Message { get; set; }
		public string Tenant { get; set; }
	}
}
