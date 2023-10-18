using NetworkInfo.ViewModels;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace NetworkInfo.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class NetworkInfoPage : ContentPage
    {
        public NetworkInfoViewModel NetworkInfoViewModel { get; }

        public NetworkInfoPage()
        {
            InitializeComponent();
            BindingContext = NetworkInfoViewModel = new NetworkInfoViewModel();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            NetworkInfoViewModel.OnAppearing();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            NetworkInfoViewModel.OnDisappearing();
        }
    }
}