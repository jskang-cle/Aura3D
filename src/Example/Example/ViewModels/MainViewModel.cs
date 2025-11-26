using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace Example.ViewModels
{
    public partial class MainViewViewModel : ViewModelBase
    {
        public MainViewViewModel() 
        {

            WeakReferenceMessenger.Default.Register<MainViewViewModel, MenuViewModel, string>(this, "JumpTo", OnNavigation);
            _menus = [
                new MenuViewModel {
                    Title = "Base Geometries",
                    ViewModel = new BaseGeometriesViewModel()
                },
                new MenuViewModel {
                    Title = "Load Gltf Model",
                    ViewModel = new GltfModelViewModel()
                },
                new MenuViewModel {
                    Title = "Frustum Culling ",
                    ViewModel = new FrustumCullingViewModel()
                },
                new MenuViewModel {
                    Title = "Animation",
                    ViewModel = new AnimationViewModel()
                }
            ];
            OnNavigation(this, _menus.First());
        }


        private void OnNavigation(MainViewViewModel vm, MenuViewModel menuItem)
        {
            Content =  menuItem.ViewModel;

        }


        [ObservableProperty] 
        private object? _content;

        [ObservableProperty]
        private ObservableCollection<MenuViewModel> _menus;

        [ObservableProperty]
        private string _greeting = "Welcome to Avalonia!";
    }
}
