using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls.Primitives;

namespace CommunityToolkit.WinUI.Controls.Future
{
    public class ManualToggleButton : ToggleButton
    {
        protected override void OnToggle()
        {
            // Do Nothing so we don't change the state of IsChecked on Activation
            // Then we can control that with MVVM practices and such.
        }
    }
}
