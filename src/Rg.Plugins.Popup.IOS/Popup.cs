using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using Rg.Plugins.Popup.IOS.Impl;
using Rg.Plugins.Popup.IOS.Renderers;
using Rg.Plugins.Popup.Services;
using UIKit;
using Xamarin.Forms;

namespace Rg.Plugins.Popup.IOS
{
    public static class Popup
    {
		public static UIViewController PLATFORM_RENDERER;

        [Obsolete("Initialization is not required in iOS project")]
        public static void Init()
        {
			PLATFORM_RENDERER = UIApplication.SharedApplication.KeyWindow.RootViewController;
        }
    }
}
