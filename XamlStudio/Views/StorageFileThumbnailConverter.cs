// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CommunityToolkit.WinUI;
using System;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media.Imaging;

namespace XamlStudio.Views;

public class StorageFileThumbnailConverter : IValueConverter
{
    DispatcherQueue _queue;
    public StorageFileThumbnailConverter()
    {
        _queue = DispatcherQueue.GetForCurrentThread();
    }

    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is StorageFile file)
        {
            var result = new BitmapImage();
            Task t = new Task(async () =>
            {
                if (file.IsAvailable)
                {
                    var thumbnail = await file.GetScaledImageAsThumbnailAsync(Windows.Storage.FileProperties.ThumbnailMode.PicturesView, 128, Windows.Storage.FileProperties.ThumbnailOptions.ResizeThumbnail);

                    if (thumbnail == null)
                    {
                        thumbnail = await file.GetThumbnailAsync(Windows.Storage.FileProperties.ThumbnailMode.PicturesView, 128, Windows.Storage.FileProperties.ThumbnailOptions.ResizeThumbnail);
                    }

                    if (thumbnail != null)
                    {
                        InMemoryRandomAccessStream randomAccessStream = new InMemoryRandomAccessStream();
                        await RandomAccessStream.CopyAsync(thumbnail, randomAccessStream);
                        randomAccessStream.Seek(0);
                        await _queue.EnqueueAsync(async () =>
                        {
                            await result.SetSourceAsync(randomAccessStream);
                        });
                    }
                    else
                    {
                        using (var stream = await file.OpenAsync(FileAccessMode.Read))
                        {
                            await _queue.EnqueueAsync(async () =>
                            {
                                await result.SetSourceAsync(stream);
                            });
                        }
                    }
                }
            });
            t.Start();
            return result;
        }

        return DependencyProperty.UnsetValue;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
