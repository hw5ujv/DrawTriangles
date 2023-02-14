using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Intrinsics.X86;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
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
            PreviewKeyDown += MainWindow_PreviewKeyDown;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openDialog = new Microsoft.Win32.OpenFileDialog();
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
                if(selectedRectangle != null)
                {
                    var adornerLayer = AdornerLayer.GetAdornerLayer(MyGrid);
                    adornerLayer.Remove(adornerLayer.GetAdorners(selectedRectangle)[0]);
                }
                selectedRectangle = hitRect;
                System.Diagnostics.Debug.WriteLine("selected rectangle" + hitResult.VisualHit);
                AdornerLayer.GetAdornerLayer(MyGrid).Add(new ResizeAdorner(selectedRectangle));
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

                
                if(selectedRectangle != null)
                {
                    var adornerLayer = AdornerLayer.GetAdornerLayer(MyGrid);
                    adornerLayer.Remove(adornerLayer.GetAdorners(selectedRectangle)[0]);
                    selectedRectangle = null;
                }


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
                System.Windows.MessageBox.Show("No image loaded. Please load an image before saving.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            RenderTargetBitmap renderTarget = new RenderTargetBitmap((int)drawingCanvas.ActualWidth, (int)drawingCanvas.ActualHeight, 96, 96, PixelFormats.Pbgra32);
            renderTarget.Render(drawingCanvas);

            PngBitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(renderTarget));

            Microsoft.Win32.SaveFileDialog saveFileDialog = new Microsoft.Win32.SaveFileDialog();
            saveFileDialog.Filter = "PNG image file|*.png";
            if (saveFileDialog.ShowDialog() == true)
            {
                using (FileStream stream = new FileStream(saveFileDialog.FileName, FileMode.Create))
                {
                    encoder.Save(stream);
                }
            }
        }

        private void ChangeColorButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectedRectangle == null)
            {
                System.Windows.MessageBox.Show("No rectangle selected. Please click on a rectangle or create one.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            ColorDialog colorDialog = new ColorDialog();
            if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                // Get the selected color
                System.Drawing.Color selectedColor = colorDialog.Color;

                // Update the color of the selected rectangle
                SolidColorBrush brush = new SolidColorBrush(Color.FromArgb(selectedColor.A, selectedColor.R, selectedColor.G, selectedColor.B));
                selectedRectangle.Fill = brush;
                System.Diagnostics.Debug.WriteLine("Color changed successfully");
               
            }
        }

        private void MainWindow_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Back)
            {
                if (selectedRectangle != null)
                {
                    drawingCanvas.Children.Remove(selectedRectangle);
                    var adornerLayer = AdornerLayer.GetAdornerLayer(MyGrid);
                    adornerLayer.Remove(adornerLayer.GetAdorners(selectedRectangle)[0]);
                    selectedRectangle = null;
                }
            }
        }




        private void Image_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            isMouseDown = false;
            //selectedRectangle = null;
        }
    }



}
