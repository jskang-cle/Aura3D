using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Example.Pages;
using Example.ViewModels;
namespace Example;

public class ViewLocator : IDataTemplate
{
    public Control? Build(object? data)
    {
        return data switch
        {
            BaseGeometriesViewModel baseGeometriesViewModel => new BaseGeometriesPage(),
            GltfModelViewModel gltfModelViewModel => new GltfModelPage(),
            _ => new TextBlock() { Text = "NotFound" }
        };
    }

    public bool Match(object? data)
    {
        return data is ViewModelBase;
    }
}
