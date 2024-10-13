using Playnite.SDK;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;


using ESMetadata.Models;
using ESMetadata.Views;
using ESMetadata.Settings;
using Playnite.SDK.Models;
using System.Linq;
using ESMetadata.Common;

namespace ESMetadata.ViewModels
{
    class DialogCanceledException : Exception
    {}

    class LinkSelectionViewModel : ObservableObject
    {
        private IDialogsFactory Dialogs => ESMetadata.PlayniteApi.Dialogs;

        private List<Link> selectedResult = default;
        public List<Link> SelectedResult
        {
            get => selectedResult;
            set
            {
                selectedResult = value;
                OnPropertyChanged();
            }
        }

        public List<LinkOption> LinkOptions { get; set; }

        private ESMetadataSettings settings;
        private Window window;

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
            });
        }

        private static readonly ILogger logger = LogManager.GetLogger();
        private string title;

        private string CreateOriginalPath( string gamePath, string type )
        {
            string extension = type.Equal(LinkField.VideoTrailer.ToString()) ? "mp4" : "png";
            string path = Path.Combine(gamePath, $"{type}.{extension}");
            return File.Exists(path) ? path : null;
        }

        public LinkSelectionViewModel(
            string title,
            string gamePath,
            List<Link> data)
        {
            this.title = title;

            LinkOptions = data
                .GroupBy(item => item.Name)
                .Select(group =>
                    new LinkOption(
                        group.Key,
                        CreateOriginalPath(gamePath, group.Key),
                        group.Select(item => item.Url).ToList()
                ))
                .ToList();
        }

        static public List<Link> ChooseMedia(string title, string gamePath, List<Link> data)
        {
            LinkSelectionViewModel selection = new LinkSelectionViewModel(title, gamePath, data);
            bool result = Application.Current.Dispatcher.Invoke(() => selection.ShowDialog());
            return result ? selection.SelectedResult : new List<Link>();
        }

        private bool ShowDialog()
        {
            window = ESMetadata.PlayniteApi.Dialogs.CreateWindow(new WindowCreationOptions());
            window.Height = 600;
            window.Width = 800;
            window.Title = title;
            window.Content = new LinkSelectionView();
            window.DataContext = this;
            window.Owner = ESMetadata.PlayniteApi.Dialogs.GetCurrentAppWindow();
            window.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            return window.ShowDialog() ?? false;
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
    }
}
