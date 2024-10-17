using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System.Collections.Generic;
using ESMetadata.Settings;


namespace ESMetadata.Models.Provider
{
    public partial class  ESMetadataProvider : OnDemandMetadataProvider
    {
        private readonly MetadataRequestOptions Options;
        private readonly IPlayniteAPI PlayniteApi;
        private readonly ESMetadataSettings Settings;
        private readonly ESMetadataLibrary esLibrary;

        public override List<MetadataField> AvailableFields
        {
            get => esLibrary.Game.GetAvailableFields();
        }

        public ESMetadataProvider(MetadataRequestOptions options, ESMetadata plugin)
        {
            Options = options;
            PlayniteApi = ESMetadata.PlayniteApi;
            Settings = plugin.GetSettings();
            esLibrary = new ESMetadataLibrary(plugin, options.GameData, options.IsBackgroundDownload);
        }

        public override string GetName(GetMetadataFieldArgs args)
        => esLibrary.Game.GetField(MetadataField.Name);

        public override string GetDescription(GetMetadataFieldArgs args)
        => esLibrary.Game.GetField(MetadataField.Description);

        public override MetadataFile GetCoverImage(GetMetadataFieldArgs args)
        => ToMetadataFile(
            MetadataField.CoverImage.ToString(),
            esLibrary.Game.GetMultiField(MetadataField.CoverImage),
            Settings.GetCoverImageMaxSize().Width,
            Settings.GetCoverImageMaxSize().Height
        );

        public override MetadataFile GetIcon(GetMetadataFieldArgs args)
        => ToMetadataFile(
            MetadataField.Icon.ToString(),
            esLibrary.Game.GetMultiField(MetadataField.Icon),
            Settings.GetIconMaxSize().Width,
            Settings.GetIconMaxSize().Height
        );

        public override MetadataFile GetBackgroundImage(GetMetadataFieldArgs args)
        => ToMetadataFile(
            MetadataField.BackgroundImage.ToString(),
            esLibrary.Game.GetMultiField(MetadataField.BackgroundImage),
            Settings.GetBackgroundImageMaxSize().Width,
            Settings.GetBackgroundImageMaxSize().Height
        );

        public override IEnumerable<MetadataProperty> GetGenres(GetMetadataFieldArgs args)
        => ToMetadataProperty(esLibrary.Game.GetField(MetadataField.Genres), '/');

        public override IEnumerable<MetadataProperty> GetRegions(GetMetadataFieldArgs args)
        => ToMetadataRegions(esLibrary.Game.GetField(MetadataField.Region), ',');

        public override ReleaseDate? GetReleaseDate(GetMetadataFieldArgs args)
        => ToMetadataReleaseDate(esLibrary.Game.GetField(MetadataField.ReleaseDate));

        public override int? GetCriticScore(GetMetadataFieldArgs args)
        => ToMetadataScore(esLibrary.Game.GetField(MetadataField.CriticScore));

        public override int? GetCommunityScore(GetMetadataFieldArgs args)
        => ToMetadataScore(esLibrary.Game.GetField(MetadataField.CommunityScore));

        public override IEnumerable<MetadataProperty> GetDevelopers(GetMetadataFieldArgs args)
        => ToMetadataProperty(esLibrary.Game.GetField(MetadataField.Developers), '/');

        public override IEnumerable<MetadataProperty> GetPublishers(GetMetadataFieldArgs args)
        => ToMetadataProperty(esLibrary.Game.GetField(MetadataField.Publishers), '/');

        public override IEnumerable<Link> GetLinks(GetMetadataFieldArgs args)
        => GetLinks();

        public override IEnumerable<MetadataProperty> GetTags(GetMetadataFieldArgs args)
        => GetTags();

        public override void Dispose(){}

    }
}