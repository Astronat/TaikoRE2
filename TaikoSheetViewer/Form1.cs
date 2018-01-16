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
        SongData s;

        //Current X Position
        long cXPos = 0;

        //the current scale
        int scale = 80;

        public Form1() {
            InitializeComponent();

            Load += Form1_Load;

            //set up mouse events for moving the viewer around
            skC.MouseDown += Form1_MouseDown;
            skC.MouseUp += Form1_MouseUp;
            skC.MouseMove += Form1_MouseMove;
            skC.MouseLeave += Form1_MouseLeave;
        }

        //Form has loaded, set 
        private void Form1_Load(object sender, EventArgs e) {
            //temporary hard coded meemee
            string fn = @"J:\Taiko\wii5 sheets\sheet\newsht\solo\linda_h.bin";
            
            //read the sheet
            s = NewShtReader.ReadSheet(fn);

            //Get the total note count for ~statistics~
            int nc = 0;
            foreach (Bar b in s.Bars) { nc += b.NoteCount; }
            
            //Change the window title to show the file name, bar count and note count
            this.Text = fn + " " + s.BarCount +" bars | " + nc + " notes";
            
            //set up the Skia graphics control and its paint event
            skC.Size = this.Size;
            skC.PaintSurface += Gl_PaintSurface;
            
            //Add the Skia control to the form
            Controls.Add(skC);
        }
        
        bool isMouseDown = false;
        long lastMouseX = 0;


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
            //Clear the canvas, of course
            e.Surface.Canvas.Clear(SKColor.Parse("AAAAAA"));

            //TODO::OPTIMIZE AND REMOVE ALL COPY PASTE SHITE
            //TODO::ADD ZOOM
            //TODO::FIND THE REST OF THE NOTE TYPES
            //TODO::TRY SONGS LIKE THAT ONE WITH THE BEEPS THAT CHANGES PATH
            //TODO::WORK OUT BPM MATH OW MY HEAD TOO TIRED RN
            //TODO::FINISH COMMENTS

            //For each of the bars
            for (int i = 0; i < s.BarCount; i++) {
                e.Surface.Canvas.DrawLine(
                    cXPos + ((s.Bars[i].Timecode / 1000) * scale), 
                    0, cXPos + ((s.Bars[i].Timecode / 1000) * scale), 
                    30, new SKPaint());

                e.Surface.Canvas.DrawText(
                    "Bar " + i.ToString(), 
                    cXPos + ((s.Bars[i].Timecode / 1000) * scale), 
                    87, new SKPaint());

                e.Surface.Canvas.DrawText(
                    s.Bars[i].Speed.ToString("0.00"), 
                    cXPos + ((s.Bars[i].Timecode / 1000) * scale), 
                    100, new SKPaint());

                foreach (Note n in s.Bars[i].Notes) {
                    string type = NewShtReader.GetNoteType(n.NoteType);

                    SKPaint p = new SKPaint();

                    p.StrokeWidth = (type.StartsWith("Large") ? 16 : 10);

                    if (type == "Don" || type == "LargeDon") {
                        p.Color = SKColor.Parse("FF0000");
                    } else if (type == "Katsu" || type == "LargeKatsu") {
                        p.Color = SKColor.Parse("0000FF");
                    } else if (type == "Roll" || type == "LargeRoll") {
                        p.Color = SKColor.Parse("FFFF00");

                        e.Surface.Canvas.DrawText(
                            NewShtReader.GetNoteType(n.NoteType),
                            cXPos + ((s.Bars[i].Timecode + n.OffsetMs) / 1000) * scale,
                            50, p);

                        e.Surface.Canvas.DrawText(
                            n.NoteLength.ToString() + "ms", 
                            cXPos + ((s.Bars[i].Timecode + n.OffsetMs) / 1000) * scale, 
                            65, p);

                        e.Surface.Canvas.DrawLine(
                            cXPos + ((s.Bars[i].Timecode + n.OffsetMs) / 1000) * scale, 
                            14.5f, 
                            cXPos + (((s.Bars[i].Timecode + n.OffsetMs) / 1000) + (n.NoteLength / 1000)) * scale, 
                            14.5f, p);

                        e.Surface.Canvas.DrawCircle(
                            cXPos + (((s.Bars[i].Timecode + n.OffsetMs) / 1000) + (n.NoteLength / 1000)) * scale, 
                            15, 
                            (type.StartsWith("Large") ? 8 : 5), p);
                    } else { 
                        p.Color = SKColor.Parse("FFCC00");

                        e.Surface.Canvas.DrawText(
                            NewShtReader.GetNoteType(n.NoteType), 
                            cXPos + ((s.Bars[i].Timecode + n.OffsetMs) / 1000) * scale, 
                            50, p);

                        e.Surface.Canvas.DrawText(
                            n.RequiredHits.ToString() + " hits in " + n.NoteLength.ToString() + "ms", 
                            cXPos + ((s.Bars[i].Timecode + n.OffsetMs) / 1000) * scale, 
                            65, p);

                        e.Surface.Canvas.DrawLine(
                            cXPos + ((s.Bars[i].Timecode + n.OffsetMs) / 1000) * scale, 
                            14.5f, 
                            cXPos + (((s.Bars[i].Timecode + n.OffsetMs) / 1000) + (n.NoteLength / 1000)) * scale, 14.5f, p);

                        e.Surface.Canvas.DrawCircle(
                            cXPos + (((s.Bars[i].Timecode + n.OffsetMs) / 1000) + (n.NoteLength / 1000)) * scale,
                            15,
                            (type.StartsWith("Large") ? 8 : 5), p);
                    }

                    e.Surface.Canvas.DrawCircle(
                        cXPos + ((s.Bars[i].Timecode + n.OffsetMs) / 1000) * scale, 
                        15, 
                        (type.StartsWith("Large") ? 8 : 5), p);
                }
            }
        }
    }
}
