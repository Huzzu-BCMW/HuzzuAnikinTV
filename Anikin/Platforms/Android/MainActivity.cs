using System;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Content.Res;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using AndroidX.Core.View;
using Anikin.Utils;
using Microsoft.Maui;
using Microsoft.Maui.ApplicationModel;
using static Android.Views.ViewGroup;

namespace Anikin
{
    [IntentFilter(
        actions: new[] { Intent.ActionMain },
        Categories = new[] { Intent.CategoryLeanbackLauncher }
    )]
    [Activity(
        Theme = "@style/Maui.SplashTheme",
        MainLauncher = true,
        SupportsPictureInPicture = true,
        ClearTaskOnLaunch = true,
        ConfigurationChanges = ConfigChanges.ScreenSize
            | ConfigChanges.Orientation
            | ConfigChanges.UiMode
            | ConfigChanges.ScreenLayout
            | ConfigChanges.SmallestScreenSize
            | ConfigChanges.Density
    )]
    public class MainActivity
        : MauiAppCompatActivity,
          IOnApplyWindowInsetsListener,
          View.IOnTouchListener
    {
        public AndroidStoragePermission? AndroidStoragePermission { get; set; }
        public PackageInstallPermission? PackageInstallPermission { get; set; }
        public PlatformMediaController? MediaElementController { get; set; }

        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            Window.DecorView.ViewTreeObserver.GlobalFocusChange += FocusChanged;

            WindowCompat.SetDecorFitsSystemWindows(Window, false);

            var view1 = FindViewById(Android.Resource.Id.Content);

            ViewCompat.SetOnApplyWindowInsetsListener(view1, this);

            // Other parts of your existing code...

            // Set a custom logo for Android TV
            if (IsAndroidTV())
            {
                ActionBar?.SetLogo(Resource.Drawable.banner); // Assuming the logo is named banner.png
                ActionBar?.SetDisplayUseLogoEnabled(true);
                ActionBar?.SetDisplayShowHomeEnabled(true);
            }
        }

        void FocusChanged(object? sender, ViewTreeObserver.GlobalFocusChangeEventArgs e)
        {
            e.OldFocus?.SetOnTouchListener(null);
            e.NewFocus?.SetOnTouchListener(this);

            if (e.NewFocus is null && e.OldFocus is not null)
            {
                var imm = InputMethodManager.FromContext(this);

                var wt = e.OldFocus.WindowToken;

                if (imm is null || wt is null)
                    return;

                imm.HideSoftInputFromWindow(wt, HideSoftInputFlags.None);
            }
        }

        public override bool DispatchTouchEvent(MotionEvent? ev)
        {
            var dispatch = base.DispatchTouchEvent(ev);

            if (ev?.Action == MotionEventActions.Down && CurrentFocus is not null)
            {
                var outRect = new Rect();
                CurrentFocus.GetGlobalVisibleRect(outRect);

                if (!outRect.Contains((int)ev.RawX, (int)ev.RawY))
                {
                    KeepFocus = false;
                }
                else
                {
                    KeepFocus = true;
                }

                if (!KeepFocus)
                    CurrentFocus.ClearFocus();
                KeepFocus = false;
            }

            return dispatch;
        }

        public static bool KeepFocus { get; set; }

        bool OnTouch(View? v, MotionEvent? e)
        {
            if (e?.Action == MotionEventActions.Down && CurrentFocus == v)
                KeepFocus = true;

            return v?.OnTouchEvent(e) == true;
        }

        bool View.IOnTouchListener.OnTouch(View? v, MotionEvent? e) => OnTouch(v, e);

        public void HideStatusBar()
        {
            Window.AddFlags(Android.Views.WindowManagerFlags.Fullscreen);
        }

        public WindowInsetsCompat OnApplyWindowInsets(View v, WindowInsetsCompat insets1)
        {
            var insets = insets1.GetInsets(WindowInsetsCompat.Type.SystemBars());
            var mlp = (MarginLayoutParams)v.LayoutParameters;
            v.LayoutParameters = mlp;

            return WindowInsetsCompat.Consumed;
        }

        // Other methods...

        private bool IsAndroidTV()
        {
            UiModeManager uiModeManager = (UiModeManager)GetSystemService(UiModeService);
            return uiModeManager?.CurrentModeType == UiMode.TypeTelevision;
        }

        protected override void OnActivityResult(
            int requestCode,
            [GeneratedEnum] Result resultCode,
            Intent? data
        )
        {
            base.OnActivityResult(requestCode, resultCode, data);

            AndroidStoragePermission?.OnActivityResult(requestCode, resultCode, data);
            PackageInstallPermission?.OnActivityResult(requestCode, resultCode, data);
        }

        public override void OnRequestPermissionsResult(
            int requestCode,
            string[] permissions,
            [GeneratedEnum] Permission[] grantResults
        )
        {
            Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            AndroidStoragePermission?.OnRequestPermissionsResult(
                requestCode,
                permissions,
                grantResults
            );
        }

        protected override void OnResume()
        {
            base.OnResume();
            MediaElementController?.OnResume();
        }

        protected override void OnPause()
        {
            base.OnPause();
            MediaElementController?.MediaElement.Pause();
        }

        #pragma warning disable CS0618, CS0672, CA1422
        public override void OnPictureInPictureModeChanged(bool isInPictureInPictureMode)
        {
            MediaElementController?.OnPiPChanged(isInPictureInPictureMode);
            base.OnPictureInPictureModeChanged(isInPictureInPictureMode);
        }

        public override void OnPictureInPictureUiStateChanged(PictureInPictureUiState pipState)
        {
            MediaElementController?.OnPiPChanged(IsInPictureInPictureMode);
            base.OnPictureInPictureUiStateChanged(pipState);
        }

        public override void OnPictureInPictureModeChanged(
            bool isInPictureInPictureMode,
            Configuration? newConfig
        )
        {
            MediaElementController?.OnPiPChanged(isInPictureInPictureMode);
            base.OnPictureInPictureModeChanged(isInPictureInPictureMode, newConfig);
        }
        #pragma warning restore CS0618, CS0672, CA1422
    }
}
