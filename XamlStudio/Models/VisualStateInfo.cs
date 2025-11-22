using CommunityToolkit.Mvvm.ComponentModel;

namespace XamlStudio.Models;

public partial class VisualStateInfo : ObservableObject
{

    [ObservableProperty]
    public partial bool IsCurrent { get; set; }

    public string Name { get; private set; }

    public string Group { get; private set; }

    public VisualStateInfo(string name, string group, bool isCurrent)
    {
        Name = name;
        Group = group;
        IsCurrent = isCurrent;
    }
}
