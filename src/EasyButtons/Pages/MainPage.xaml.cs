using EasyButtons.Services;
using EasyButtons.ViewModels;

namespace EasyButtons.Pages;

public partial class MainPage : ContentPage
{
    private MainViewModel ViewModel => (MainViewModel)BindingContext;

    public MainPage(MainViewModel vm, ProService pro)
    {
        BindingContext = vm;
        InitializeComponent();
#if DEBUG
        DebugBanner.IsVisible = true;
        DebugProButton.IsVisible = true;
        RefreshDebugProLabel(pro);
        DebugProButton.Clicked += (_, _) =>
        {
            pro.DebugTogglePro();
            RefreshDebugProLabel(pro);
            // Refresh limit hint visibility
            vm.NotifyProChanged();
        };
#endif
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        ViewModel.LoadCommand.Execute(null);
    }

#if DEBUG
    private void RefreshDebugProLabel(ProService pro) =>
        DebugProButton.Text = pro.DebugIsPro ? "⭐ Pro: ON" : "Pro: OFF";
#endif
}
