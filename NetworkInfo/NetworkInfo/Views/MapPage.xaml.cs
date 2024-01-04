using NetworkInfo.ViewModels;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace NetworkInfo.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MapPage : ContentPage
    {
        public MapViewModel MapViewModel { get; }

        public MapPage()
        {
            InitializeComponent();

            BindingContext = MapViewModel = new MapViewModel(ref NetworksPicker);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            MapViewModel.OnAppearing();
        }
    }
}