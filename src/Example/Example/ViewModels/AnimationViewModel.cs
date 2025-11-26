using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Example.ViewModels;

public partial class AnimationViewModel : ViewModelBase
{
    [ObservableProperty]
    private ObservableCollection<string> _animations = [];

    [ObservableProperty]
    private string _selectedAnimation = string.Empty;

    private double _speed = 1;
    public double Speed
    {
        get => _speed;
        set
        {

            OnPropertyChanging("Speed");
            OnPropertyChanging("SpeedString");
            _speed = value;
            OnPropertyChanged("SpeedString");
            OnPropertyChanged("Speed");
        }
    }

    public string SpeedString => Math.Round(_speed, 1).ToString();
}
