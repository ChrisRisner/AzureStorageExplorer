using System;
using Realms;

namespace AzureStorageExplorer.PCL
{
	public class RealmCloudQueue : RealmObject
	{
		public string QueueName
		{
			get;
			set;
		}

		public string StorageAccountName
		{
			get;
			set;
		}

		public string QueueUri
		{
			get;
			set;
		}

		public RealmCloudQueue(string queueName, string storageAccountName, string queueUri)
		{
			QueueName = queueName;
			StorageAccountName = storageAccountName;
			QueueUri = queueUri;
		}

		public RealmCloudQueue()
		{
		}
	}
}
