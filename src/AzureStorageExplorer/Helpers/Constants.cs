using System;
namespace AzureStorageExplorer
{
	public static class Constants
	{
		public const string AuthCommonAuthority = "https://login.windows.net/common";
		public const string AuthNoTenantAuthority = "https://login.microsoftonline.com/";
		public static readonly Uri AuthReturnUri = new Uri("urn:ietf:wg:oauth:2.0:oob");
        public const string AuthGraphResourceUri = "https://graph.windows.net";
		public const string AuthGraphApiVersion = "2013-11-08";
		public const string AuthManagementResourceUri = "https://management.core.windows.net/";
		public const string StorageConnectionString = "DefaultEndpointsProtocol=https;AccountName=<ACCOUNTNAME>;AccountKey=<ACCOUNTKEY>";
		public static readonly string[] AuthScopes = { "User.Read" };
		public const int MaxMbDownloadSize = 50;
	}

	public static class ApiKeys
	{

#if DEBUG
		public const string AuthClientId = "__AuthClientId__";

#if __IOS__
        public const string MobileCenterId = "__MobileCenterId__";
#endif

#if __ANDROID__
        public const string MobileCenterId = "__MobileCenterId__";
#endif

#else
		public const string MobileCenterId = "__MobileCenterId__";

        public const string AuthClientId = "__AuthClientId__";
#endif
	}

	public static class MessageKeys
	{
		public const string NavigateLogin = "navigate_login";
		public const string NavigateToSubscriptions = "navigate_subscriptions";
		public const string NavigateToStorageAccounts = "navigate_storageaccounts";
		public const string Message = "message";
		public const string Error = "error";
		public const string DeleteContainer = "delete_container";
		public const string DeleteBlob = "delete_blob";
        public const string DeleteFileShare = "delete_file_share";
        public const string DeleteFile = "delete_file";
		public const string DeleteQueue = "delete_queue";
		public const string DeleteQueueMessage = "delete_queue_message";
		public const string DeleteTable = "delete_table";
		public const string DeleteTableRow = "delete_table_row";
		public const string BlobDownloaded = "blob_downloaded";
		public const string Question = "question";
	}

	public class MessageArgsDeleteQueue
	{
		public string QueueName;
		public string StorageAccountName;
		public MessageArgsDeleteQueue(string queueName, string storageAccountName)
		{
			this.QueueName = queueName;
			this.StorageAccountName = storageAccountName;
		}
	}

	public class MessageArgsDeleteTable
	{
		public string TableName;
		public string StorageAccountName;
		public MessageArgsDeleteTable(string tableName, string storageAccountName)
		{
			this.TableName = tableName;
			this.StorageAccountName = storageAccountName;
		}
	}

	public class MessageArgsDeleteTableRow
	{
		public string RowId;
		public MessageArgsDeleteTableRow(string rowId)
		{
			this.RowId = rowId;
		}
	}

	public class MessageArgsDeleteQueueMessage
	{
		public string QueueId;
		public MessageArgsDeleteQueueMessage(string queueId)
		{
			this.QueueId = queueId;
		}
	}

	public class MessageArgsDeleteFileShare
	{
		public string FileShareName;
		public string StorageAccountName;
		public MessageArgsDeleteFileShare(string fileShareName, string storageAccountName)
		{
            this.FileShareName = fileShareName;
			this.StorageAccountName = storageAccountName;
		}
	}

	public class MessageArgsDeleteFile
	{
		public string FileName;
		public MessageArgsDeleteFile(string fileName)
		{
            this.FileName = fileName;
		}
	}

	public class MessageArgsDeleteContainer
	{
		public string ContainerName;
		public string StorageAccountName;
		public MessageArgsDeleteContainer(string containerName, string storageAccountName)
		{
			this.ContainerName = containerName;
			this.StorageAccountName = storageAccountName;
		}
	}

	public class MessageArgsDeleteBlob
	{
		public string ContainerName;
		public string BlobName;
		public MessageArgsDeleteBlob(string blobName, string containerName)
		{
			this.BlobName = blobName;
			this.ContainerName = containerName;
		}
	}

	public class MessageArgsBlobDownloaded
	{
		public string BlobPath;
		public MessageArgsBlobDownloaded(string blobPath)
		{
			this.BlobPath = blobPath;
		}
	}



}
