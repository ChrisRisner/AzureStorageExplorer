using System;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using Realms;

namespace AzureStorageExplorer
{
	public class ASECloudBlobContainer
	{
		[Ignored]
		public CloudBlobContainer BaseContainer
		{
			get; set;
		}

		public string StorageAccountName
		{
			get;
			set;
		}

		public string ContainerName
		{
			get;
			set;
		}

		public ASECloudBlobContainer()
		{

		}

		public ASECloudBlobContainer(CloudBlobContainer container)
		{
			BaseContainer = container;
			ContainerName = container.Name;
		}

		public ASECloudBlobContainer(CloudBlobContainer container, string storageAccountName)
		{
			BaseContainer = container;
			ContainerName = container.Name;
			StorageAccountName = storageAccountName;
		}

	}
}
