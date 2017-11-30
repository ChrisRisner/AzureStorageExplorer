using System;
using Realms;

namespace AzureStorageExplorer.PCL
{
	public class RealmCloudTable : RealmObject
	{
		public string TableName
		{
			get;
			set;
		}

		public string StorageAccountName
		{
			get;
			set;
		}

		public string TableUri
		{
			get;
			set;
		}

		public RealmCloudTable(string tableName, string storageAccountName, string tableUri)
		{
			TableName = tableName;
			StorageAccountName = storageAccountName;
			TableUri = tableUri;
		}

		public RealmCloudTable()
		{
		}
	}
}
