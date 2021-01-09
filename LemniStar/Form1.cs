using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text;
using System.Windows.Forms;

namespace LemniStar
{
    public partial class Form1 : Form
    {
        int nAnim = 256;
        int nPeaks = 9; //4; 5; 6; ...12; //Anzahl der Spitzen des Sterns
        double speed = 0.5;
        int curL = 0;
        int rotdir = -1; //direction of rotation right = -1; left = +1
        float scalefact = 100.0F;
        //flags
        bool paused = false;
        Vector2[] Lemniscate;
        //Background colors
        Color bgHigh = Color.Black;
        Color bgLow = Color.DarkBlue; //Color.AliceBlue; //

        public Form1()
        {
            InitializeComponent();
            this.MouseWheel += new MouseEventHandler(Form1_MouseWheel); //for zooming also use '+' and '-'
            CreateLemniscate();
            pictureBox1.Dock = DockStyle.Fill;
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            InitGradientBackground(pictureBox1);
            timer1.Interval = (int)((1000.0 / nAnim) / speed);
            timer1.Enabled = true;
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            pictureBox1.Invalidate();
        }
        private void Form1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)'p') paused = !paused;
            if (e.KeyChar == (char)'o') rotdir = -rotdir;
            if (e.KeyChar == (char)'+') ScaleDir(1);
            if (e.KeyChar == (char)'-') ScaleDir(-1);
            if (e.KeyChar == (char)'a') nPeaks += 1;
            if (e.KeyChar == (char)'s') nPeaks -= 1;
            if (nPeaks <=1) nPeaks = 2;
        }
        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape) this.Dispose();
        }
        private void Form1_MouseWheel(object sender, MouseEventArgs e)
        {
            ScaleDir(e.Delta);
        }
        private void ScaleDir(int s)
        {
            if (s > 0) { scalefact *= 1.05F;} else { scalefact /= 1.05F;}
        }
        private void Form1_Resize(object sender, EventArgs e)
        {
            InitGradientBackground(pictureBox1);
        }
        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.TranslateTransform(pictureBox1.ClientSize.Width / 2, pictureBox1.ClientSize.Height / 2);
            g.ScaleTransform(scalefact, -scalefact);
            double angle = 2 * Math.PI / nPeaks;
            Vector2 nextLemi = NextLemniPoint();
            Vector2 firstPeak = new Vector2(0.0, 1.0);
            Vector2 nextPeak = new Vector2(firstPeak);
            int sz = 2 * nPeaks + 1;
            PointF[] points = new PointF[sz];
            for (int i = 0; i < sz - 1; i += 2)
            {
                points[i] = new PointF((float)nextPeak.x, (float)nextPeak.y);
                points[i + 1] = new PointF((float)nextLemi.x, (float)nextLemi.y);
                nextPeak = Vector2.rotate(nextPeak, -angle);
                nextLemi = Vector2.rotate(nextLemi, -angle);
            }
            points[sz - 1] = new PointF((float)firstPeak.x, (float)firstPeak.y);
            Brush B = getRadialGradientBrush(points);
            g.FillPolygon(B, points);
        }
        private void InitGradientBackground(PictureBox canvas)
        {
            Bitmap gradientBackground = new Bitmap(canvas.ClientSize.Width, canvas.ClientSize.Height);
            using (Graphics g = Graphics.FromImage(gradientBackground))
            {
                //g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                LinearGradientBrush lgb = new LinearGradientBrush(new Point(0, 0), new Point(0, canvas.ClientSize.Height),
                    bgHigh, bgLow);
                g.FillRectangle(lgb, new Rectangle(0, 0, canvas.ClientSize.Width, canvas.ClientSize.Height));
                //canvas.Image = gradientBackground;
                canvas.BackgroundImage = gradientBackground;
                canvas.Invalidate();
            }
        }
        private PathGradientBrush getRadialGradientBrush(PointF[] points)
        {
            GraphicsPath path = new GraphicsPath();
            RectangleF R;
            R = getExtent(points);
            R = getSurroundCircle(R);
            path.AddEllipse(R);
            //path.AddRectangle(R);//???
            PathGradientBrush B = new PathGradientBrush(path); //Brush zum Füllen der Kugel erstellen
            B.CenterColor = Color.White;
            Color[] colors = { Color.Yellow, Color.Yellow }; //Randfarbe der Kugel
            B.SurroundColors = colors;
            return B;
        }
        private RectangleF getExtent(PointF[] points)
        {
            float xMin = 0, yMin = 0, xMax = 0, yMax = 0;
            foreach (PointF p in points)
            {
                xMin = Math.Min(xMin, p.X); yMin = Math.Min(yMin, p.Y);
                xMax = Math.Max(xMax, p.X); yMax = Math.Max(yMax, p.Y);
            }
            return new RectangleF(xMin, yMin, xMax - xMin, yMax - yMin);
        }
        private RectangleF getSurroundCircle(RectangleF rect)
        {
            //float w = xMax - xMin;
            //float h = yMax - yMin;
            float mx = rect.X + rect.Width / 2;
            float my = rect.Y + rect.Height / 2;
            float w2 = rect.Width / 2;
            float h2 = rect.Height / 2;
            float r = (float)Math.Sqrt((double)(w2 * w2 + h2 * h2));
            return new RectangleF(mx - r, my - r, 2 * r, 2 * r);
            //da is wohl irgendwo ein Denkfehler???
        }
        private void CreateLemniscate()
        {
            double t, sint, sint4, cost, v;
            double a = 1.0;//.6;
            double Sqrt2 = 1.4142135623730950488016887242097;
            double TWO_PI = 2 * Math.PI;

            Lemniscate = new Vector2[nAnim];
            for (int i = 0; i < nAnim; ++i)
            {
                t = TWO_PI * i / nAnim;
                sint = Math.Sin(t); sint4 = Math.Pow(sint, 4);
                cost = Math.Cos(t);
                v = a * Sqrt2 * (cost / (1 + sint4));
                Lemniscate[i] = new Vector2(v * sint, v);
            }
        }
        private Vector2 NextLemniPoint()
        {
            Vector2 p = Lemniscate[curL];
            if (!paused) curL += rotdir;
            if (curL >= nAnim) curL = 0;
            if (curL < 0) curL = nAnim - 1;
            return p;
        }
        private struct Vector2
        {
            public double x;
            public double y;
            //copyconstructor
            public Vector2(Vector2 other) { x = other.x; y = other.y; }
            public Vector2(double X, double Y) { x = X; y = Y; }
            public void mult(double scalar) { x *= scalar; y *= scalar; }
            public double angle() { return Math.Atan2(y, x); }
            public static Vector2 rotate(Vector2 V, double rotangle)
            {
                double a = V.angle() + rotangle;
                double l = V.length();
                return new Vector2(Math.Cos(a) * l, Math.Sin(a) * l);
            }
            public double length() { return Math.Sqrt(x * x + y * y); }
        }
        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }
    }
}