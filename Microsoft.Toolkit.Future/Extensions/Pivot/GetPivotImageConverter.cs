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
using Microsoft.Toolkit.Uwp.UI.Extensions.Future;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;

namespace Microsoft.Toolkit.Uwp.UI.Extensions.Pivot.Future
{
    /// <summary>
    /// Helper to retrieve the Image Attached Property from a PivotItem for the PivotHeaderItem Style Templates.
    /// </summary>
    [Bindable]
    public class GetPivotImageConverter : GetPivotItemConverter
    {
        public override object Convert(object value, Type targetType, object parameter, string language)
        {
            if (base.Convert(value, targetType, parameter, language) is PivotItem pivotitem)
            {
                var source = PivotExtensions.GetImageSource(pivotitem);

                return source;
            }

            return null;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
