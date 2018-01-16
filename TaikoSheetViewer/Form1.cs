using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SkiaSharp;
using SkiaSharp.Views.Desktop;
using TaikoSheetReader2;
using OpenTK;


namespace WindowsFormsApp1 {
    public partial class Form1 : Form {
        SKControl skC = new SKControl();

        SongData sheet;

        SKRect BarRect;
        SKPaint skP;

        //Current X position
        long cXPos = 0;

        //Last Mouse X position
        long lastMouseX = 0;

        //the current scale
        int scale = 200;

        bool isMouseDown = false;

        public Form1() {
            InitializeComponent();

            Load += Form1_Load;

            //set up mouse events for moving the viewer around
            skC.MouseDown += Form1_MouseDown;
            skC.MouseUp += Form1_MouseUp;
            skC.MouseMove += Form1_MouseMove;
            skC.MouseLeave += Form1_MouseLeave;
        }

        //Form has loaded
        private void Form1_Load(object sender, EventArgs e) {
            //temporary hard coded meemee
            string fn = @"J:\Taiko\wii5 sheets\sheet\newsht\solo\linda_h.bin";
            
            //read the sheet
            sheet = NewShtReader.ReadSheet(fn);

            //Get the total note count for ~statistics~
            int nc = 0;
            foreach (Bar b in sheet.Bars) { nc += b.NoteCount; }
            
            //Change the window title to show the file name, bar count and note count
            Text = fn + " " + sheet.BarCount +" bars | " + nc + " notes";

            BarRect = new SKRect(0, 0, Size.Width, 30);

            //set up the Skia graphics control and its paint event
            skC.Size = Size;
            skC.PaintSurface += Gl_PaintSurface;

            
            //Add the Skia control to the form
            Controls.Add(skC);
        }
        
        private void Form1_MouseMove(object sender, MouseEventArgs e) {
            //if the mouse is down, move the current position accordingly
            if (isMouseDown) {
                cXPos = cXPos - (lastMouseX - e.X);
                //refresh the control to update the graphics
                skC.Refresh();
            }

            //keep an eye on where the mouse was last
            lastMouseX = e.X;
        }

        //Mouse is being clicked
        private void Form1_MouseDown(object sender, MouseEventArgs e) { isMouseDown = true; }

        //Mouse has stopped being clicked
        private void Form1_MouseUp(object sender, MouseEventArgs e) { isMouseDown = false; }

        //Obviously, disable the mouse scrolling if the mouse leaves the form
        private void Form1_MouseLeave(object sender, EventArgs e) { isMouseDown = false; }


        //Draw the ugly circles that make up the viewer
        private void Gl_PaintSurface(object sender, SKPaintSurfaceEventArgs e) {
            skP = new SKPaint();
            //Clear the canvas, of course
            e.Surface.Canvas.Clear(SKColor.Parse("AAAAAA"));
            e.Surface.Canvas.DrawLine(
                0,
                30,
                Size.Width,
                30, new SKPaint());

            skP.Color = SKColor.Parse("333333");

            e.Surface.Canvas.DrawRect(BarRect, skP);

            //TODO::OPTIMIZE AND REMOVE ALL COPY PASTE SHITE IN HERE
            //TODO::ADD ZOOM
            //TODO::FIND THE REST OF THE NOTE TYPES
            //TODO::TRY SONGS LIKE THAT ONE WITH THE BEEPS THAT CHANGES PATH
            //TODO::WORK OUT BPM MATH OW MY HEAD TOO TIRED RN
            //TODO::FINISH COMMENTS

            //For each of the bars
            for (int i = 0; i < sheet.BarCount; i++) {
                skP.StrokeWidth = 1;

                skP.Color = SKColor.Parse("AAAAAA");
                e.Surface.Canvas.DrawLine(
                    cXPos + ((sheet.Bars[i].Timecode / 1000) * scale), 
                    0, 
                    cXPos + ((sheet.Bars[i].Timecode / 1000) * scale), 
                    30, skP);

                skP.Color = SKColor.Parse("222222");
                e.Surface.Canvas.DrawLine(
                    cXPos + ((sheet.Bars[i].Timecode / 1000) * scale),
                    30,
                    cXPos + ((sheet.Bars[i].Timecode / 1000) * scale),
                    Size.Height, skP);

                e.Surface.Canvas.DrawText(
                    "Bar " + i.ToString(), 
                    cXPos + ((sheet.Bars[i].Timecode / 1000) * scale) + 3, 
                    87, skP);

                e.Surface.Canvas.DrawText(
                    sheet.Bars[i].Speed.ToString("0.00"), 
                    cXPos + ((sheet.Bars[i].Timecode / 1000) * scale) + 3, 
                    100, skP);

                foreach (Note n in sheet.Bars[i].Notes) {
                    string type = NewShtReader.GetNoteType(n.NoteType);
                    
                    skP.StrokeWidth = (type.StartsWith("Large") ? 16 : 10);

                    if (type == "Don" || type == "LargeDon") {
                        skP.Color = SKColor.Parse("DD4444");
                    } else if (type == "Katsu" || type == "LargeKatsu") {
                        skP.Color = SKColor.Parse("4444DD");
                    } else if (type == "Roll" || type == "LargeRoll") {
                        skP.Color = SKColor.Parse("DDDD44");

                        e.Surface.Canvas.DrawText(
                            NewShtReader.GetNoteType(n.NoteType),
                            cXPos + ((sheet.Bars[i].Timecode + n.OffsetMs) / 1000) * scale,
                            50, skP);

                        e.Surface.Canvas.DrawText(
                            n.NoteLength.ToString() + "ms", 
                            cXPos + ((sheet.Bars[i].Timecode + n.OffsetMs) / 1000) * scale, 
                            65, skP);

                        e.Surface.Canvas.DrawLine(
                            cXPos + ((sheet.Bars[i].Timecode + n.OffsetMs) / 1000) * scale, 
                            16f, 
                            cXPos + (((sheet.Bars[i].Timecode + n.OffsetMs) / 1000) + (n.NoteLength / 1000)) * scale, 
                            16f, skP);

                        e.Surface.Canvas.DrawCircle(
                            cXPos + (((sheet.Bars[i].Timecode + n.OffsetMs) / 1000) + (n.NoteLength / 1000)) * scale, 
                            16, 
                            (type.StartsWith("Large") ? 8 : 5), skP);
                    } else { 
                        skP.Color = SKColor.Parse("FFCC00");

                        e.Surface.Canvas.DrawText(
                            NewShtReader.GetNoteType(n.NoteType), 
                            cXPos + ((sheet.Bars[i].Timecode + n.OffsetMs) / 1000) * scale, 
                            50, skP);

                        e.Surface.Canvas.DrawText(
                            n.RequiredHits.ToString() + " hits in " + n.NoteLength.ToString() + "ms", 
                            cXPos + ((sheet.Bars[i].Timecode + n.OffsetMs) / 1000) * scale, 
                            65, skP);

                        e.Surface.Canvas.DrawLine(
                            cXPos + ((sheet.Bars[i].Timecode + n.OffsetMs) / 1000) * scale, 
                            16, 
                            cXPos + (((sheet.Bars[i].Timecode + n.OffsetMs) / 1000) + (n.NoteLength / 1000)) * scale, 16, skP);

                        e.Surface.Canvas.DrawCircle(
                            cXPos + (((sheet.Bars[i].Timecode + n.OffsetMs) / 1000) + (n.NoteLength / 1000)) * scale,
                            16,
                            (type.StartsWith("Large") ? 8 : 5), skP);
                    }

                    e.Surface.Canvas.DrawCircle(
                        cXPos + ((sheet.Bars[i].Timecode + n.OffsetMs) / 1000) * scale, 
                        16, 
                        (type.StartsWith("Large") ? 8 : 5), skP);
                }
            }
        }
    }
}
