using System;
using Xamarin.Forms;
using AzureStorageExplorer.Droid;
using Android.Widget;
using Plugin.CurrentActivity;

[assembly: Dependency(typeof(Toaster))]
namespace AzureStorageExplorer.Droid
{
	public class Toaster : IToast
	{
		public void SendToast(string message)
		{
			var context = CrossCurrentActivity.Current.Activity ?? Android.App.Application.Context;
			Device.BeginInvokeOnMainThread(() =>
				{
					Toast.MakeText(context, message, ToastLength.Long).Show();
				});

		}
	}
}

