using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

public partial class ViewModel : ObservableObject
{
    [ObservableProperty]
    public partial int Number { get; set; }

    [RelayCommand]
    private void Increment()
    {
        Number++;
    }
}
