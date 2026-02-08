using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Runtime.Versioning;
using System.Windows.Forms;

namespace Screen_saver
{
    public partial class Form1 : Form
    {
        private List<int> snowFlakeY = new List<int>();
        private List<int> snowFlakeX = new List<int>();
        private List<int> snowFlakeSize = new List<int>();
        private List<int> snowFlakeSpeed = new List<int>();

        private const int snowFlakeCount = 150;
        private const int intervalTimer = 20;
        private const int minSpeed = 5;
        private const int maxSpeed = 25;
        private const int minSize = 10;
        private const int maxSize = 60;

        Random rand = new Random();

        private Bitmap? bufferBitmap;
        private Image? originalSnowFlake;

        public Form1()
        {
            InitializeComponent();

            this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);

            this.Load += Form1_Load;
            this.Paint += Form1_Paint;
            this.Resize += Form1_Resize;

            var timer = new System.Windows.Forms.Timer();
            timer.Interval = intervalTimer;
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void Form1_Paint(object? sender, PaintEventArgs e)
        {
            if (bufferBitmap == null)
            {
                return;
            }

            e.Graphics.DrawImage(bufferBitmap, 0, 0);

            for (int i = 0; i < snowFlakeCount; i++)
            {
                if (originalSnowFlake != null)
                {
                    e.Graphics.DrawImage(originalSnowFlake,
                        snowFlakeX[i], snowFlakeY[i],
                        snowFlakeSize[i], snowFlakeSize[i]);
                }
            }
        }

        private void Form1_Load(object? sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Maximized;
            this.FormBorderStyle = FormBorderStyle.None;

            this.BackColor = Color.Black;
            originalSnowFlake = Properties.Resources.SnowFlake;
            CreateBackgroundBuffer();
            InitializeSnowflakes();
        }

        private void InitializeSnowflakes()
        {
            snowFlakeX.Clear();
            snowFlakeY.Clear();
            snowFlakeSize.Clear();
            snowFlakeSpeed.Clear();

            for (int i = 0; i < snowFlakeCount; i++)
            {
                int size = rand.Next(minSize, maxSize + 1);
                snowFlakeSize.Add(size);

                int speed = minSpeed + (int)((float)(size - minSize) / (maxSize - minSize) * (maxSpeed - minSpeed));
                snowFlakeSpeed.Add(speed);

                snowFlakeX.Add(rand.Next(0, this.ClientSize.Width - size));
                snowFlakeY.Add(rand.Next(-this.ClientSize.Height, 0));
            }
        }

        private void CreateBackgroundBuffer()
        {
            if (this.ClientSize.Width > 0 && this.ClientSize.Height > 0)
            {
                if (bufferBitmap != null)
                    bufferBitmap.Dispose();

                bufferBitmap = new Bitmap(this.ClientSize.Width, this.ClientSize.Height);
                using (var g = Graphics.FromImage(bufferBitmap))
                {
                    g.DrawImage(Properties.Resources.bkg, 0, 0,
                               this.ClientSize.Width, this.ClientSize.Height);
                }
            }
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            List<Rectangle> updateAreas = new List<Rectangle>();

            for (int i = 0; i < snowFlakeX.Count; i++)
            {
                int oldY = snowFlakeY[i];
                int size = snowFlakeSize[i];

                snowFlakeY[i] += snowFlakeSpeed[i];

                if (snowFlakeY[i] > this.ClientSize.Height)
                {
                    snowFlakeSize[i] = rand.Next(minSize, maxSize + 1);
                    size = snowFlakeSize[i];

                        snowFlakeSpeed[i] = minSpeed +
                        (int)((float)(size - minSize) / (maxSize - minSize) * (maxSpeed - minSpeed));

                    snowFlakeY[i] = -size;
                    snowFlakeX[i] = rand.Next(0, this.ClientSize.Width - size);
                }

                updateAreas.Add(new Rectangle(snowFlakeX[i], oldY, size, size));

                updateAreas.Add(new Rectangle(snowFlakeX[i], snowFlakeY[i], size, size));
            }


            this.Refresh();
        }

        private void Form1_Resize(object? sender, EventArgs e)
        {
            if (bufferBitmap != null)
            {
                bufferBitmap.Dispose();
                bufferBitmap = null!;
            }
            CreateBackgroundBuffer();
            InitializeSnowflakes();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            Application.Exit();
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            Application.Exit();
        }
    }
}