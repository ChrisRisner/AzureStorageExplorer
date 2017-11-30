using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace AzureStorageExplorer
{
    public class FileShareCell : ViewCell
    {
        readonly INavigation navigation;
        public FileShareCell(INavigation navigation = null)
        {
            View = new FileShareCellView();
            this.navigation = navigation;

        }
    }

    public partial class FileShareCellView : ContentView
    {
        public FileShareCellView()
        {
            InitializeComponent();
        }
    }
}
