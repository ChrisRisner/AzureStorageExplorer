using System;
using Realms;

namespace AzureStorageExplorer.PCL
{
	public class RealmCloudBlobContainer : RealmObject
	{
		public string ContainerName
		{
			get;
			set;
		}

		public string StorageAccountName
		{
			get;
			set;
		}

		public string ContainerUri
		{
			get;
			set;
		}

		public RealmCloudBlobContainer(string containerName, string storageAccountName, string containerUri)
		{
			ContainerName = containerName;
			StorageAccountName = storageAccountName;
			ContainerUri = containerUri;
		}

		public RealmCloudBlobContainer()
		{
		}
	}
}
