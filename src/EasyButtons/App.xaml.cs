using EasyButtons.Services;

namespace EasyButtons;

public partial class App : Application
{
    private readonly ProService _pro;

    public App(ProService pro)
    {
        _pro = pro;
        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
        => new Window(new AppShell());

    protected override void OnResume()
    {
        base.OnResume();
        // Re-verify Pro purchase whenever app comes to foreground
        _ = _pro.RefreshAsync();
    }
}
