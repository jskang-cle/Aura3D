using Avalonia.Controls;
using CommunityToolkit.Mvvm.Messaging;
using Example.ViewModels;
namespace Example.Views;

public partial class MainView : UserControl
{


    public MainView()
    {
        InitializeComponent();

        
    }
    private void UserControl_ActualThemeVariantChanged(object? sender, System.EventArgs e)
    {
    }
}