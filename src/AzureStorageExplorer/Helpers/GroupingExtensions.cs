using System;
using System.Collections.Generic;
using System.Linq;
using AzureStorageExplorer.PCL;
using MvvmHelpers;

namespace AzureStorageExplorer
{
	public static class GroupingExtensions
	{
		public static IEnumerable<Grouping<string, ASECloudQueue>> FilterByStorageAccount(this IList<ASECloudQueue> queues)
		{
			var grouped = (from queue in queues
			               orderby queue.StorageAccountName, queue.BaseQueue.Name
						   group queue by queue.StorageAccountName
							into queueGroup
			               select new Grouping<string, ASECloudQueue>(queueGroup.Key, queueGroup)).ToList();
			return grouped;
		}

		public static IEnumerable<Grouping<string, ASECloudBlobContainer>> FilterByStorageAccount(this IList<ASECloudBlobContainer> containers)
		{
			var grouped = (from container in containers
			               orderby container.StorageAccountName, container.BaseContainer.Name
						   group container by container.StorageAccountName
							into containerGroup
						   select new Grouping<string, ASECloudBlobContainer>(containerGroup.Key, containerGroup)).ToList();
			return grouped;
		}

		public static IEnumerable<Grouping<string, StorageAccountExt>> FilterBySubscription(this IList<StorageAccountExt> storageAccounts)
		{
			var grouped = (from storageAccount in storageAccounts
			               orderby storageAccount.SubscriptionName, storageAccount.Name
			               group storageAccount by storageAccount.SubscriptionName
							into storageAccountGroup
			               select new Grouping<string, StorageAccountExt>(storageAccountGroup.Key, storageAccountGroup)).ToList();
			return grouped;
		}

		public static IEnumerable<Grouping<string, ASECloudTable>> FilterByStorageAccount(this IList<ASECloudTable> tables)
		{
			var grouped = (from table in tables
			               orderby table.StorageAccountName, table.BaseTable.Name
						   group table by table.StorageAccountName
							into tableGroup
			               select new Grouping<string, ASECloudTable>(tableGroup.Key, tableGroup)).ToList();
			return grouped;
		}

        public static IEnumerable<Grouping<string, ASECloudFileShare>> FilterByStorageAccount(this IList<ASECloudFileShare> fileShares)
		{
			var grouped = (from fileShare in fileShares
                           orderby fileShare.StorageAccountName, fileShare.BaseFileShare.Name
						   group fileShare by fileShare.StorageAccountName
                            into fileShareGroup
                           select new Grouping<string, ASECloudFileShare>(fileShareGroup.Key, fileShareGroup)).ToList();
			return grouped;
		}
	}
}
