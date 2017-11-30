using System;
using Realms;

namespace AzureStorageExplorer.PCL
{
	public class RealmCloudFileShare : RealmObject
	{
		public string FileShareName
		{
			get;
			set;
		}

		public string StorageAccountName
		{
			get;
			set;
		}

		public string FileShareUri
		{
			get;
			set;
		}

        public RealmCloudFileShare(string fileShareName, string storageAccountName, string fileShareUri)
		{
            FileShareName = fileShareName;
			StorageAccountName = storageAccountName;
            FileShareUri = fileShareUri;
		}

		public RealmCloudFileShare()
		{
		}
	}
}
