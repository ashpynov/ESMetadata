using Playnite.SDK;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using ESMetadata.ViewModels;
using ESMetadata.Extensions;

namespace ESMetadata.Models.Provider
{
    public partial class ESMetadataProvider : OnDemandMetadataProvider
    {
        private string ChooseImageFile(List<string> paths, string caption = null, int maxWidth = 240, int maxHeight = 180, string original = null)
        {
            if (paths.IsNullOrSingle() || (Options.IsBackgroundDownload && Settings.SelectAutomaticly))
                return paths.FirstOrDefault();

            List<string> distinct = DistinctPaths(paths, original);

            if (distinct.IsNullOrSingle())
                return distinct.FirstOrDefault();

            double itemWidth = 240;
            double itemHeight = 180;

            if (maxWidth != int.MaxValue && maxHeight != int.MaxValue)
            {
                double zoom = Math.Min(itemWidth / maxWidth, itemHeight / maxHeight);
                itemWidth = maxWidth * zoom;
                itemHeight = maxHeight * zoom;

            }
            string result = PlayniteApi.Dialogs.ChooseImageFile(
                distinct.Select(p => new ImageFileOption(p)).ToList(),
                caption, itemWidth, itemHeight)?.Path;

            return !original.IsNullOrEmpty() && result.Equal(original) ? null : result;
        }

        private string ChooseVideoFile(List<string> paths, string caption = null, string original = null)
        {
            if (paths.IsNullOrSingle() || (Options.IsBackgroundDownload && Settings.SelectAutomaticly))
                return paths.FirstOrDefault();

            List<string> distinct = DistinctPaths(paths, original);

            if (distinct.IsNullOrSingle())
                return distinct.FirstOrDefault();

            string result = VideoSelectionViewModel.ChooseVideoFile(
                distinct.Select(p => new VideoFileOption(p)).ToList(),
                caption, 320, 180)?.Path;

            return !original.IsNullOrEmpty() && result.Equal(original) ? null : result;

        }
    }
}