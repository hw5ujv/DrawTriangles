using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
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

namespace DrawTriangles
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private bool isMouseDown;
        private Point startPoint;
        private Rectangle rect;
        Brush customColor;
        Random r = new Random();
        
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openDialog = new OpenFileDialog();
            openDialog.Filter = "Image files|*.bmp;*.jpg;*.png";
            openDialog.FilterIndex = 1;
            if(openDialog.ShowDialog() == true)
            {
                drawingCanvas.Children.Clear();
                imagePicture.Source = new BitmapImage(new Uri(openDialog.FileName));
                drawingCanvas.Children.Add(imagePicture);
            }
        }

        private void Image_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            startPoint = e.GetPosition(drawingCanvas);
            customColor = new SolidColorBrush(Color.FromRgb((byte)r.Next(1, 255), (byte)r.Next(1, 255), (byte)r.Next(1, 255)));
            rect = new Rectangle
            {
                Stroke = Brushes.Black,
                Fill = customColor,
                StrokeThickness = 2
            };
            Canvas.SetLeft(rect, startPoint.X);
            Canvas.SetTop(rect, startPoint.Y);
            drawingCanvas.Children.Add(rect);
            isMouseDown = true;
        }

        private void Image_PreviewMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (!isMouseDown) return;

            var endPoint = e.GetPosition(drawingCanvas);
            var width = Math.Abs(endPoint.X - startPoint.X);
            var height = Math.Abs(endPoint.Y - startPoint.Y);

            if (endPoint.X < startPoint.X)
            {
                Canvas.SetLeft(rect, endPoint.X);
            }

            if (endPoint.Y < startPoint.Y)
            {
                Canvas.SetTop(rect, endPoint.Y);
            }

            rect.Width = width;
            rect.Height = height;
        }

        private void Image_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            isMouseDown = false;
        }
    }
}
