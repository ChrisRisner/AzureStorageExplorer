using System;
using System.Collections;
using System.Diagnostics;
using AzureStorageExplorer;
using Xamarin.Forms;
using Microsoft.Azure.Mobile;
using Microsoft.Azure.Mobile.Analytics;

[assembly: Dependency(typeof(ASELogger))]
namespace AzureStorageExplorer
{
	public class ASELogger : ILogger
	{
		//Todo: Consider turning this on and off by a flag
		bool enableMobileCenterAnalytics = true;

		#region ILogger implementation

		public virtual void TrackPage(string page, string id = null)
		{
			Debug.WriteLine("ASE Logger: TrackPage: " + page.ToString() + " Id: " + id ?? string.Empty);

			if (!enableMobileCenterAnalytics)
				return;
			Analytics.TrackEvent($"{page}Page");
		}


		public virtual void Track(string trackIdentifier)
		{
			Debug.WriteLine("ASE Logger: Track: " + trackIdentifier);

			if (!enableMobileCenterAnalytics)
				return;
			Analytics.TrackEvent(trackIdentifier);
		}

		public virtual void Track(string trackIdentifier, string key, string value)
		{
			Debug.WriteLine("ASE Logger: Track: " + trackIdentifier + " key: " + key + " value: " + value);

			if (!enableMobileCenterAnalytics)
				return;

			trackIdentifier = $"{trackIdentifier}-{key}-{@value}";

			Analytics.TrackEvent(trackIdentifier);
		}

		public virtual void Report(Exception exception = null, Severity warningLevel = Severity.Warning)
		{
			Debug.WriteLine("ASE Logger: Report: " + exception);
		}
		public virtual void Report(Exception exception, IDictionary extraData, Severity warningLevel = Severity.Warning)
		{
			Debug.WriteLine("ASE Logger: Report: " + exception);
		}
		public virtual void Report(Exception exception, string key, string value, Severity warningLevel = Severity.Warning)
		{
			Debug.WriteLine("ASE Logger: Report: " + exception + " key: " + key + " value: " + value);
		}
		#endregion
	}


}

