using System;
using System.Threading.Tasks;
using System.Windows.Input;
using FormsToolkit;
using Microsoft.WindowsAzure.Storage.Blob;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;

namespace AzureStorageExplorer
{
	public class DeleteBlobViewModel : ViewModelBase
	{
		CloudBlob blob;

		public CloudBlob Blob
		{
			get { return this.blob; }
			set { SetProperty(ref blob, value); }
		}

		string blobNameConfirmation;
		public string BlobNameConfirmation
		{
			get { return blobNameConfirmation; }
			set { 
				SetProperty(ref blobNameConfirmation, value); 
				OnPropertyChanged("ReadyToDelete");
			}
		}

		public bool ReadyToDelete
		{
			get
			{
				if (BlobNameConfirmation == Blob.Name)
					return true;
				return false;
			}
		}
			
		public DeleteBlobViewModel(INavigation navigation, CloudBlob blob) : base(navigation)
		{
			this.Blob = blob;
		}

		ICommand deleteBlobCommand;
		public ICommand DeleteBlobCommand =>
		deleteBlobCommand ?? (deleteBlobCommand = new Command(async () => await ExecuteDeleteBlobCommandlAsync()));

		async Task ExecuteDeleteBlobCommandlAsync()
		{
			MessagingService.Current.SendMessage(MessageKeys.DeleteBlob, new MessageArgsDeleteBlob(Blob.Name, Blob.Container.Name));
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
