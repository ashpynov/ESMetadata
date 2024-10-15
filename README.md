# EmulationStation Metadata source For Playnite

EmulationStation is s graphical and themeable emulator front-end. It is well-known in retrogaming, and many retrogame emulators setup based on it, e.g. Retrobat, Batocera, RecallBox, RetroPie.

So if you are in Retrogaming word - most probably you already have your ROM collection, with your favourite covers and gameplay video. May be you spend many time to collect and polish it.

This Metadata source is aimed to help you to import your collection to Playnite.

## Functions and Limitation

The set of game info media do not the same between Playnite and EmulationStation. For example here is no native support of Video or Logo. But huge set of Playnite Extension will help you. E.g. ExtraMetadata extension are supported by many themes and will help you to add Video and logos to you game.

But there in no API to deal with it. So This Source will use a little tricks:
- Some information like 'Favorite' mark or In-game statistic are imported if 'Tags' is requested from source.
- Additional media resurces like Video, logo, bezel, fanart, manual may are imported as 'Links'

To be able be used as media for ExtraMetadata extension - files may be copied to Extrametadata folder. Also you may setup:
- Copy some media from you collection,
- Just save link to media from your collection
- If file was copied, you may keep link to original or replace it to link to copied media.

For sure you may choose priority of sources for Icon, cover, or background and downscale sizes.

Name of game does not imported on automation metadata load (Playnite implementation)

## Game record choosing and Fuzzy match

By default record in collection look up by ROM path. If you collection is mature enough and organized - it should be enought. Colection is searching from closest one to ROM location. (Common structure is => roms/platform/rom_files + gamelist.xml )

But if your collection same as mine: ROM name dont match, some copy of ROM with different names, some added rom just-to-play or modified. Without description... Fuzzy search will try to find most somilar Best filled game in collection.

The mark of 'not filled game' is absence of description field. So if field is absence - it will try to find next best match with description.

What is 'best match'? it will try to compare ROM filename and name with Game name, during comparasion it may ignote articles, underscores and simbols. And allow some difference between. So sometime it will give errors. Cause `eaarth worm jim 2" is real close to 'earthworm jim'.

But in any case - if you have several version of game, you may choose to suggest also images from other similar games.




