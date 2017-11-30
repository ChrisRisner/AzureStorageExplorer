using System;
using System.Collections.Generic;
using Microsoft.WindowsAzure.Management.Storage.Models;
using Realms;

namespace AzureStorageExplorer.PCL
{
	public class StorageAccountExt : RealmObject
	{
		//Empty Constructor required by Realm
		public StorageAccountExt() { }	

		public StorageAccountExt(StorageAccount baseAccount, string primaryKey, string secondaryKey, string subscriptionName,
		                        bool isStorageAccountOn = false)
		{
			ExtendedProperties = baseAccount.ExtendedProperties;
			MigrationState = baseAccount.MigrationState;
			Name = baseAccount.Name;
			Properties = baseAccount.Properties;
			Uri = baseAccount.Uri;
			PrimaryKey = primaryKey;
			SecondaryKey = secondaryKey;
			SubscriptionName = subscriptionName;
			IsStorageAccountOn = isStorageAccountOn;
		}

		public bool IsStorageAccountOn
		{
			get;
			set;
		}

		public string PrimaryKey
		{
			get;
			set;
		}

		public string SecondaryKey
		{
			get;
			set;
		}

		public string SubscriptionName
		{
			get;
			set;
		}

		[Ignored]
		public IDictionary<string, string> ExtendedProperties
		{
			get;
			set;
		}

		public string MigrationState
		{
			get;
			set;
		}

		[PrimaryKey]
		public string Name
		{
			get;
			set;
		}

		[Ignored]
		public StorageAccountProperties Properties
		{
			get;
			set;
		}

		[Ignored]
		public Uri Uri
		{
			get;
			set;
		}
	}
}
