using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace TwitchAlarmAndroid
{
    [Activity(Label = "HelpActivity")]
    public class HelpActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            var view = new TextView(this);
            view.Text = GetString(Resource.String.help_description);
            SetContentView(view);
        }
    }
}