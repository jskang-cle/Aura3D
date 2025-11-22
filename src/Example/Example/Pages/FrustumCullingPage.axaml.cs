using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Example.Pages;

public partial class FrustumCullingPage : UserControl
{
    public FrustumCullingPage()
    {
        InitializeComponent();
    }

    private void Aura3DView_SceneInitialized(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
    }
}