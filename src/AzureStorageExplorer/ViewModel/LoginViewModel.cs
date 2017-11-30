using System;
using System.Threading.Tasks;
using System.Windows.Input;
using AzureStorageExplorer.PCL;
using FormsToolkit;
using Realms;
using Xamarin.Forms;

namespace AzureStorageExplorer
{
	public class LoginViewModel : ViewModelBase
	{
		ISignOnClient signOnClient;

		string message;
		public string Message
		{
			get { return message; }
			set { SetProperty(ref message, value); }
		}

		public LoginViewModel(INavigation navigation) : base(navigation)
		{
			signOnClient = DependencyService.Get<ISignOnClient>();
		}

		ICommand loginCommand;
		public ICommand LoginCommand =>
		loginCommand ?? (loginCommand = new Command(async () => await ExecuteLoginAsync()));

		async Task ExecuteLoginAsync()
		{
			try
			{
				IsBusy = true;
				var result = await signOnClient.Authenticate();
				Settings.Current.Email = result.UserInfo.DisplayableId;
				Settings.Current.FirstName = result.UserInfo.GivenName;
				Settings.Current.LastName = result.UserInfo.FamilyName;
				Message = "Sign in successful, fetching Subscriptions";
				var tenants = await Services.AzureSubscriptionInfoService.GetTenants(result.AccessToken);
                foreach (var tenant in tenants.TenantCollection)
				{
					var subResult = await signOnClient.AuthenticateSilently(tenant.TenantId, result.UserInfo.UniqueId);
					var subs = await Services.AzureSubscriptionInfoService.GetSubscriptionsForTenant(tenant.TenantId, subResult.AccessToken);
					foreach (var sub in subs)
						sub.tenantId = tenant.TenantId;
					App.current.Subscriptions.AddRange(subs);
				}

				Message = "Subscriptions obtained, fetching Storage Accounts";
                foreach (var sub in App.current.Subscriptions)
				{
					await AzureManagementHelper.GetStorageAccountsForSubscription(sub.SubscriptionId, sub.Token, sub.DisplayName,sub.tenantId);
				}
                //Send them to the navigation page
				var nav = Application.Current?.MainPage?.Navigation;
				if (nav == null)
					return;
				Settings.FirstRun = false;
				Settings.Current.ForceUserLogin = false;
				await Navigation.PopModalAsync();
                MessagingService.Current.SendMessage(MessageKeys.NavigateToStorageAccounts);
			}
			catch (Exception ex)
			{
				Logger.Track(ASELoggerKeys.LoginFailure, "Reason", ex?.Message ?? string.Empty);
				MessagingService.Current.SendMessage<MessagingServiceAlert>(MessageKeys.Message, new MessagingServiceAlert
				{
					Title = "Unable to Sign in",
					Message = "There was an issue signing in.",
					Cancel = "OK"
				});
                IsBusy = false;
			}
			finally
			{
				//IsBusy = false;
			}
		}

		ICommand cancelCommand;
		public ICommand CancelCommand =>
		cancelCommand ?? (cancelCommand = new Command(async () => await ExecuteCancelAsync()));

		async Task ExecuteCancelAsync()
		{
			
		}

		ICommand signupCommand;
		public ICommand SignupCommand =>
		signupCommand ?? (signupCommand = new Command(async () => await ExecuteSignupAsync()));

		async Task ExecuteSignupAsync()
		{

		}
	}
}

