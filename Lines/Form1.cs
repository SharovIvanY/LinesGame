using System;
using System.Drawing;
using System.Windows.Forms;

namespace Lines
{
    public enum Item
    {
        none,
        ball,
        jump,
        next,
        path
    }
    public delegate void ShowItem(Ball ball, Item item);
    public partial class Form1 : Form
    {
        int max = 9;
        int size = 64;
        PictureBox[,] box;
        Game game;
        
        public Form1()
        {
            InitializeComponent();
            CreateBoxes();
            game = new Game(max, ShowItem);
            timer.Enabled = true;
        }

        public void CreateBoxes()
        {
            box = new PictureBox[max, max];
            for(int x = 0; x < max; x++)
                for(int y = 0; y < max; y++)
                {
                    box[x, y] = new PictureBox();
                    panel.Controls.Add(box[x, y]);
                    box[x, y].BorderStyle = BorderStyle.FixedSingle;
                    box[x, y].Size = new Size(size, size);
                    box[x, y].Location = new Point(x * (size - 1), y * (size-1));
                    box[x, y].Image = Properties.Resources.none;
                    box[x, y].SizeMode = PictureBoxSizeMode.Zoom;
                    box[x, y].Click += new EventHandler(this.PictureBox_Click);
                    box[x, y].Tag = new Point(x, y);
                }
            panel.Size = new Size(max * (size - 1) + 2, max * (size - 1) + 2);
        }

        private void PictureBox_Click(object sender, EventArgs e)
        {
            Point xy = (Point)((PictureBox)sender).Tag;
            game.ClickBox(xy.X, xy.Y);
        }

        private Bitmap ImgBall(int nr)
        {
            switch(nr)
            {
                case 1: return Properties.Resources.ball1;
                case 2: return Properties.Resources.ball2;
                case 3: return Properties.Resources.ball3;
                case 4: return Properties.Resources.ball4;
                case 5: return Properties.Resources.ball5;
                case 6: return Properties.Resources.ball6;
            }
            return null;
        }

        private Bitmap ImgJump(int nr)
        {
            switch (nr)
            {
                case 1: return Properties.Resources.ball1s;
                case 2: return Properties.Resources.ball2s;
                case 3: return Properties.Resources.ball3s;
                case 4: return Properties.Resources.ball4s;
                case 5: return Properties.Resources.ball5s;
                case 6: return Properties.Resources.ball6s;
            }
            return null;
        }
        private Bitmap ImgNext(int nr)
        {
            switch (nr)
            {
                case 1: return Properties.Resources.ball1n;
                case 2: return Properties.Resources.ball2n;
                case 3: return Properties.Resources.ball3n;
                case 4: return Properties.Resources.ball4n;
                case 5: return Properties.Resources.ball5n;
                case 6: return Properties.Resources.ball6n;
            }
            return null;
        }

        private void ShowItem(Ball ball, Item item)
        {
            Image img;

            switch(item)
            {
                case Item.none: img = Properties.Resources.none; break;
                case Item.ball: img = ImgBall(ball.color); break;
                case Item.next: img = ImgNext(ball.color); break;
                case Item.jump: img = ImgJump(ball.color); break;
                case Item.path: img = Properties.Resources.path; break;
                default       : img = Properties.Resources.none; break;

            }

            box[ball.x, ball.y].Image = img;
            box[ball.x, ball.y].SizeMode = PictureBoxSizeMode.Zoom;
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            game.Step();
        }
    }
}
