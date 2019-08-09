using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Hollow_Knight_Screensaver
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private int animationFrame = 1;
        private int frames = 0;
        private Timer timer = new Timer();
        private Stopwatch stopwatch = new Stopwatch();
        private bool stopped = false;
        private double xSpeed;
        private double ySpeed;
        private double xMax;
        private double yMax;
        private Random rng = new Random();
        private bool caged = false;
        private int mouseMoveFrames = 0;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void DoAnimationStep(object source, ElapsedEventArgs e)
        {
            animationFrame = animationFrame >= 9
                ? 1
                : animationFrame + 1;
            frames++;

            if (mouseMoveFrames > 0) mouseMoveFrames--;

            Dispatcher.Invoke((Action)delegate ()
            {
                gruzImage.Source = new BitmapImage(new Uri("/Resources/0" + animationFrame.ToString() + ".png", UriKind.Relative));
            });
        }

        protected void ProcessGruzMother(object sender, EventArgs e)
        {
            if (!stopwatch.IsRunning)
            {
                stopwatch.Start();
                return;
            }

            double delta = stopwatch.Elapsed.TotalSeconds;
            stopwatch.Restart();

            GruzTransformRelative(delta * xSpeed, delta * ySpeed);

            double x = gruzImage.Margin.Left;
            double y = gruzImage.Margin.Top;

            if (caged)
            {
                if (x > xMax + 50)
                {
                    xSpeed = -300 + (rng.NextDouble() * 25);
                    ySpeed = Math.Sign(ySpeed) * 100 + (rng.NextDouble() * 200);
                }

                if (x < -50)
                {
                    xSpeed = 300 - (rng.NextDouble() * 25);
                    ySpeed = Math.Sign(ySpeed) * 100 + (rng.NextDouble() * 200);
                }

                if (y > yMax + 20)
                {
                    ySpeed = -250 + (rng.NextDouble() * 100);
                }

                if (y < -40)
                {
                    ySpeed = 250 - (rng.NextDouble() * 100);
                }

                ((ScaleTransform)gruzImage.RenderTransform).ScaleX = Math.Sign(xSpeed);
            } else if (x >= 0 && x <= xMax && y >= 0 && y <= yMax)
            {
                if (!stopped) caged = true;
            } else if (stopped && (x > xMax + gruzImage.ActualWidth || y > yMax + gruzImage.ActualHeight || x < -gruzImage.ActualWidth || y < -gruzImage.ActualHeight))
            {
                Close();
            }

            if (stopped)
            {
                xSpeed *= 1 + delta * 2;
                ySpeed *= 1 + delta * 2;
            }
        }

        private void GruzTransformRelative(double x, double y)
        {
            gruzImage.Margin = new Thickness(
                gruzImage.Margin.Left + x,
                gruzImage.Margin.Top + y,
                gruzImage.Margin.Right,
                gruzImage.Margin.Bottom
            );
        }

        private void GruzTransformAbsolute(double x, double y)
        {
            gruzImage.Margin = new Thickness(
                x,
                y,
                gruzImage.Margin.Right,
                gruzImage.Margin.Bottom
            );
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            timer.Stop();
        }

        private void HideScreenSaver()
        {
            if (stopped) return;
            stopped = true;
            caged = false;
            ColorAnimation backgroundAnimation = new ColorAnimation();
            backgroundAnimation.From = ((SolidColorBrush) this.Background).Color;
            backgroundAnimation.To = Color.FromArgb(0, 0, 0, 0);
            backgroundAnimation.Duration = new Duration(TimeSpan.FromSeconds(0.5));
            Background.BeginAnimation(SolidColorBrush.ColorProperty, backgroundAnimation, HandoffBehavior.SnapshotAndReplace);
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            if (frames > 10)
            {
                mouseMoveFrames++;
                if (mouseMoveFrames > 50)
                {
                    HideScreenSaver();
                }
            }
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            HideScreenSaver();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            timer.Interval = 90;
            timer.Elapsed += new ElapsedEventHandler(DoAnimationStep);
            timer.Enabled = true;

            ColorAnimation backgroundAnimation = new ColorAnimation();
            backgroundAnimation.From = Color.FromArgb(0, 0, 0, 0);
            backgroundAnimation.To = Color.FromArgb(200, 0, 0, 0);
            backgroundAnimation.Duration = new Duration(TimeSpan.FromSeconds(5));
            Background.BeginAnimation(SolidColorBrush.ColorProperty, backgroundAnimation, HandoffBehavior.SnapshotAndReplace);

            xMax = ActualWidth - gruzImage.ActualWidth;
            yMax = ActualHeight - gruzImage.ActualHeight;

            GruzTransformAbsolute(-400, rng.NextDouble() * yMax / 2 + yMax / 4);

            xSpeed = 300;
            ySpeed = gruzImage.Margin.Top < yMax
                ? rng.NextDouble() * -75
                : rng.NextDouble() * 75;

            CompositionTarget.Rendering += ProcessGruzMother;
        }
    }
}
