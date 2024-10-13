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
using ESMetadata.Common;


namespace ESMetadata.Views
{

    public class MediaTypeSelector : DataTemplateSelector
    {
        public DataTemplate ImageTemplate { get; set; }
        public DataTemplate VideoTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if ( item is string path)
            {
                path = Path.GetExtension(path).TrimStart('.');
                if (path.Equal("mp4"))
                {
                    return VideoTemplate;
                }
                else
                {
                    return ImageTemplate;
                }
            }
            return base.SelectTemplate(item, container);
        }
    }
    public partial class LinkSelectionView : UserControl
    {
        public LinkSelectionView()
        {
            InitializeComponent();
        }
        private void RadioButton_GotFocus(object sender, RoutedEventArgs e)
        {
            PlayMedia(sender, true);
        }

        private void RadioButton_LostFocus(object sender, RoutedEventArgs e)
        {
            PlayMedia(sender, false);
        }
        private void PlayMedia(object sender, bool play)
        {
            MediaElement mediaElement = VisualTreeHelperExtensions.FindChild<MediaElement>(sender as RadioButton);

            if (mediaElement != null)
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
            var mediaElement = sender as MediaElement;
            if (mediaElement != null)
            {
                mediaElement.LoadedBehavior = MediaState.Pause;
                mediaElement.Position = TimeSpan.FromSeconds(1);
            }
        }
        private void MediaElement_MediaEnded(object sender, RoutedEventArgs e)
        {
            var mediaElement = sender as MediaElement;
            if (mediaElement != null)
            {
                mediaElement.Position = TimeSpan.FromSeconds(0);
                mediaElement.LoadedBehavior = MediaState.Play;
            }
        }
    }
}
