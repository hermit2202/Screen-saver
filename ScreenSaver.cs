using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Runtime.Versioning;
using System.Windows.Forms;

namespace Screen_saver
{
    /// <summary>
    /// Основная форма экрана заставки с снежинками.
    /// Представляет полноэкранное приложение с анимацией падающих снежинок.
    /// </summary>
    public partial class ScreenSaver : Form
    {
        private List<Snowflake> snowflakes = [];

        private const int SnowFlakeCount = 100;
        private const int IntervalTimer = 40;
        private const float MinSnowflakeSpeed = 5f;
        private const float MaxSnowflakeSpeed = 25f;
        private const int MinSnowflakeSize = 10;
        private const int MaxSnowflakeSize = 60;
        private const int InitialSpawnHeightMultiplier = 3;
        private const int DrawStartPositionX = 0;
        private const int DrawStartPositionY = 0;

        private Random random = new();
        private Bitmap bufferBitmap;
        private Graphics bufferGraphics;
        private Graphics screenGraphics = null!;  
        private float deltaTime;
        private DateTime lastFrameTime;
        private System.Windows.Forms.Timer animationTimer;
        private Image cleanBackground = null!;
        private Image snowflakeImage = null!;

        /// <summary>
        /// Инициализирует новый экземпляр класса ScreenSaver
        /// </summary>
        public ScreenSaver()
        {
            InitializeComponent();
            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;

            bufferBitmap = new Bitmap(Width, Height);
            bufferGraphics = Graphics.FromImage(bufferBitmap);

            animationTimer = new System.Windows.Forms.Timer();
            animationTimer.Interval = IntervalTimer;
            animationTimer.Tick += AnimationTimer_Tick;

            this.Paint += ScreenSaver_Paint;
            this.Load += ScreenSaver_Load;
            this.SizeChanged += ScreenSaver_SizeChanged;
            this.FormClosing += ScreenSaver_FormClosing;
            this.KeyDown += ScreenSaver_KeyDown;
            this.Click += ScreenSaver_Click;
            this.KeyPreview = true;
        }

        private void ScreenSaver_KeyDown(object? sender, KeyEventArgs e)
        {
            Application.Exit();
        }

        private void ScreenSaver_Click(object? sender, EventArgs e)
        {
            Application.Exit();
        }

        private void ScreenSaver_FormClosing(object? sender, FormClosingEventArgs e)
        {
            animationTimer.Stop();
            screenGraphics?.Dispose(); 
            bufferGraphics?.Dispose();
            bufferBitmap?.Dispose();
            cleanBackground?.Dispose();
            snowflakeImage?.Dispose();
        }

        private void ScreenSaver_Paint(object? sender, PaintEventArgs e)
        {
            e.Graphics.DrawImage(bufferBitmap, DrawStartPositionX, DrawStartPositionY);
        }

        private void ScreenSaver_Load(object? sender, EventArgs e)
        {
            this.BackColor = Color.Black;

            snowflakeImage = Properties.Resources.SnowFlake;
            cleanBackground = new Bitmap(Width, Height);
            using (var g = Graphics.FromImage(cleanBackground))
            {
                g.Clear(Color.Black);
                if (Properties.Resources.bkg != null)
                    g.DrawImage(Properties.Resources.bkg, DrawStartPositionX, DrawStartPositionY, Width, Height);
            }

            screenGraphics = this.CreateGraphics();

            InitializeSnowflakes();
            lastFrameTime = DateTime.Now;
            animationTimer.Start();
            this.Focus();
        }

        private void InitializeSnowflakes()
        {
            for (var i = 0; i < SnowFlakeCount; i++)
            {
                var size = random.Next(MinSnowflakeSize, MaxSnowflakeSize);
                var speed = MinSnowflakeSpeed + (float)(size - MinSnowflakeSize) * (MaxSnowflakeSpeed - MinSnowflakeSpeed) /
                    (MaxSnowflakeSize - MinSnowflakeSize);

                snowflakes.Add(new Snowflake
                {
                    X = random.Next(DrawStartPositionX, Width - size),
                    Y = random.Next(-Height * InitialSpawnHeightMultiplier, -size),
                    Size = size,
                    Speed = speed
                });
            }
        }

        private void InitializeBuffer()
        {
            bufferBitmap?.Dispose();
            bufferGraphics?.Dispose();

            bufferBitmap = new Bitmap(Width, Height);
            bufferGraphics = Graphics.FromImage(bufferBitmap);

            bufferGraphics.SmoothingMode = SmoothingMode.None;
            bufferGraphics.InterpolationMode = InterpolationMode.NearestNeighbor;
            bufferGraphics.Clear(Color.Black);
        }

        private void AnimationTimer_Tick(object? sender, EventArgs e)
        {
            var currentTime = DateTime.Now;
            deltaTime = (float)(currentTime - lastFrameTime).TotalSeconds;
            lastFrameTime = currentTime;

            if (deltaTime > 0.1f)
                deltaTime = 0.1f;

            var targetFrameRate = 60f;
            float frameMultiplier = deltaTime * targetFrameRate;

            MoveSnowflakes(frameMultiplier);
            DrawFrame();

            screenGraphics.DrawImage(bufferBitmap, DrawStartPositionX, DrawStartPositionY);
        }

        private void MoveSnowflakes(float frameMultiplier)
        {
            for (var i = 0; i < snowflakes.Count; i++)
            {
                var flake = snowflakes[i];
                flake.Y += (int)(flake.Speed * frameMultiplier);

                int offscreenResetThreshold = 100;
                if (flake.Y > this.ClientSize.Height + offscreenResetThreshold)
                {
                    flake.Size = random.Next(MinSnowflakeSize, MaxSnowflakeSize);

                    flake.Speed = MinSnowflakeSpeed + (float)(flake.Size - MinSnowflakeSize) /
                        (MaxSnowflakeSize - MinSnowflakeSize) * (MaxSnowflakeSpeed - MinSnowflakeSpeed);

                    flake.Y = random.Next(-this.ClientSize.Height * InitialSpawnHeightMultiplier, -flake.Size);
                    flake.X = random.Next(DrawStartPositionX, this.ClientSize.Width - flake.Size);
                }

                snowflakes[i] = flake;
            }
        }

        private void DrawFrame()
        {
            bufferGraphics.DrawImage(cleanBackground, DrawStartPositionX, DrawStartPositionY);

            foreach (var flake in snowflakes)
            {
                if (flake.Y + flake.Size > 0 && flake.Y < Height)
                {
                    bufferGraphics.DrawImage(snowflakeImage,
                        flake.X, flake.Y, flake.Size, flake.Size);
                }
            }
        }

        private void ScreenSaver_SizeChanged(object? sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized) return;
            if (Width <= 0 || Height <= 0) return;

            InitializeBuffer();

            cleanBackground?.Dispose();
            cleanBackground = new Bitmap(Width, Height);
            using (var g = Graphics.FromImage(cleanBackground))
            {
                g.Clear(Color.Black);
                if (Properties.Resources.bkg != null)
                    g.DrawImage(Properties.Resources.bkg, DrawStartPositionX, DrawStartPositionY,
                        Width, Height);
            }

            for (var i = 0; i < snowflakes.Count; i++)
            {
                var flake = snowflakes[i];
                if (flake.X > Width - flake.Size)
                    flake.X = random.Next(DrawStartPositionX, Width - flake.Size);
                snowflakes[i] = flake;
            }

            DrawFrame();
            screenGraphics?.Dispose();
            screenGraphics = this.CreateGraphics();
            screenGraphics.DrawImage(bufferBitmap, DrawStartPositionX, DrawStartPositionY);
        }
    }
}