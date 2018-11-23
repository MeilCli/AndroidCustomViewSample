using Android.App;
using Android.Runtime;
using System;

namespace AndroidCustomViewSample
{
    [Application(Label = "@string/ApplicationName", Icon = "@mipmap/iconlauncher", RoundIcon = "@mipmap/iconlauncherround", Theme = "@style/AppTheme")]
    public class CustomViewSampleApplication : Application
    {
        public CustomViewSampleApplication()
        {
        }

        protected CustomViewSampleApplication(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }
    }
}