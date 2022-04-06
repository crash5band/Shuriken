using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Shuriken.Models.Animation;

namespace Shuriken.Controls
{
    /// <summary>
    /// Interaction logic for AnimationTimeline.xaml
    /// </summary>
    public partial class AnimationTimeline : UserControl, INotifyPropertyChanged
    {
        private Brush lineStroke = SystemColors.GrayTextBrush;
        private Brush keyBrush = new SolidColorBrush(Color.FromRgb(255, 50, 50));
        private Brush keyStrokeBrush = new SolidColorBrush(Color.FromRgb(255, 255, 255));
        private Brush curveBrush = new SolidColorBrush(Color.FromRgb(255, 255, 255));
        private const double frameMargin = 20.0;
        private const double xOffset = 100.0;

        private Line cursor;

        private int currentFrame = 0;
        public int CurrentFrame
        {
            get { return currentFrame; }
            set { currentFrame = value; }
        }

        private double zoom = 1.5;
        public double Zoom
        {
            get { return zoom; }
            set { zoom = value;DrawTimeline(); }
        }
        public double MinZoom => 0.5;
        public double MaxZoom => 3.0;

        private AnimationTrack track;

        private Keyframe keyframe;
        public Keyframe SelectedKey
        {
            get { return keyframe; }
            set { keyframe = value;NotifyPropertyChanged("KeySelected"); }
        }

        public bool KeySelected
        {
            get { return keyframe != null; }
        }

        private double minValue;
        public double MinValue
        {
            get { return minValue; }
            set { minValue = value;DrawTimeline(); }
        }

        private double maxValue;
        public double MaxValue
        {
            get { return maxValue; }
            set { maxValue = value;DrawTimeline(); }
        }

        private bool holdingKey;

        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<AnimationGroup> Animations
        {
            get => (ObservableCollection<AnimationGroup>)GetValue(AnimationProperty);
            set => SetValue(AnimationProperty, value);
        }

        public static readonly DependencyProperty AnimationProperty = DependencyProperty.Register(
            "Animations", typeof(ObservableCollection<AnimationGroup>), typeof(AnimationTimeline), new PropertyMetadata(null));
        public AnimationTimeline()
        {
            InitializeComponent();
            Animations = new ObservableCollection<AnimationGroup>();
            LayoutRoot.DataContext = this;

            Timeline.Height = TimelineContainer.ActualHeight;

            cursor = GetFrameLine(CurrentFrame);
            cursor.Stroke = new SolidColorBrush(Color.FromRgb(221, 82, 70));
            cursor.StrokeThickness = 2;

            MinValue = 0;
            MaxValue = 100;
            holdingKey = false;

            UpdateValueEditor();
            DrawTimeline();
        }

        private void UpdateValueEditor()
        {
            if (track != null && track.Type.IsColor())
            {
                FrameValueColor.Visibility = Visibility.Visible;
                FrameValueText.Visibility = Visibility.Collapsed;
            }
            else
            {
                FrameValueColor.Visibility = Visibility.Collapsed;
                FrameValueText.Visibility = Visibility.Visible;
            }
        }

        private Line GetFrameLine(int frame)
        {
            Line frameLine = new Line();
            frameLine.X1 = GetFrameXPos(frame);
            frameLine.Y1 = GetFrameYPos(frame);
            frameLine.X2 = GetFrameXPos(frame);
            frameLine.Y2 = Timeline.Height;
            frameLine.Stroke = lineStroke;
            frameLine.StrokeThickness = frame % 2 == 0 ? 1.0 : 0.75;

            return frameLine;
        }

        private double GetFrameXPos(int frame)
        {
            return (frame * frameMargin * zoom) + xOffset;
        }

        private double GetFrameYPos(int frame)
        {
            return frame % 2 == 0 ? 10 : 0;
        }

        private TextBlock GetFrameLabel(int frame, double x, double y)
        {
            TextBlock frameText = new TextBlock();
            frameText.Text = frame.ToString();
            frameText.FontSize = 10;
            frameText.HorizontalAlignment = HorizontalAlignment.Center;
            Canvas.SetLeft(frameText, x);
            Canvas.SetTop(frameText, y);

            return frameText;
        }

        private TextBlock GetValueLabel(double fraction, double max, double min)
        {
            TextBlock lbl = new TextBlock();
            lbl.FontSize = 10;
            lbl.Text = (max -  (1 - fraction) * (max - min)).ToString();
            lbl.VerticalAlignment = VerticalAlignment.Center;
            return lbl;
        }

        private Line GetValueLine(double fraction)
        {
            Line line = new Line();
            line.X1 = 0;
            line.X2 = Timeline.Width;
            line.Y1 = line.Y2 = Timeline.Height - (Timeline.Height * fraction);
            line.Stroke = lineStroke;
            line.StrokeThickness = line.Y1 == 0 ? 4 : fraction == 0.5 ? 1 : 0.5;

            return line;
        }

        private double ValueToHeight(float value, double minValue, double maxValue)
        {
            double percent = 1 - ((value - minValue) / (maxValue - minValue));
            return percent * Timeline.Height;
        }

        private double HeightToValue(double height, double minValue, double maxValue)
        {
            double percent = height / (Timeline.Height);
            return ((1 - percent) * (maxValue - minValue)) + minValue;
        }

        private void ResizeCursor()
        {
            cursor.Y1 = 0;
            cursor.Y2 = Timeline.Height;
        }

        private Line GetCurve(Keyframe k1, Keyframe k2)
        {
            double x1 = GetFrameXPos(k1.Frame);
            double x2 = GetFrameXPos(k2.Frame);
            double y1 = ValueToHeight(k1.KValue, minValue, maxValue);
            double y2 = ValueToHeight(k2.KValue, minValue, maxValue);

            Line line = new Line();
            line.X1 = x1;
            line.X2 = x2;
            line.Y1 = y1;
            line.Y2 = y2;
            line.Stroke = curveBrush;
            line.StrokeThickness = 2.0;

            return line;
        }

        private void DrawKeyframes()
        {
            if (track != null)
            {
                for (int k = 0; k < track.Keyframes.Count; ++k)
                {
                    Ellipse c = new Ellipse();
                    c.Width = c.Height = 10;
                    c.Fill = keyBrush;
                    c.Stroke = keyStrokeBrush;
                    c.StrokeThickness = 1;
                    c.MouseLeftButtonDown += KeyMouseDown;
                    c.MouseMove += KeyMouseMove;
                    c.MouseLeftButtonUp += KeyMouseUp;

                    Canvas.SetLeft(c, GetFrameXPos(track.Keyframes[k].Frame) - (c.Width / 2));
                    Canvas.SetTop(c, ValueToHeight(track.Keyframes[k].KValue, minValue, maxValue));

                    if (k < track.Keyframes.Count - 1)
                    {
                        Line line = GetCurve(track.Keyframes[k], track.Keyframes[k + 1]);
                        line.X1 -= c.Width / 2;
                        line.X2 -= c.Width / 2;
                        line.Y1 += c.Height / 2;
                        line.Y2 += c.Height / 2;
                        //Timeline.Children.Add(line);
                    }

                    Timeline.Children.Add(c);
                }
            }
        }

        private void DrawTimeline()
        {
            Timeline.Children.Clear();
            int maxFrame = 400;
            Timeline.Width = (maxFrame * frameMargin * zoom) + xOffset;
            Timeline.Height = TimelineContainer.ActualHeight;
            if (Timeline.Height > 0)
                Timeline.Height -= 20.0;

            for (int i = 0; i < maxFrame; ++i)
            {
                Line frameLine = GetFrameLine(i);
                Timeline.Children.Add(frameLine);

                if (i % 10 == 0)
                {
                    TextBlock frameText = GetFrameLabel(i, frameLine.X1, frameLine.Y1 - 10);
                    Timeline.Children.Add(frameText);
                }
            }

            for (int i = 4; i > 0; --i)
            {
                double fraction = i * 0.25;
                Line valueLine = GetValueLine(fraction);
                TextBlock valueLabel = GetValueLabel(fraction, maxValue, minValue);

                Canvas.SetTop(valueLabel, valueLine.Y1);

                Timeline.Children.Add(valueLine);
                Timeline.Children.Add(valueLabel);
            }

            DrawKeyframes();

            // draw cursor
            Timeline.Children.Add(cursor);
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            DrawTimeline();
            ResizeCursor();
        }

        private void TimelineMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!holdingKey)
            {
                Point pos = e.GetPosition(Timeline);
                int frame = (int)Math.Round((pos.X - xOffset) / frameMargin / zoom);
                CurrentFrame = Math.Clamp(frame, 0, 400);
                cursor.X1 = GetFrameXPos(CurrentFrame);
                cursor.X2 = GetFrameXPos(CurrentFrame);

                ScanKeyframe();
            }
        }

        private void ScanKeyframe()
        {
            if (track != null)
            {
                foreach (var key in track.Keyframes)
                {
                    if (key.Frame == currentFrame)
                    {
                        SelectedKey = key;
                        return;
                    }
                }
            }

            SelectedKey = null;
        }

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void AnimationTreeViewSelected(object sender, RoutedEventArgs e)
        {
            TreeViewItem item = e.OriginalSource as TreeViewItem;
            if (item.DataContext is AnimationTrack)
            {
                track = item.DataContext as AnimationTrack;
            }
            else
            {
                track = null;
            }

            ScanKeyframe();
            UpdateValueEditor();
            DrawTimeline();
        }

        private void KeyMouseDown(object sender, MouseButtonEventArgs e)
        {
            var key = sender as Ellipse;
            if (key.CaptureMouse())
            {
                holdingKey = true;
                Point mousePos = e.GetPosition(Timeline);
                int frame = (int)Math.Round((mousePos.X - xOffset) / frameMargin / zoom);
                CurrentFrame = Math.Clamp(frame, 0, 400);

                ScanKeyframe();
            }
        }

        private void KeyMouseMove(object sender, MouseEventArgs e)
        {
            if (holdingKey)
            {
                var key = sender as Ellipse;
                Point delta = e.GetPosition(key);

                double xPos = Canvas.GetLeft(key);
                double yPos = Canvas.GetTop(key);
                xPos += delta.X;
                yPos += delta.Y;

                int frame = (int)Math.Round((xPos - xOffset) / frameMargin / zoom);
                SelectedKey.Frame = Math.Clamp(frame, 0, 400);
                SelectedKey.KValue = (float)HeightToValue(yPos, minValue, maxValue);

                Canvas.SetLeft(key, GetFrameXPos(SelectedKey.Frame) - (key.Width / 2));
                Canvas.SetTop(key, (float)ValueToHeight(SelectedKey.KValue, minValue, maxValue) - (key.Height / 2));
            }
        }

        private void KeyMouseUp(object sender, MouseButtonEventArgs e)
        {
            var key = sender as Ellipse;
            key.ReleaseMouseCapture();
            holdingKey = false;
        }
    }
}
