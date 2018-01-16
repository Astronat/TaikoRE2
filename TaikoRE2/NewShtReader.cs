using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace TaikoSheetReader2 {
    public struct Bar {
        public float Speed;
        public float Timecode;
        public short NoteCount;
        public float DisplaySpeedChange;

        public long filePosition;

        public Note[] Notes;
    }

    public struct Note {
        public byte NoteType;
        public float OffsetMs;
        public short Points;
        public short PointBonus;

        public float NoteLength;
        public float NoteLengthUnknown;
    }

    public struct SongData {
        public int BarCount;
        public bool Loaded;
        public Bar[] Bars;
    }

    class NewShtReader {
        static readonly Dictionary<byte, string> NoteType = new Dictionary<byte, string>() {
            { 0x00,     "None"       },
            { 0x01,     "Don"        },
            { 0x02,     "Don"        },
            { 0x03,     "Don"        },
            { 0x04,     "Katsu"      },
            { 0x05,     "Katsu"      },
            { 0x06,     "Roll"       },
            { 0x07,     "LargeDon"   },
            { 0x08,     "LargeKatsu" },
            { 0x09,     "LargeRoll"  },
            { 0x10,     "Balloon"    },
            { 0x11,     "Dumpling"   },
            { 0x0C,     "PartyBall"  }
        };
        public static string GetNoteType(byte code) {
            string name;
            if (NoteType.TryGetValue(code, out name)) return name;
            else return "INVALID NOTE: " + code;
        }
    
        const int USEFUL_DATA_START = 0x200;

        public static SongData ReadSheet (string FileName) {
            using (FileStream fs = new FileStream(FileName, FileMode.Open)) {
                
                //seek to the first bit of data we care about
                fs.Seek(USEFUL_DATA_START, SeekOrigin.Begin);
                int barCount = ReadInt(fs) - 1;

                //skip 4 bytes
                //work this out
                fs.Seek(4, SeekOrigin.Current);

                Bar[] bars = new Bar[barCount];

                for (int i = 0; i < barCount; i++) {
                    //grab the speed and offset from the start of the bar then skip all the Fs
                    float speed = ReadFloat(fs);
                    float offset = ReadFloat(fs);

                    fs.Seek(32, SeekOrigin.Current);

                    //the note count for the current bar
                    short notecount = ReadShort(fs);
                    fs.Seek(2, SeekOrigin.Current);

                    //that one value that is 1 unless the speed is changing
                    float speedthing = ReadFloat(fs);

                    //create the array of notes
                    Note[] notes = new Note[notecount];

                    for (int n = 0; n < notecount; n++) {
                        //skip some zeroes
                        fs.Seek(3, SeekOrigin.Current);
                        //the current note type byte
                        byte notetype = ReadByte(fs);

                        //the current note's offset in ms
                        float noteoffset = ReadFloat(fs);

                        //skip the split in the middle of the note's data
                        fs.Seek(8, SeekOrigin.Current);
                        
                        //scoring data
                        short s1 = ReadShort(fs);
                        short s2 = ReadShort(fs);

                        //if the note is a roll or big roll, 
                        //we get things slightly differently to normal hits
                        if (notetype == 6 || notetype == 9) {
                            float length1 = ReadFloat(fs);
                            float length2 = ReadFloat(fs);
                            fs.Seek(4, SeekOrigin.Current);

                            notes[n] = new Note {
                                NoteType = notetype,
                                NoteLength = length1,
                                NoteLengthUnknown = length2,
                                OffsetMs = noteoffset,
                                Points = s1,
                                PointBonus = s2
                            };

                        //dumplings and the like are also different
                        //this is a dumpling specifically though
                        //more testing required for other types
                        } else if (notetype == 10) {
                            float length1 = ReadFloat(fs);

                            notes[n] = new Note {
                                NoteType = notetype,
                                NoteLength = length1,
                                NoteLengthUnknown = -1,
                                OffsetMs = noteoffset,
                                Points = s1,
                                PointBonus = s2
                            };

                        //nothing special in here
                        } else {
                            notes[n] = new Note {
                                NoteType = notetype,
                                NoteLength = -1,
                                NoteLengthUnknown = -1,
                                OffsetMs = noteoffset,
                                Points = s1,
                                PointBonus = s2
                            };

                            fs.Seek(4, SeekOrigin.Current);
                        }

                    }
                    bars[i] = new Bar { NoteCount = notecount, Notes = notes, Speed = speed, Timecode = offset, DisplaySpeedChange = speedthing, filePosition = fs.Position };

                    fs.Seek(16, SeekOrigin.Current);
                }
                SongData sd = new SongData { BarCount = barCount, Loaded = true, Bars = bars };

                return sd;
            }                
        }

        private static byte ReadByte(FileStream fs) {
            byte[] b = new byte[1];
            fs.Read(b, 0, 1);
            return b[0];
        }

        private static int ReadInt(FileStream fs) {
            byte[] b = new byte[4];
            fs.Read(b, 0, 4);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(b);
            return BitConverter.ToInt32(b, 0);
        }

        private static short ReadShort(FileStream fs) {
            byte[] b = new byte[2];
            fs.Read(b, 0, 2);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(b);
            return BitConverter.ToInt16(b, 0);
        }

        private static float ReadFloat(FileStream fs) {
            byte[] b = new byte[4];
            fs.Read(b, 0, 4);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(b);   
            return BitConverter.ToSingle(b, 0);
        }
    }
}
