using System;
using System.Threading.Tasks;
using AzureStorageExplorer.Droid;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Xamarin.Forms;
using System.Linq;
using Android.App;
using Plugin.CurrentActivity;

[assembly: Dependency(typeof(SignOnClient))]
namespace AzureStorageExplorer.Droid
{
	public class SignOnClient : ISignOnClient
	{
		public Task<bool> LoginAsync()
		{
			throw new Exception("unimplemented method");
		}

		public void LogoutAsync()
		{
			var authContext = new AuthenticationContext(Constants.AuthCommonAuthority);
			if (authContext.TokenCache.ReadItems().Any())
			{
				authContext.TokenCache.Clear();
			}
		}

		public string GetTokenForTenant(string tenantId)
		{
			var authContext = new AuthenticationContext(Constants.AuthCommonAuthority);
			var tokenInfo = authContext.TokenCache.ReadItems().FirstOrDefault((arg) => arg.TenantId == tenantId);
			return tokenInfo != null ? tokenInfo.AccessToken : null;
		}

		public bool IsUserAuthenticated()
		{
			var authContext = new AuthenticationContext(Constants.AuthCommonAuthority);
			if (authContext.TokenCache.ReadItems().Any())
			{
				if (authContext.TokenCache.ReadItems().First().ExpiresOn.CompareTo(DateTimeOffset.Now) >= 0)
				{					
					return true;
				}
			}
			Console.WriteLine("Token has expired, user must relogin");
			return false;
		}

		public async Task<bool> SilentlyRefreshUser()
		{
			var authContext = new AuthenticationContext(Constants.AuthCommonAuthority);
			if (authContext.TokenCache.ReadItems().Any())
			{
				foreach (var token in authContext.TokenCache.ReadItems())
				{
					try
					{
						var res = await AuthenticateSilently(token.TenantId, token.UniqueId);
					}
					catch (Exception ex)
					{
						DependencyService.Get<ILogger>().Track(ASELoggerKeys.LoginFailure, "Reason", ex?.Message ?? string.Empty);
						//User's unable to refresh their tokens for some reason, force them to login before app
						//can be used or any background actions can occur
						Settings.Current.ForceUserLogin = true;
						authContext.TokenCache.Clear();
                        return false;
					}
				}
				return true;
			}
			return false;
		}

		public async Task<AuthenticationResult> Authenticate()
		{
			try
			{
				var authContext = new AuthenticationContext(Constants.AuthCommonAuthority);
				if (authContext.TokenCache.ReadItems().Any())
				{					
					authContext = new AuthenticationContext(authContext.TokenCache.ReadItems().First().Authority);

				}
				authContext.ExtendedLifeTimeEnabled = true;
				var authResult = await authContext.AcquireTokenAsync(Constants.AuthManagementResourceUri, ApiKeys.AuthClientId, Constants.AuthReturnUri, new PlatformParameters(CrossCurrentActivity.Current.Activity));
                return authResult;
            }
			catch (Exception ex)
			{
				Console.WriteLine("Error: " + ex.Message);
				return null;
			}
		}

		public async Task<AuthenticationResult> AuthenticateSilently(string tenantId, string userId)
		{
			try
			{
				var loginAuthority = Constants.AuthNoTenantAuthority + tenantId;
				var authContext = new AuthenticationContext(loginAuthority);
				authContext.ExtendedLifeTimeEnabled = true;
				var authResult = await authContext.AcquireTokenSilentAsync(
					Constants.AuthManagementResourceUri, ApiKeys.AuthClientId,
					new UserIdentifier(userId, UserIdentifierType.UniqueId));

				return authResult;
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}


	}
}
