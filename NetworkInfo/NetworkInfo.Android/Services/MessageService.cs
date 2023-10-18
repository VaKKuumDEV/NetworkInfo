using Android.App;
using Android.Widget;
using NetworkInfo.Droid.Services;
using NetworkInfo.Services;

[assembly: Xamarin.Forms.Dependency(typeof(MessageAndroid))]
namespace NetworkInfo.Droid.Services
{
    public class MessageAndroid : IMessage
    {
        public void LongAlert(string message)
        {
            Toast.MakeText(Application.Context, message, ToastLength.Long).Show();
        }

        public void ShortAlert(string message)
        {
            Toast.MakeText(Application.Context, message, ToastLength.Short).Show();
        }
    }
}