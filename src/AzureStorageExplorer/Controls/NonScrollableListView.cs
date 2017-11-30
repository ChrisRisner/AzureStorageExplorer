using System;
using Xamarin.Forms;

namespace AzureStorageExplorer
{
public class NonScrollableListView : ListView
{
	public NonScrollableListView()
		: base(ListViewCachingStrategy.RecycleElement)
	{
            if (Device.RuntimePlatform == Device.Windows || Device.RuntimePlatform == Device.WinPhone)
			BackgroundColor = Color.White;
	} }
}
