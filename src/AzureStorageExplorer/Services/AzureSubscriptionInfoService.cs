using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using AzureStorageExplorer.PCL;
using FormsToolkit;
using Newtonsoft.Json;

namespace AzureStorageExplorer.Services
{
	public class AzureSubscriptionInfoService
	{
		public static async Task<TenantResponse> GetTenants(string accessToken)
		{
			TenantResponse tenantCollection = null;
			var requestUrl = "https://management.azure.com/tenants?api-version=2015-01-01";
			try
			{
				HttpClient client = new HttpClient();
				client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
				client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);


				var tenantResponse = await client.GetStringAsync(requestUrl);
				tenantCollection = JsonConvert.DeserializeObject<TenantResponse>(tenantResponse);
			}
			catch (Exception ex)
			{
				Console.WriteLine("Error getting tenants: " + ex.Message);
				
				MessagingService.Current.SendMessage<MessagingServiceAlert>(MessageKeys.Message, new MessagingServiceAlert
				{
					Title = "Unable to fetch Tenants",
					Message = "There was an issue requesting your Azure tenants.",
					Cancel = "OK"
				});
			}
			foreach (var tenant in tenantCollection.TenantCollection)
			{
				Console.WriteLine(tenant.Id + " :: " + tenant.TenantId);
			}

			return tenantCollection;
		}

		public static async Task<SubscriptionResponse.Subscription[]> GetSubscriptionsForTenant(string tenantId, string accessToken)
		{
			List<SubscriptionResponse.Subscription> subscriptions = new List<SubscriptionResponse.Subscription>();
			var requestUrl = "https://management.azure.com/subscriptions?api-version=2014-04-01";
			try
			{
				HttpClient client = new HttpClient();				
				client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
                var subsriptionResponse = await client.GetStringAsync(requestUrl);
				var subscriptionData = JsonConvert.DeserializeObject<SubscriptionResponse>(subsriptionResponse);
				foreach (var subscription in subscriptionData.SubscriptionCollection)
				{
					Console.WriteLine("subscrip: " + subscription.Id);
					subscription.Token = accessToken;
					subscriptions.Add(subscription);
				}
				return subscriptions.ToArray();

			}
			catch (Exception ex)
			{
				Console.WriteLine("Error: " + ex.Message);

				MessagingService.Current.SendMessage<MessagingServiceAlert>(MessageKeys.Message, new MessagingServiceAlert
				{
					Title = "Unable to fetch Subscriptions",
					Message = "There was an issue requesting your Azure Subscriptions.",
					Cancel = "OK"
				});

				throw (ex);
			}
		}
	}
}
