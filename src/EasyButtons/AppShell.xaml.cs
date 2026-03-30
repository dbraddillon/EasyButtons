using EasyButtons.Pages;

namespace EasyButtons;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        Routing.RegisterRoute(nameof(EditButtonPage), typeof(EditButtonPage));
    }
}
