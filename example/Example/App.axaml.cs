using Aura3D.Core;
using Aura3D.Core.Nodes;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Avalonia.Platform;
using Example.ViewModels;
using Example.Views;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Example
{
    public partial class App : Application
    {
        public static Model? model;
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);

            Task.Run(() =>
            {

                using (var stream = AssetLoader.Open(new Uri($"avares://Example/Assets/Models/present_11_BACKED.glb")))
                {
                    var m = ModelLoader.LoadGlbModel(stream);

                    var item1 = m.Meshes.First(mesh => mesh.Name == "item1");
                    var item2 = m.Meshes.First(mesh => mesh.Name == "item2");
                    var item3 = m.Meshes.First(mesh => mesh.Name == "item3");
                    var item4 = m.Meshes.First(mesh => mesh.Name == "item4");
                    var item5 = m.Meshes.First(mesh => mesh.Name == "item5");
                    var item6 = m.Meshes.First(mesh => mesh.Name == "item6");

                    var item7List = m.Meshes.Where(mesh => mesh.Name == "item7").ToList();
                    var item7 = item7List.First();



                    m.RemoveChild(item2.Parent);
                    item2.Parent.RemoveChild(item2);
                    m.RemoveChild(item3.Parent);
                    item3.Parent.RemoveChild(item3);
                    m.RemoveChild(item4.Parent);
                    item4.Parent.RemoveChild(item4);
                    m.RemoveChild(item5.Parent);
                    item5.Parent.RemoveChild(item5);
                    m.RemoveChild(item6.Parent);
                    item6.Parent.RemoveChild(item6);
                    m.RemoveChild(item7.Parent);
                    item7.Parent.RemoveChild(item7);
                    item7List[1].Parent.RemoveChild(item7List[1]);
                    item7List[1].Name = "item7_2";
                    item7.AddChild(item7List[1]);


                    item1.AddChild(item2);
                    item2.AddChild(item3);
                    item3.AddChild(item4);
                    item4.AddChild(item5);
                    item5.AddChild(item6);
                    item6.AddChild(item7List[0]);

                    model = m;

                }
            });
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                // Avoid duplicate validations from both Avalonia and the CommunityToolkit. 
                // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
                DisableAvaloniaDataAnnotationValidation();
                desktop.MainWindow = new MainWindow
                {
                    DataContext = new MainViewViewModel()
                };
            }
            else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
            {
                singleViewPlatform.MainView = new MainView
                {
                    DataContext = new MainViewViewModel()
                };
            }

            base.OnFrameworkInitializationCompleted();
        }

        private void DisableAvaloniaDataAnnotationValidation()
        {
            // Get an array of plugins to remove
            var dataValidationPluginsToRemove =
                BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

            // remove each entry found
            foreach (var plugin in dataValidationPluginsToRemove)
            {
                BindingPlugins.DataValidators.Remove(plugin);
            }
        }
    }
}