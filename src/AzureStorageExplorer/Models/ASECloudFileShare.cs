using System;
using Microsoft.WindowsAzure.Storage.File;
using Realms;

namespace AzureStorageExplorer
{
    public class ASECloudFileShare
    {
        [Ignored]
        public CloudFileShare BaseFileShare
        {
            get;
            set;
        }

		public string StorageAccountName
		{
			get;
			set;
		}

		public string FileShareName
		{
			get;
			set;
		}

		public ASECloudFileShare()
		{
		}

        public ASECloudFileShare(CloudFileShare fileShare)
		{
            BaseFileShare = fileShare;
            FileShareName = fileShare.Name;
		}

		public ASECloudFileShare(CloudFileShare fileShare, string storageAccountName)
		{
			BaseFileShare = fileShare;
			FileShareName = fileShare.Name;
			StorageAccountName = storageAccountName;
		}
    }
}
