using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaikoSheetReader2
{
    class Program
    {
        static void Main(string[] args)
        {
            SongData sd = NewShtReader.ReadSheet(@"J:\Taiko\wii5 sheets\sheet\newsht\solo\linda_m.bin");
            int c = 0;
            foreach (Bar b in sd.Bars) {
                c++;
                Console.WriteLine("Bar #" + c + " (" + b.NoteCount + ") : @" + b.Timecode + " S:" + b.Speed);
                
                foreach (Note n in b.Notes) {
                    string type = NewShtReader.GetNoteType(n.NoteType);
                    if (type == "Don" ) {
                        Console.BackgroundColor = ConsoleColor.DarkRed;
                        Console.WriteLine(type);
                    }
                    else if (type == "Katsu") {
                        Console.BackgroundColor = ConsoleColor.DarkBlue;
                        Console.WriteLine(type);
                    }
                    else {
                        Console.BackgroundColor = ConsoleColor.DarkYellow;
                        Console.WriteLine(type);
                    }
                }
                Console.BackgroundColor = ConsoleColor.Black;
            }
            Console.Read();
        }
    }
}
