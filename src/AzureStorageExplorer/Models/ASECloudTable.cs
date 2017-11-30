using System;
using Microsoft.WindowsAzure.Storage.Table;
using Realms;

namespace AzureStorageExplorer
{
	public class ASECloudTable
	{
		[Ignored]
		public CloudTable BaseTable
		{
			get; set;
		}

		public string StorageAccountName
		{
			get;
			set;
		}

		public string TableName
		{
			get;
			set;
		}

		public ASECloudTable()
		{

		}

		public ASECloudTable(CloudTable table)
		{
			BaseTable = table;
			TableName = table.Name;
		}

		public ASECloudTable(CloudTable table, string storageAccountName)
		{
			BaseTable = table;
			TableName = table.Name;
			StorageAccountName = storageAccountName;
		}
	}
}