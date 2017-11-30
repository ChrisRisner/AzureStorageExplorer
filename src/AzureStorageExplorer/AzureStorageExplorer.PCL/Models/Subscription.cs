using System;
using Realms;

namespace AzureStorageExplorer.PCL
{
	public class Subscription : RealmObject
	{
		[PrimaryKey]
		public string Id
		{
			get;
			set;
		}

		public string Name
		{
			get;
			set;
		}

		public bool IsSubOn
		{
			get;
			set;
		}

		public string TenantId
		{
			get;
			set;
		}

		public Subscription() { }

		public Subscription(string name, string id, string tenantId, bool isSubOn = false)
		{
			Name = name;
			Id = id;
			TenantId = tenantId;
			IsSubOn = isSubOn;
		}
	}
}
