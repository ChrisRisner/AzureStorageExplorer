using System;
using Microsoft.WindowsAzure.Storage.Queue;
using Realms;

namespace AzureStorageExplorer
{
	public class ASECloudQueue
	{
		[Ignored]
		public CloudQueue BaseQueue
		{
			get; set;
		}

		public string StorageAccountName
		{
			get;
			set;
		}

		public string QueueName
		{
			get;
			set;
		}

		public ASECloudQueue()
		{

		}

		public ASECloudQueue(CloudQueue queue)
		{
			BaseQueue = queue;
			QueueName = queue.Name;
		}

		public ASECloudQueue(CloudQueue queue, string storageAccountName)
		{
			BaseQueue = queue;
			QueueName = queue.Name;
			StorageAccountName = storageAccountName;
		}
	}
}
