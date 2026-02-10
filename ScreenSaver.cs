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
        private List<int> snowFlakeY = [];
        private List<int> snowFlakeX = [];
        private List<int> snowFlakeSize = [];
        private List<int> snowFlakeSpeed = [];

        private const int snowFlakeCount = 150;
        private const int intervalTimer = 20;
        private const int minSnowflakeSpeed = 5;
        private const int maxSnowflakeSpeed = 25;
        private const int minSnowflakeSize = 10;
        private const int maxSnowflakeSize = 60;
        private const int drawStartPositionX = 0;
        private const int drawStartPositionY = 0;

        Random random = new();

        private Bitmap bufferBitmap;
        private Image? originalSnowFlake;

        /// <summary>
        /// Инициализирует новый экземпляр класса ScreenSaver
        /// Настраивает параметры отрисовки и запускает таймер анимации.
        /// </summary>
        public ScreenSaver()
        {
            InitializeComponent();

            this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);

            this.Load += ScreenSaver_Load;
            this.Paint += ScreenSaver_Paint;
            this.Resize += ScreenSaver_Resize;

            bufferBitmap = new Bitmap(1, 1);

            var timer = new System.Windows.Forms.Timer();
            timer.Interval = intervalTimer;
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void ScreenSaver_Paint(object? sender, PaintEventArgs e)
        {
            e.Graphics.DrawImage(bufferBitmap, drawStartPositionX, drawStartPositionY);

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

        private void ScreenSaver_Load(object? sender, EventArgs e)
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
                int size = random.Next(minSnowflakeSize, maxSnowflakeSize + 1);
                snowFlakeSize.Add(size);

                int speed = minSnowflakeSpeed + (int)((float)(size - minSnowflakeSize) / (maxSnowflakeSize - minSnowflakeSize) * (maxSnowflakeSpeed - minSnowflakeSpeed));
                snowFlakeSpeed.Add(speed);

                snowFlakeX.Add(random.Next(drawStartPositionX, this.ClientSize.Width - size));
                snowFlakeY.Add(random.Next(-this.ClientSize.Height, drawStartPositionY));
            }
        }

        private void CreateBackgroundBuffer()
        {
            if (bufferBitmap != null)
            { 
             bufferBitmap.Dispose();   
            }

            bufferBitmap = new Bitmap(this.ClientSize.Width, this.ClientSize.Height);
            using (var g = Graphics.FromImage(bufferBitmap))
            {
                g.DrawImage(Properties.Resources.bkg, drawStartPositionX, drawStartPositionY,
                           this.ClientSize.Width, this.ClientSize.Height);
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
                    snowFlakeSize[i] = random.Next(minSnowflakeSize, maxSnowflakeSize + 1);
                    size = snowFlakeSize[i];

                        snowFlakeSpeed[i] = minSnowflakeSpeed +
                        (int)((float)(size - minSnowflakeSize) / (maxSnowflakeSize - minSnowflakeSize) * (maxSnowflakeSpeed - minSnowflakeSpeed));

                    snowFlakeY[i] = -size;
                    snowFlakeX[i] = random.Next(0, this.ClientSize.Width - size);
                }

                updateAreas.Add(new Rectangle(snowFlakeX[i], oldY, size, size));

                updateAreas.Add(new Rectangle(snowFlakeX[i], snowFlakeY[i], size, size));
            }

            this.Refresh();
        }

        private void ScreenSaver_Resize(object? sender, EventArgs e)
        {
            bufferBitmap.Dispose();
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