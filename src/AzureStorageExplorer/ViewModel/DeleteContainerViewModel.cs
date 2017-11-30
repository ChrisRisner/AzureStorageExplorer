using System;
using System.Threading.Tasks;
using System.Windows.Input;
using FormsToolkit;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;

namespace AzureStorageExplorer
{
	public class DeleteContainerViewModel : ViewModelBase
	{
		string containerName = null;
		string storageAccountName = null;

		public string ContainerName
		{
			get
			{
				return this.containerName;
			}
		}

		string containerNameConfirmation;
		public string ContainerNameConfirmation
		{
			get { return containerNameConfirmation; }
			set { 
				value = value.ToLower();
				SetProperty(ref containerNameConfirmation, value); 
				OnPropertyChanged("ReadyToDelete");
			}
		}

		public bool ReadyToDelete
		{
			get
			{
				if (ContainerNameConfirmation == containerName)
					return true;
				return false;
			}
		}

		public DeleteContainerViewModel(INavigation navigation, string containerName, string storageAccountName) : base(navigation)
		{
			this.containerName = containerName;
			this.storageAccountName = storageAccountName;
		}

		ICommand deleteContainerCommand;
		public ICommand DeleteContainerCommand =>
		deleteContainerCommand ?? (deleteContainerCommand = new Command(async () => await ExecuteDeleteContainerCommandlAsync()));

		async Task ExecuteDeleteContainerCommandlAsync()
		{
			MessagingService.Current.SendMessage(MessageKeys.DeleteContainer, new MessageArgsDeleteContainer(containerName, storageAccountName));
			await PopupNavigation.PopAsync();
		}

		ICommand cancelCommand;
		public ICommand CancelCommand =>
		cancelCommand ?? (cancelCommand = new Command(async () => await ExecuteCancelAsync()));

		async Task ExecuteCancelAsync()
		{			
			await PopupNavigation.PopAsync();
		}
	}
}
