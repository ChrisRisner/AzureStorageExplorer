using System;
using System.Collections;

namespace AzureStorageExplorer
{
	public static class ASELoggerKeys {

		public const string LoginSuccess = "LoginSuccess";
		public const string LoginFailure = "LoginFailure";
		public const string Logout = "Logout";
        public const string LaunchedBrowser = "LaunchedBrowser";
	}

	public enum Severity
	{
		/// <summary>
		/// Warning Severity
		/// </summary>
		Warning,
		/// <summary>
		/// Error Severity, you are not expected to call this from client side code unless you have disabled unhandled exception handling.
		/// </summary>
		Error,
		/// <summary>
		/// Critical Severity
		/// </summary>
		Critical
	}

	public interface ILogger
	{
		void TrackPage(string page, string id = null);
		void Track(string trackIdentifier);
		void Track(string trackIdentifier, string key, string value);
		void Report(Exception exception = null, Severity warningLevel = Severity.Warning);
		void Report(Exception exception, IDictionary extraData, Severity warningLevel = Severity.Warning);
		void Report(Exception exception, string key, string value, Severity warningLevel = Severity.Warning);
	}
}
