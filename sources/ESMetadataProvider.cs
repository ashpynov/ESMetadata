using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESMetadata
{
    public class ESMetadataProvider : OnDemandMetadataProvider
    {
        private readonly MetadataRequestOptions options;
        private readonly ESMetadata plugin;

        public override List<MetadataField> AvailableFields => throw new NotImplementedException();

        public ESMetadataProvider(MetadataRequestOptions options, ESMetadata plugin)
        {
            this.options = options;
            this.plugin = plugin;
        }

        // Override additional methods based on supported metadata fields.
        public override string GetDescription(GetMetadataFieldArgs args)
        {
            return options.GameData.Name + " description";
        }
    }
}