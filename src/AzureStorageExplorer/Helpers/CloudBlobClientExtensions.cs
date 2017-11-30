using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.File;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;

namespace AzureStorageExplorer
{
	/// <summary>
	/// Helpful extensions for the CloudBlobClient class.
	/// Found here: https://ahmetalpbalkan.com/blog/azure-listblobssegmentedasync-listcontainerssegmentedasync-how-to/
	/// </summary>
	public static class StorageExtensions
	{
		public static async Task<List<CloudQueue>> ListQueuesAsync(this CloudQueueClient client)
		{
			QueueContinuationToken continuationToken = null;
			List<CloudQueue> results = new List<CloudQueue>();
			do
			{
				var response = await client.ListQueuesSegmentedAsync(continuationToken);
				continuationToken = response.ContinuationToken;
				results.AddRange(response.Results);
			} 
			while (continuationToken != null);
			return results;
		}

		public static async Task<List<CloudBlobContainer>> ListContainersAsync(this CloudBlobClient client)
		{
			BlobContinuationToken continuationToken = null;
			List<CloudBlobContainer> results = new List<CloudBlobContainer>();
			do
			{
				var response = await client.ListContainersSegmentedAsync(continuationToken);
				continuationToken = response.ContinuationToken;
				results.AddRange(response.Results);
			}
			while (continuationToken != null);
			return results;
		}

		public static async Task<List<IListBlobItem>> ListBlobsAsync(this CloudBlobClient client, string prefix) {
			BlobContinuationToken continuationToken = null;
			List<IListBlobItem> results = new List<IListBlobItem>();
			do
			{
				var response = await client.ListBlobsSegmentedAsync(prefix, continuationToken);
				continuationToken = response.ContinuationToken;
				results.AddRange(response.Results);
			}
			while (continuationToken != null);
			return results;

		}

		public static async Task<List<IListBlobItem>> ListBlobsAsync(this CloudBlobContainer container)
		{
			BlobContinuationToken continuationToken = null;
			List<IListBlobItem> results = new List<IListBlobItem>();
			do
			{
				var response = await container.ListBlobsSegmentedAsync(continuationToken);
				continuationToken = response.ContinuationToken;
				results.AddRange(response.Results);
			}
			while (continuationToken != null);
			return results;
		}

		public static async Task<List<CloudTable>> ListTablesAsync(this CloudTableClient client)
		{
			TableContinuationToken continuationToken = null;
			List<CloudTable> results = new List<CloudTable>();
			do
			{
				var response = await client.ListTablesSegmentedAsync(continuationToken);
				continuationToken = response.ContinuationToken;
				results.AddRange(response.Results);
			}
			while (continuationToken != null);
			return results;
		}

        public static async Task<List<CloudFileShare>> ListFileSharesAsync(this CloudFileClient client)
		{
            FileContinuationToken continuationToken = null;
            List<CloudFileShare> results = new List<CloudFileShare>();
			do
			{
                var response = await client.ListSharesSegmentedAsync(continuationToken);
				continuationToken = response.ContinuationToken;
				results.AddRange(response.Results);
			}
			while (continuationToken != null);
			return results;
		}

        public static async Task<List<IListFileItem>> ListFilesAndDirectoriesAsync(this CloudFileDirectory directory)
		{
			FileContinuationToken continuationToken = null;
			List<IListFileItem> results = new List<IListFileItem>();
			do
			{
                var response = await directory.ListFilesAndDirectoriesSegmentedAsync(continuationToken);   
				continuationToken = response.ContinuationToken;
				results.AddRange(response.Results);
			}
			while (continuationToken != null);
			return results;
		}
	}


}
