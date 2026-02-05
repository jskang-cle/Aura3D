using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Example.ViewModels;

public partial class BlendSpaceViewModel : ViewModelBase
{
    [ObservableProperty]
    private double _x;

    [ObservableProperty]
    private double _y;
}
