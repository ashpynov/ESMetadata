using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Playnite.SDK;
using ESMetadata.ViewModels;
using System;
using System.Globalization;
using System.Windows.Data;
using ESMetadata.Models;
using System.IO;
using ESMetadata.Tools;
using System.Windows.Threading;
using System.Windows.Input;
using ESMetadata.Extensions;


namespace ESMetadata.Views
{

    public partial class VideoSelectionView : UserControl
    {
        private readonly DispatcherTimer debounceTimer;

        public VideoSelectionView()
        {
            InitializeComponent();
            debounceTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(400)
            };
            debounceTimer.Tick += DebounceTimer_Tick;
        }

        private void DebounceTimer_Tick(object sender, EventArgs e)
        {
            debounceTimer.Stop();
            // Logic to play the media
            var focusedItem = Keyboard.FocusedElement as DependencyObject;
            PlayMedia(focusedItem, true);
        }

        private void Item_GotFocus(object sender, RoutedEventArgs e)
        {
            debounceTimer.Stop();
            debounceTimer.Start();
        }

        private void Item_LostFocus(object sender, RoutedEventArgs e)
        {
            PlayMedia(sender, false);
        }
        private void PlayMedia(object sender, bool play)
        {
            if (VisualTreeHelperExtensions.FindChild<MediaElement>(sender as DependencyObject) is MediaElement mediaElement)
            {
                if ( play )
                {
                    mediaElement.Position = TimeSpan.FromSeconds(0);
                    mediaElement.LoadedBehavior = MediaState.Play;
                }
                else
                {
                    mediaElement.LoadedBehavior = MediaState.Pause;
                    mediaElement.Position = TimeSpan.FromSeconds(1);
                }
            }
        }
        private void MediaElement_MediaOpened(object sender, RoutedEventArgs e)
        {
            if (sender is MediaElement mediaElement)
            {
                mediaElement.LoadedBehavior = MediaState.Pause;
                mediaElement.Position = TimeSpan.FromSeconds(1);
            }
        }
        private void MediaElement_MediaEnded(object sender, RoutedEventArgs e)
        {
            if (sender is MediaElement mediaElement)
            {
                mediaElement.Position = TimeSpan.FromSeconds(0);
                mediaElement.LoadedBehavior = MediaState.Play;
            }
        }
    }
}