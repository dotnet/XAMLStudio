using CommunityToolkit.WinUI;
using CommunityToolkit.WinUI.Helpers;
using System.Collections.Generic;
using Windows.UI.Xaml;
using XamlStudio.Toolkit.Models;

namespace XamlStudio.Toolkit.Helpers;

public class VisualStateWatcher
{
    public Dictionary<string, VisualStateInfo[]> VisualStates { get; private set; }

    public VisualStateWatcher(FrameworkElement element)
    {
        Dictionary<string, VisualStateInfo[]> visualStateGroups = new();

        // Get child of the control in visual tree as that'll contain the VSM
        // and will be more expected when manipulating a control itself (as that's the element we manipulate).
        var child = element.FindDescendant<FrameworkElement>();
        if (child is not null)
        {
            int gindex = 0;
            var groups = VisualStateManager.GetVisualStateGroups(child);
            foreach (var group in groups)
            {
                var gname = group.Name;
                if (string.IsNullOrEmpty(gname))
                {
                    gname = $"- Unnamed {gindex++} -";
                }

                List<VisualStateInfo> visualStates = new();
                foreach (var state in group.States)
                {
                    visualStates.Add(new VisualStateInfo(state.Name, gname, state == group.CurrentState));
                }
                if (!visualStateGroups.ContainsKey(gname))
                {
                    visualStateGroups.Add(gname, visualStates.ToArray());
                }
                else
                {
                    // TODO: Warning of duplicate group name?
                    visualStateGroups.Add(gname + $" (Duplicate {gindex++})", visualStates.ToArray());
                }

                // Register for state changes
                var weakPropertyChangedListener = new WeakEventListener<VisualStateWatcher, object, VisualStateChangedEventArgs>(this)
                {
                    OnEventAction = (instance, source, eventArgs) => instance.VisualStateGroup_StateChanged(gname, eventArgs), // Note: Capture 'gname' as Local Reference Only! (As we don't have reference to group in event method callback...)
                    OnDetachAction = (weakEventListener) => group.CurrentStateChanged -= weakEventListener.OnEvent // Use Local References Only
                };
                group.CurrentStateChanged += weakPropertyChangedListener.OnEvent;
            }

            VisualStates = visualStateGroups;
        }
    }

    private void VisualStateGroup_StateChanged(string groupName, VisualStateChangedEventArgs args)
    {
        // Update our current state in the model
        if (VisualStates.TryGetValue(groupName, out var states))
        {
            foreach (var state in states)
            {
                state.IsCurrent = state.Name == args.NewState.Name;
            }
        }
    }
}
