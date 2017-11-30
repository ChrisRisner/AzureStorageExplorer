using System;
using Xamarin.Forms;
using ToastIOS;
using AzureStorageExplorer.iOS;

[assembly:Dependency(typeof(Toaster))]
namespace AzureStorageExplorer.iOS
{
	public class Toaster : IToast
	{
		public void SendToast(string message)
		{
			Device.BeginInvokeOnMainThread(() =>
				{
					Toast.MakeText(message, Toast.LENGTH_LONG).SetCornerRadius(0).Show();
				});
		}
	}
}

