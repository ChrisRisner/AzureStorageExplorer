using System;
using System.Linq;
using Android.App;
using Android.Runtime;
using Android.Widget;
using Rg.Plugins.Popup.Contracts;
using Rg.Plugins.Popup.Droid.Extensions;
using Rg.Plugins.Popup.Droid.Helpers;
using Rg.Plugins.Popup.Droid.Impl;
using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using XApplication = Xamarin.Forms.Application;

[assembly: Dependency(typeof(PopupNavigationDroid))]
namespace Rg.Plugins.Popup.Droid.Impl
{
    [Preserve(AllMembers = true)]
    class PopupNavigationDroid : IPopupNavigation
    {
        private FrameLayout _decoreView
        {
            get { return (FrameLayout)((Activity)Forms.Context).Window.DecorView; }
        }

        public void AddPopup(PopupPage page)
        {
            var decoreView = _decoreView;

            page.Parent = XApplication.Current.MainPage;

            var renderer = page.GetOrCreateRenderer();

            page.Layout(DependencyService.Get<IScreenHelper>().ScreenSize);

            decoreView.AddView(renderer.ViewGroup);
            UpdateListeners(true);
        }

        public void RemovePopup(PopupPage page)
        {
            var renderer = page.GetOrCreateRenderer();
            if (renderer != null)
            {
                _decoreView.RemoveView(renderer.ViewGroup);
                UpdateListeners(false);
                //renderer.Dispose();
            }
        }

        #region Listeners

        private void UpdateListeners(bool isAdd)
        {
            var isPrevent = PopupNavigation.PopupStack.Count > 0 || isAdd;

            if (Forms.Context is FormsApplicationActivity)
            {
                var handleBackPressed = (FormsApplicationActivity.BackButtonPressedEventHandler)PlatformHelper.GetHandleBackPressed<FormsApplicationActivity.BackButtonPressedEventHandler>();
                FormsApplicationActivity.BackPressed -= handleBackPressed;
                FormsApplicationActivity.BackPressed -= OnBackPressed;
                if (!isPrevent)
                {
                    FormsApplicationActivity.BackPressed += handleBackPressed;
                }
                else
                {
                    FormsApplicationActivity.BackPressed += OnBackPressed;
                }

            }
            else if (Forms.Context is FormsAppCompatActivity)
            {
                var handleBackPressed = (FormsAppCompatActivity.BackButtonPressedEventHandler)PlatformHelper.GetHandleBackPressed<FormsAppCompatActivity.BackButtonPressedEventHandler>();
                FormsAppCompatActivity.BackPressed -= handleBackPressed;
                FormsAppCompatActivity.BackPressed -= OnBackPressed;
                if (!isPrevent)
                {
                    FormsAppCompatActivity.BackPressed += handleBackPressed;
                }
                else
                {
                    FormsAppCompatActivity.BackPressed += OnBackPressed;
                }
            }
        }

        private bool OnBackPressed(object sender, EventArgs e)
        {
            if (PopupNavigation.PopupStack.Count > 0)
            {
                var isPreventClose = PopupNavigation.PopupStack.Last().SendBackButtonPressed();
                if (!isPreventClose)
                {
                    Device.BeginInvokeOnMainThread(async () =>
                    {
                        await PopupNavigation.PopAsync();
                    });
                }
            }
            return true;
        }

        #endregion
    }
}