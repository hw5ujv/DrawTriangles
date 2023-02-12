using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Intrinsics.X86;
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
using static System.Net.Mime.MediaTypeNames;

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
        private Rectangle selectedRectangle;
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
            var hitResult = VisualTreeHelper.HitTest(drawingCanvas, startPoint);
            rect = null; // initialize rect with a default value of null
            if (hitResult.VisualHit is Rectangle hitRect)
            {
                selectedRectangle = hitRect;
                isMouseDown = true;
            }
            else
            {
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
            
        }

        private void Image_PreviewMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (!isMouseDown) return;

            var endPoint = e.GetPosition(drawingCanvas);
            if (selectedRectangle != null)
            {
                double left = endPoint.X - selectedRectangle.Width / 2;
                double top = endPoint.Y - selectedRectangle.Height / 2;

                //double left = Canvas.GetLeft(selectedRectangle);
                //double top = Canvas.GetTop(selectedRectangle);
                double width = selectedRectangle.Width;
                double height = selectedRectangle.Height;
                double mouseX = endPoint.X;
                double mouseY = endPoint.Y;
                double resizeThreshold = 5;

                // Check if the rectangle is within the bounds of the image
                if (left >= 0 && left + selectedRectangle.Width <= imagePicture.RenderSize.Width &&
                    top >= 0 && top + selectedRectangle.Height <= imagePicture.RenderSize.Height)
                {
                    Canvas.SetLeft(selectedRectangle, left);
                    Canvas.SetTop(selectedRectangle, top);
                }
            }
            else
            {
                var width = Math.Abs(endPoint.X - startPoint.X);
                var height = Math.Abs(endPoint.Y - startPoint.Y);

                //Canvas.SetLeft(rect, Math.Min(startPoint.X, endPoint.X));
                //Canvas.SetTop(rect, Math.Min(startPoint.Y, endPoint.Y));
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
            
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            //raise exception if there is no image load
            if (imagePicture.Source == null)
            {
                MessageBox.Show("No image loaded. Please load an image before saving.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            RenderTargetBitmap renderTarget = new RenderTargetBitmap((int)drawingCanvas.ActualWidth, (int)drawingCanvas.ActualHeight, 96, 96, PixelFormats.Pbgra32);
            renderTarget.Render(drawingCanvas);

            PngBitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(renderTarget));

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "PNG image file|*.png";
            if (saveFileDialog.ShowDialog() == true)
            {
                using (FileStream stream = new FileStream(saveFileDialog.FileName, FileMode.Create))
                {
                    encoder.Save(stream);
                }
            }
        }
        






        private void Image_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            isMouseDown = false;
            selectedRectangle = null;
        }
    }
}
