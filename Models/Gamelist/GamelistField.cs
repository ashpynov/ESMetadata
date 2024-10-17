namespace ESMetadata.Models.Gamelist
{

    public enum GamelistField
    {
        Name,
        Desc,
        Genre,
        Region,
        ReleaseDate,
        Rating,
        Developer,
        Publisher,
        Favorite,
        PlayCount,
        LastPlayed,
        GameTime,
        Kidgame,
        AdultGame,

        // ^ not_path above,
        // v paths below

        Path,
        Image,
        Thumbnail,
        Marquee,
        Fanart,
        Video,
        Bezel,
        Manual,
        Boxback,
        Box,
        Magazine,
        Map,
        TitleShot
    };
}

/*
		<path>./Dizzy V - Spellbound Dizzy.z80.zip</path>
		<name>Dizzy V - Spellbound Dizzy</name>
		<desc>Spellbound Dizzy is a continuation of the adventures of the egg and his friends, this time containing over 100 screens with individual names. Dizzy has trapped himself in a strange world whilst experimenting with a spell book, and must find his way out. Gameplay is mostly similar to the previous games, combining puzzle-solving and object manipulation with arcade-style jumps and hazard-dodging. Lateral thinking is required to decipher the riddles found on scrolls. There are also extra arcade sections on a runaway mine-cart and for a spot of scuba-diving.</desc>
		<image>./images/Dizzy V - Spellbound Dizzy.z80-image.png</image>
		<marquee>./images/Dizzy V - Spellbound Dizzy.z80-marquee.png</marquee>
		<thumbnail>./images/Dizzy V - Spellbound Dizzy.z80-thumb.png</thumbnail>
		<fanart>./images/Dizzy V - Spellbound Dizzy.z80-fanart.png</fanart>
		<titleshot>./images/Dizzy V - Spellbound Dizzy.z80-titleshot.png</titleshot>
		<manual>./manuals/Dizzy V - Spellbound Dizzy.z80-manual.png</manual>
		<magazine>./magazines/Dizzy V - Spellbound Dizzy.z80-magazine.png</magazine>
		<map>./images/Dizzy V - Spellbound Dizzy.z80-map.png</map>
		<boxback>./images/Dizzy V - Spellbound Dizzy.z80-boxback.png</boxback>
		<rating>1</rating>
		<releasedate>19910101T000000</releasedate>
		<developer>Big Red Software</developer>
		<publisher>Codemasters</publisher>
		<kidgame>true</kidgame>
		<playcount>2</playcount>
		<lastplayed>20210110T014355</lastplayed>
		<md5>b87383358236c835818102dacdcd9d8a</md5>
		<gametime>51</gametime>
		<lang>en</lang>
*/