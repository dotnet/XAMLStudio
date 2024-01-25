// ******************************************************************
// Copyright (c) Microsoft. All rights reserved.
// This code is licensed under the MIT License (MIT).
// THE CODE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
// IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
// DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
// TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH
// THE CODE OR THE USE OR OTHER DEALINGS IN THE CODE.
// ******************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.WinUI.Extensions.Future;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;

namespace CommunityToolkit.WinUI.Extensions.Pivot.Future
{
    /// <summary>
    /// Helper to retrieve the CloseCommandButton Attached Property from a Pivot for the PivotHeaderItem Style Templates.
    /// </summary>
    [Bindable]
    public class GetPivotCloseButtonCommandConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var pivot = (value as DependencyObject)?.FindAscendant<Windows.UI.Xaml.Controls.Pivot>();

            if (pivot != null)
            {
                return PivotExtensions.GetCloseButtonCommand(pivot);
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
