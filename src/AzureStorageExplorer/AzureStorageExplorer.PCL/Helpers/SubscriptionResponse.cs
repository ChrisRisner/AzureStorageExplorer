using Newtonsoft.Json;

namespace AzureStorageExplorer.PCL
{
	public partial class SubscriptionResponse
	{
		public class SubscriptionPolicies
		{

			[JsonProperty("locationPlacementId")]
			public string LocationPlacementId { get; set; }

			[JsonProperty("quotaId")]
			public string QuotaId { get; set; }
		}
	}

	public partial class SubscriptionResponse
	{
		public class Subscription
		{

			[JsonProperty("id")]
			public string Id { get; set; }

			[JsonProperty("subscriptionId")]
			public string SubscriptionId { get; set; }

			[JsonProperty("displayName")]
			public string DisplayName { get; set; }

			[JsonProperty("state")]
			public string State { get; set; }

			[JsonProperty("subscriptionPolicies")]
			public SubscriptionPolicies SubscriptionPolicy { get; set; }

			public string Token
			{
				get;
				set;
			}

			public string tenantId
			{
				get;
				set;
			}
		}

		[JsonProperty("value")]
		public Subscription[] SubscriptionCollection { get; set; }

		[JsonConstructor]
		public SubscriptionResponse() { 
			//EmptyConstructor
		}
	}


}