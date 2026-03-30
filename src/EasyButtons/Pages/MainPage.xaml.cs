using EasyButtons.ViewModels;

namespace EasyButtons.Pages;

public partial class MainPage : ContentPage
{
    private MainViewModel ViewModel => (MainViewModel)BindingContext;

    public MainPage(MainViewModel vm)
    {
        BindingContext = vm;
        InitializeComponent();
#if DEBUG
        DebugBanner.IsVisible = true;
#endif
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        ViewModel.LoadCommand.Execute(null);
    }
}
