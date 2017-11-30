using System;
using System.Threading.Tasks;
using AzureStorageExplorer.iOS;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Xamarin.Forms;
using System.Linq;
using UIKit;

[assembly: Dependency(typeof(SignOnClient))]
namespace AzureStorageExplorer.iOS
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
			var authContext = new AuthenticationContext(Constants.AuthCommonAuthority);
			if (authContext.TokenCache.ReadItems().Any())
			{
				if (authContext.TokenCache.ReadItems().First().ExpiresOn.CompareTo(DateTimeOffset.Now) < 0)
				{
					Console.WriteLine("Token has expired");
					authContext = new AuthenticationContext(authContext.TokenCache.ReadItems().First().Authority);
					var res = await authContext.AcquireTokenSilentAsync(Constants.AuthManagementResourceUri, ApiKeys.AuthClientId,
																		new UserIdentifier(authContext.TokenCache.ReadItems().First().UniqueId, UserIdentifierType.UniqueId));
					return res;
				}
				authContext = new AuthenticationContext(authContext.TokenCache.ReadItems().First().Authority);
			}

			authContext.ExtendedLifeTimeEnabled = true;
			var window = UIApplication.SharedApplication.KeyWindow;
			var vc = window.RootViewController;
			while (vc.PresentedViewController != null)
			{
				vc = vc.PresentedViewController;
			}

			var authResult = await authContext.AcquireTokenAsync(Constants.AuthManagementResourceUri, ApiKeys.AuthClientId,
																 Constants.AuthReturnUri, new PlatformParameters(vc));
			return authResult;
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

