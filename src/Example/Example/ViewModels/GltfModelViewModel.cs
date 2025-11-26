using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Example.ViewModels;

public partial class GltfModelViewModel : ViewModelBase
{
    [ObservableProperty]
    public double _scale = 1.0;

    [ObservableProperty]
    public double _yaw = 1.0;
}
