using System;
namespace AzureStorageExplorer
{

	public class DeepLinkPage
	{
		public AppPage Page { get; set; }
		public string Id { get; set; }
	}

    public enum AppPage
    {
        StorageAccounts,
        Subscriptions,
        About,
        Containers,
        Queues,
        Tables,
        MorePage,
        FileShares,
        Files,
		Login,
		Settings,
		Blobs,
		BlobDetails,
		NewContainer,
		DeleteContainer,
		DeleteBlob,
        NewFileShare,
        NewFile,
        DeleteFileShare,
        DeleteFile,
		NewQueue,
		QueueMessages,
		DeleteQueue,
		NewQueueMessage,
		QueueMessageDetails,
		DeleteQueueMessage,
		TableRows,
		NewTable,
		DeleteTable,
		NewTableRow,
	}

	public enum AppEvent
	{
		DownloadBlob,
		BlobTooLargeForDownload,
		DeleteContainer,
		DeleteBlob,
		ShareBlob,
        DeleteFileShare,
        DeleteFile,
		DeleteQueue,
		DeleteQueueMessage,
		CreatedContainer,
		CreatedBlob,
		CreatedQueue,
		CreatedQueueMessage,
		CreatedTable,
		CreatedTableRow,
        CreatedFileShare,
        CreatedFile,
		OverwriteBlob,
		DeleteTable,
		DeleteTableRow
	}
}
