using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Example.ViewModels;

public partial class MenuViewModel : ViewModelBase
{
    public MenuViewModel()
    {
        ActivateCommand = new RelayCommand(OnActivate);
    }

    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private string _icon = string.Empty;

    [ObservableProperty]
    private ViewModelBase? _viewModel;

    private void OnActivate()
    {
       WeakReferenceMessenger.Default.Send(this, "JumpTo");
    }

    public ICommand ActivateCommand { get; set; }
}
