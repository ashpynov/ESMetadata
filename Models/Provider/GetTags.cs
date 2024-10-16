using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.Globalization;
using ESMetadata.Extensions;
using ESMetadata.Models.Gamelist;


namespace ESMetadata.Models.Provider
{
    public partial class ESMetadataProvider : OnDemandMetadataProvider
    {
        private IEnumerable<MetadataProperty> GetTags()
        {
            if (Settings.ImportFavorite
             && esLibrary.Game.GetField(MetadataField.Tags, GamelistField.Favorite).Equal("true")
             && !Options.GameData.Favorite)
            {
                Options.GameData.Favorite = true;
            }

            if (Settings.ImportGameStatistic)
            {
                string PlayCount = esLibrary.Game.GetField(MetadataField.Tags, GamelistField.PlayCount);
                if (!PlayCount.IsNullOrEmpty()
                    && Options.GameData.PlayCount == 0
                    && ulong.TryParse(PlayCount, out ulong playCount))
                {
                    Options.GameData.PlayCount = playCount;
                }

                string LastPlayed = esLibrary.Game.GetField(MetadataField.Tags, GamelistField.LastPlayed);
                if (!LastPlayed.IsNullOrEmpty()
                    && Options.GameData.LastActivity is null
                    && DateTime.TryParseExact(LastPlayed, "yyyyMMddTHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date))
                {
                    Options.GameData.LastActivity = date;
                }

                string GameTime = esLibrary.Game.GetField(MetadataField.Tags, GamelistField.GameTime);
                if (!GameTime.IsNullOrEmpty()
                    && Options.GameData.Playtime == 0
                    && ulong.TryParse(GameTime, out ulong gameTime))
                {
                    Options.GameData.Playtime = gameTime;
                }
            }

            return default;
        }
    }
}