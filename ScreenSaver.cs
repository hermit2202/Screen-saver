using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Runtime.Versioning;
using System.Windows.Forms;

namespace Screen_saver
{
    /// <summary>
    /// Структура для хранения параметров снежинки (инкапсуляция как в эталоне)
    /// </summary>
    public struct Snowflake  
    {
        public int X;
        public int Y;
        public int Size;
        public int Speed;
    }

    /// <summary>
    /// Основная форма экрана заставки с снежинками.
    /// Представляет полноэкранное приложение с анимацией падающих снежинок.
    /// </summary>
    public partial class ScreenSaver : Form
    {
        private List<Snowflake> snowflakes = []; 

        private const int snowFlakeCount = 100;
        private const int intervalTimer = 40;  
        private const int minSnowflakeSpeed = 5;
        private const int maxSnowflakeSpeed = 25;
        private const int minSnowflakeSize = 10;
        private const int maxSnowflakeSize = 60;
        private const int InitialSpawnHeightMultiplier = 3; 
        private const int SpawnBuffer = 100;
        private const int drawStarPositionX= 0;
        private const int drawStarPositionY = 0;

        private Random random = new();
        private Bitmap bufferBitmap = null!;
        private Graphics bufferGraphics = null!;  
        private Image originalSnowFlake = null!;

        private float deltaTime;  
        private DateTime lastFrameTime;  
        private System.Windows.Forms.Timer animationTimer;  

        /// <summary>
        /// Инициализирует новый экземпляр класса ScreenSaver
        /// </summary>
        public ScreenSaver()
        {
            InitializeComponent();
            this.FormBorderStyle = FormBorderStyle.None;  
            this.WindowState = FormWindowState.Maximized;  

            animationTimer = new System.Windows.Forms.Timer();
            animationTimer.Interval = intervalTimer;
            animationTimer.Tick += AnimationTimer_Tick; 

            this.Paint += ScreenSaver_Paint;
            this.Load += ScreenSaver_Load;
            this.Resize += ScreenSaver_Resize;
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
        }

        private void ScreenSaver_Paint(object? sender, PaintEventArgs e)
        {
            e.Graphics.DrawImage(bufferBitmap, drawStarPositionX, drawStarPositionY);
        }

        private void ScreenSaver_Load(object? sender, EventArgs e)
        {
            this.BackColor = Color.Black;
            originalSnowFlake = Properties.Resources.SnowFlake;

            InitializeBuffer();  
            InitializeSnowflakes();
            lastFrameTime = DateTime.Now;  
            animationTimer.Start();  
            this.Focus();
        }

        private void InitializeSnowflakes()
        {
            snowflakes.Clear();

            for (int i = 0; i < snowFlakeCount; i++)
            {
                int size = random.Next(minSnowflakeSize, maxSnowflakeSize + 1);
                int speed = minSnowflakeSpeed + (int)((float)(size - minSnowflakeSize) / (maxSnowflakeSize - minSnowflakeSize) * (maxSnowflakeSpeed - minSnowflakeSpeed));

                snowflakes.Add(new Snowflake
                {
                    X = random.Next(0, this.ClientSize.Width - size),
                    Y = random.Next(-this.ClientSize.Height * InitialSpawnHeightMultiplier, -size),
                    Size = size,
                    Speed = speed
                });
            }
        }

        private void InitializeBuffer()
        {
            bufferBitmap?.Dispose();
            bufferGraphics?.Dispose();

            bufferBitmap = new Bitmap(this.ClientSize.Width, this.ClientSize.Height);
            bufferGraphics = Graphics.FromImage(bufferBitmap);

            bufferGraphics.SmoothingMode = SmoothingMode.None;
            bufferGraphics.InterpolationMode = InterpolationMode.NearestNeighbor;
            bufferGraphics.Clear(Color.Black);

            if (Properties.Resources.bkg != null)
            {
                bufferGraphics.DrawImage(Properties.Resources.bkg, drawStarPositionX, drawStarPositionY,
                    this.ClientSize.Width, this.ClientSize.Height);
            }
        }

        private void AnimationTimer_Tick(object? sender, EventArgs e)
        {
            var currentTime = DateTime.Now;
            deltaTime = (float)(currentTime - lastFrameTime).TotalSeconds;
            lastFrameTime = currentTime;

            if (deltaTime > 0.1f) deltaTime = 0.1f;
            float frameMultiplier = deltaTime * 60f; 

            MoveSnowflakes(frameMultiplier);  
            DrawFrame();      
            UpdateScreen(); 
        }

        private void MoveSnowflakes(float frameMultiplier)
        {
            for (int i = 0; i < snowflakes.Count; i++)
            {
                var flake = snowflakes[i];
                flake.Y += (int)(flake.Speed * frameMultiplier);

  
                if (flake.Y > this.ClientSize.Height + SpawnBuffer)
                {
                    flake.Size = random.Next(minSnowflakeSize, maxSnowflakeSize + 1);
                    flake.Speed = minSnowflakeSpeed + (int)((float)(flake.Size - minSnowflakeSize) / (maxSnowflakeSize - minSnowflakeSize) * (maxSnowflakeSpeed - minSnowflakeSpeed));
                    flake.Y = random.Next(-this.ClientSize.Height * InitialSpawnHeightMultiplier, -flake.Size);
                    flake.X = random.Next(0, this.ClientSize.Width - flake.Size);
                }

                snowflakes[i] = flake;
            }
        }

        private void DrawFrame()
        {
            bufferGraphics.Clear(Color.Black);
            bufferGraphics.DrawImage(Properties.Resources.bkg, drawStarPositionX, drawStarPositionY,
                this.ClientSize.Width, this.ClientSize.Height);

             foreach (var flake in snowflakes)
             {
                 if (flake.Y + flake.Size > 0)  
                 {
                     bufferGraphics.DrawImage(originalSnowFlake,
                         flake.X, flake.Y, flake.Size, flake.Size);
                 }
             }
        }

        private void UpdateScreen()
        {
            using (Graphics screenGraphics = this.CreateGraphics())
            {
                screenGraphics.SmoothingMode = SmoothingMode.AntiAlias;
                screenGraphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                screenGraphics.DrawImage(bufferBitmap, drawStarPositionX, drawStarPositionY);
            }
        }

        private void ScreenSaver_Resize(object? sender, EventArgs e)
        {
            InitializeBuffer();   
            InitializeSnowflakes(); 
            UpdateScreen();       
        }
    }
}