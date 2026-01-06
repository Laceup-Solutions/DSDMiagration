using System;

namespace LaceupMigration
{

	public class ConnectionException : Exception
	{
		public ConnectionException ()
		{
		}

		public ConnectionException (string message, Exception innerException) : base(message, innerException)
		{
		}

		public ConnectionException (string message) : base(message)
		{
		}
	}

	public class AuthorizationException: Exception{
		public AuthorizationException ()
		{
		}
		
		public AuthorizationException (string message, Exception innerException) : base(message, innerException)
		{
		}
		
		public AuthorizationException (string message) : base(message)
		{
		}
	}
}
