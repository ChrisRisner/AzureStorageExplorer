using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace AzureStorageExplorer
{
	public class ASEGroupHeader : ViewCell
	{
		public ASEGroupHeader()
		{
			View = new ASEGroupHeaderView();
		}
	}
	public partial class ASEGroupHeaderView : ContentView
	{
		public ASEGroupHeaderView()
		{
			InitializeComponent();
		}
	}
}
