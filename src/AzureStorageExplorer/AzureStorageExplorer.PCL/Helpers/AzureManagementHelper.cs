using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Management.Storage;
using Microsoft.WindowsAzure.Management.Storage.Models;
using ModernHttpClient;
using Realms;

namespace AzureStorageExplorer.PCL
{
	public class AzureManagementHelper
	{
		public static async Task<List<StorageAccountExt>> GetStorageAccountsForSubscription(string subscriptionId, string token, string subscriptionName, string tenantId)
		{
			List<StorageAccountExt> storageAccounts = new List<StorageAccountExt>();
			try
			{
				TokenCloudCredentials cred = new TokenCloudCredentials(subscriptionId, token);
				var httpClient = new HttpClient(new NativeMessageHandler());
				StorageManagementClient client = new StorageManagementClient(cred, httpClient);
				var response = await client.StorageAccounts.ListAsync();
				var realm = Realm.GetInstance();
				//Save our subscriptions to the realm				
				var existingSubscription = realm.Find<Subscription>(subscriptionId);
				if (existingSubscription == null)
				{
					await realm.WriteAsync(temprealm =>
						{
							temprealm.Add(new Subscription(subscriptionName, subscriptionId, tenantId, true), true);
						});
				}

				foreach (var accnt in response.StorageAccounts)
				{					
					Debug.WriteLine("Storage account: " + accnt.Name);
					var keys = await GetStorageAccountKeys(subscriptionId, token, accnt.Name);
					Debug.WriteLine("PRimary key: " + keys.PrimaryKey);
					StorageAccountExt extendedAccount = new StorageAccountExt(accnt, keys.PrimaryKey, keys.SecondaryKey, subscriptionName);
                    storageAccounts.Add(extendedAccount);
					var existingStorageAccount = realm.Find<StorageAccountExt>(extendedAccount.Name);
					if (existingStorageAccount != null && (existingStorageAccount.PrimaryKey != keys.PrimaryKey || existingStorageAccount.SecondaryKey != keys.SecondaryKey))
					{
						existingStorageAccount.PrimaryKey = keys.PrimaryKey;
						existingStorageAccount.SecondaryKey = keys.SecondaryKey;
					} 
					else if (existingStorageAccount == null)
					{
						await realm.WriteAsync(temprealm =>
						{
							temprealm.Add(extendedAccount, true);
						});
					}
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine("Error getting storage accounts for sub: " + subscriptionName);
				Debug.WriteLine("Error getting subscription Storage Account info: " + ex.Message);
				Debug.WriteLine(ex.StackTrace);
			}
			return storageAccounts;
		}

		public static async Task<StorageAccountGetKeysResponse> GetStorageAccountKeys(string subscriptionId, string token, string storageAccountName)
		{
			TokenCloudCredentials cred = new TokenCloudCredentials(subscriptionId, token);
			var httpClient = new HttpClient(new NativeMessageHandler());
			StorageManagementClient client = new StorageManagementClient(cred, httpClient);
			var response = await client.StorageAccounts.GetKeysAsync(storageAccountName);
			return response;
		}
	}
}
