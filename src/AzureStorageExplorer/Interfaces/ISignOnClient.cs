using System;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace AzureStorageExplorer
{
	public interface ISignOnClient
	{
		Task<bool> LoginAsync();
		Task<AuthenticationResult> Authenticate();

		Task<bool> SilentlyRefreshUser();
		bool IsUserAuthenticated();
		Task<AuthenticationResult> AuthenticateSilently(string tenantId, string userId);
		void LogoutAsync();
		string GetTokenForTenant(string tenantId);


	}
}
