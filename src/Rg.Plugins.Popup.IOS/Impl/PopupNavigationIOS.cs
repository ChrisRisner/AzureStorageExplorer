﻿using System.Linq;
using Foundation;
using Rg.Plugins.Popup.Contracts;
using Rg.Plugins.Popup.IOS.Extensions;
using Rg.Plugins.Popup.IOS.Impl;
using Rg.Plugins.Popup.Pages;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: Dependency(typeof(PopupNavigationIOS))]
namespace Rg.Plugins.Popup.IOS.Impl
{
    [Preserve(AllMembers = true)]
    internal class PopupNavigationIOS : IPopupNavigation
    {
        public void AddPopup(PopupPage page)
        {
            var topViewController = GetTopViewController();
            var topRenderer = topViewController.ChildViewControllers.LastOrDefault() as IVisualElementRenderer;

            if (topRenderer != null)
                page.Parent = topRenderer.Element;
            else
                page.Parent = Application.Current.MainPage;

            var renderer = page.GetOrCreateRenderer();

            topViewController.AddChildViewController(renderer.ViewController);
            topViewController.View.AddSubview(renderer.NativeView);
            renderer.ViewController.DidMoveToParentViewController(topViewController);
        }

        public void RemovePopup(PopupPage page)
        {
            var renderer = page.GetOrCreateRenderer();
            var viewController = renderer?.ViewController;

            if (viewController != null && !viewController.IsBeingDismissed)
            {
                viewController.WillMoveToParentViewController(null);
                viewController.RemoveFromParentViewController();
                renderer.NativeView.RemoveFromSuperview();
            }

        }

        private UIViewController GetTopViewController()
        {
            var topViewController = UIApplication.SharedApplication.KeyWindow.RootViewController;

            while (topViewController.PresentedViewController != null)
            {
                topViewController = topViewController.PresentedViewController;
            }
			topViewController = Popup.PLATFORM_RENDERER;
            return topViewController;
        }
    }
}
