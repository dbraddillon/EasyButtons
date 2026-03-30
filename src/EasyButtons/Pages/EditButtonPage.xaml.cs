using EasyButtons.ViewModels;

namespace EasyButtons.Pages;

public partial class EditButtonPage : ContentPage
{
    public EditButtonPage(EditButtonViewModel vm)
    {
        BindingContext = vm;
        InitializeComponent();
    }
}
