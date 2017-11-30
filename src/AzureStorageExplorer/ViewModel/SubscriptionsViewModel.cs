using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using AzureStorageExplorer.PCL;
using FormsToolkit;
using MvvmHelpers;
using Realms;
using Xamarin.Forms;

namespace AzureStorageExplorer
{
	public class SubscriptionsViewModel : ViewModelBase
	{
		public ObservableRangeCollection<Subscription> Subscriptions { get; } = new ObservableRangeCollection<Subscription>();

		bool noSubscriptionsFound;
		public bool NoSubscriptionsFound
		{
			get { return noSubscriptionsFound; }
			set { SetProperty(ref noSubscriptionsFound, value); }
		}

		string noSubscriptionsFoundMessage;
		public string NoSubscriptionsFoundMessage
		{
			get { return noSubscriptionsFoundMessage; }
			set { SetProperty(ref noSubscriptionsFoundMessage, value); }
		}

		public SubscriptionsViewModel(INavigation navigation) : base(navigation)
		{

		}

		ICommand loadSubscriptionsCommand;
		public ICommand LoadSubscriptionsCommand =>
			loadSubscriptionsCommand ?? (loadSubscriptionsCommand = new Command<bool>(async (f) => await ExecuteLoadSubscriptionsAsync()));
        async Task<bool> ExecuteLoadSubscriptionsAsync(bool force = false)
		{
			if (IsBusy)
				return false;

			try
			{				
				IsBusy = true;
				NoSubscriptionsFound = false;
				var realm = App.GetRealm();
				var subs = realm.All<Subscription>();
				subs.SubscribeForNotifications((sender, changes, error) =>
				{
					Console.WriteLine("Change to Subscription");
				});
                Subscriptions.ReplaceRange(subs.OrderBy(sub => sub.Name));
                if (Subscriptions.Count == 0)
				{
					NoSubscriptionsFoundMessage = "No Subscriptions Found";
					NoSubscriptionsFound = true;
				}
				else
				{
					NoSubscriptionsFound = false;
				}
			}
			catch (Exception ex)
			{
				Logger.Report(ex, "Method", "ExecuteLoadSubscriptionsAsync");
				MessagingService.Current.SendMessage(MessageKeys.Error, ex);
			}
			finally
			{
				IsBusy = false;
			}
			return true;
		}
	}
}
