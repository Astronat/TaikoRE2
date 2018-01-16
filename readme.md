====== Taiko reverse engineering project

------ Tools:

### sheet viewer
allows you to view the contents of a Taiko no Tatsujin note sheet file, currently with support for the "newsht" format (Wii 4/5, PSP, or anything newer than those)

requires `SkiaSharp` and `SkiaSharp.Views` from nuget. it should probably tell you that by default but I dunno

namco plz don't sue me I love your games so much

------ RE list in order of priority

### track format (bin in sheet/newsht) MOSTLY COMPLETE
- still need to work out the rest of the note types, as well as the changey ex tracks
- BUT WHAT IS NOTETYPE 0x00000083?!

#### tuning file (bin in sheet/tuning)
- data seems to be blocks for each track, then a list of all tracks, all their difficulties, and their names in UTF-8 format
- this file probably also contains song difficulties (stars) and definitely points to correct filenames (yesss)

#### texture format (TPL/CTPL)

#### musicinfo.bin and fumensync.bin in sheet/musicinfo 
- might contain some useful stuff
- I suspect it's to do with file format handling though

#### lyric files (CBIN, sheet/lyrics)
- very likely simply just a list of timecodes, maybe something font related, and the lyrics themselves

#### font files (CBRFNT, font folder)
- probably a list of glyphs, duh, plus some metadata

#### menu files (CLMA, lm folder)
- not horribly important at all, I don't need to create the exact same goddamn menu as the original games