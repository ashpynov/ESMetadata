using System.Collections.Generic;
using System.Windows;
using ESMetadata.Models;
using ESMetadata.Views;
using ESMetadata.Extensions;
using Playnite.SDK;


namespace ESMetadata.ViewModels
{
    public class VideoSelectionViewModel : ObservableObject
    {
        public string WindowTitle { get; set; }
        public double ItemWidth { get; set; } = 240;
        public double ItemHeight { get; set; } = 180;


        private Window window;

        private List<VideoFileOption> videos = new List<VideoFileOption>();
        public List<VideoFileOption> Videos
        {
            get
            {
                return videos;
            }

            set
            {
                videos = value;
                OnPropertyChanged();
            }
        }

        private VideoFileOption selectedVideo;
        public VideoFileOption SelectedVideo
        {
            get => selectedVideo;
            set
            {
                selectedVideo = value;
                OnPropertyChanged();
            }
        }

        public RelayCommand<object> CloseCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                CloseView(false);
            });
        }

        public RelayCommand<object> ConfirmCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                ConfirmDialog();
            }, (a) => SelectedVideo != null);
        }

        public RelayCommand<object> ItemDoubleClickCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                ConfirmDialog();
            });
        }

        private static readonly ILogger logger = LogManager.GetLogger();

        public VideoSelectionViewModel(
            List<VideoFileOption> videos,
            string caption = null,
            double itemWidth = 240,
            double itemHeight = 180)
        {
            Videos = videos;
            ItemWidth = itemWidth;
            ItemHeight = itemHeight;

            window = ESMetadata.PlayniteApi.Dialogs.CreateWindow(new WindowCreationOptions());
            window.Title = caption.IsNullOrEmpty() ? ResourceProvider.GetString("LOCSelectVideoTitle") : caption;
            window.Content = new VideoSelectionView();
            window.DataContext = this;
            window.Owner = ESMetadata.PlayniteApi.Dialogs.GetCurrentAppWindow();
            window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            window.Height = 600;
            window.Width = 830;

        }

        private bool? OpenView()
        {
            return window.ShowDialog();
        }

        public void CloseView(bool? result)
        {
            window.DialogResult = result;
            window.Close();
        }

        public void Close()
        {
        }

        public void ConfirmDialog()
        {
            CloseView(true);
        }

        static public VideoFileOption ChooseVideoFile(List<VideoFileOption> files, string caption = null, double itemWidth = 240, double itemHeight = 180)
        {
            return  Application.Current.Dispatcher.Invoke(() =>
            {
                VideoSelectionViewModel model = new VideoSelectionViewModel(files, caption, itemWidth, itemHeight);
                if (model.OpenView() == true)
                {
                    return model.SelectedVideo;
                }
                else
                {
                    return null;
                }
            });
        }
    }
}
