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
using System.Windows.Forms;
using System.Drawing;
using System.Windows.Media.Media3D;

namespace AnalizaObrazu
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public Bitmap img;
        int[,,] Matrix;
        int[,,] MatrixZero;
        int[,,,] colorSpace;
        Uri currentImage;
        string currentimage;
        int myExt;
        int originalColors;
        int pmax = 0;
        List<Vector3D> cSpace;

        public MainWindow()
        {
            InitializeComponent();
        }
        private void LoadImage(object sender, RoutedEventArgs e)
        {

            colorSpace = new int[256, 256, 256, 2];

            OpenFileDialog op = new OpenFileDialog();
            op.Title = "Select a picture";
            op.Filter = "Bit Map (*.bmp)|*.bmp|" +
                "JPEG (*.jpg;*.jpeg)|*.jpg;*.jpeg|" +
                "Portable Network Graphic (*.png)|*.png|" +
                "Portable Network Graphic (*.gif)|*.gif";
            if (op.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                System.IO.Stream myStream = null;
                try
                {
                    if ((myStream = op.OpenFile()) != null)
                    {
                        using (myStream)
                        {
                            //currentimage = op.FileName; 
                            currentImage = new Uri(op.FileName);
                            img = new Bitmap(myStream);
                            //displaing pictures
                            this.board.Source = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(img.GetHbitmap(), IntPtr.Zero, System.Windows.Int32Rect.Empty, BitmapSizeOptions.FromWidthAndHeight(img.Width, img.Height));
                            this.board2.Source = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(img.GetHbitmap(), IntPtr.Zero, System.Windows.Int32Rect.Empty, BitmapSizeOptions.FromWidthAndHeight(img.Width, img.Height));
                            myExt = op.FilterIndex;
                            //getting data from picture
                            getRGB();
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show("Error: Could not read file from disk. Original error: " + ex.Message);
                }
                myStream.Dispose();
            }
        }
        public void getRGB()
        {
            Matrix = new int[img.Width, img.Height, 4];
            MatrixZero = new int[img.Width, img.Height, 4];
            cSpace = new List<Vector3D>();
            cSpace.Clear();
            for (int i = 0; i < img.Width; i++)
            {
                for (int j = 0; j < img.Height; j++)
                {
                    System.Drawing.Color pixel = img.GetPixel(i, j);
                    int p = pixel.ToArgb();

                    if (pixel != null)
                    {
                        //picture data
                        Matrix[i, j, 0] = pixel.R;
                        MatrixZero[i, j, 0] = pixel.R;
                        Matrix[i, j, 1] = pixel.G;
                        MatrixZero[i, j, 1] = pixel.G;
                        Matrix[i, j, 2] = pixel.B;
                        MatrixZero[i, j, 2] = pixel.B;
                        Matrix[i, j, 3] = pixel.A;
                        MatrixZero[i, j, 3] = pixel.A;
                        if (pmax < pixel.R)
                            pmax = pixel.R;
                        if (pmax < pixel.G)
                            pmax = pixel.G;
                        if (pmax < pixel.B)
                            pmax = pixel.B;
                        
                        Vector3D asd = new Vector3D(Matrix[i, j, 0], Matrix[i, j, 1], Matrix[i, j, 2]);
                        if (colorSpace[Matrix[i, j, 0], Matrix[i, j, 1], Matrix[i, j, 2], 0] == 0)
                        {
                            cSpace.Add(asd);
                        }
                        colorSpace[Matrix[i, j, 0], Matrix[i, j, 1], Matrix[i, j, 2], 0]++;
                        colorSpace[Matrix[i, j, 0], Matrix[i, j, 1], Matrix[i, j, 2], 1] = 129;
                    }
                }
            }
            originalColors = cSpace.Count;
        }
        private void SaveImageAs(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.FileName = "Image";
            dlg.DefaultExt = ".bmp";
            dlg.Title = "Save As";
            dlg.Filter = "Bitmap Image (.bmp)|*.bmp|" +
                "JPEG (*.jpg)|*.jpg|" +
                "Portable Network Graphic (*.png)|*.png|" +
                "Portable Network Graphic (*.gif)|*.gif";
            if (dlg.ShowDialog() == true)
            {

                if (dlg.FilterIndex == 1)
                {
                    string filename = dlg.FileName;
                    img.Save(filename, System.Drawing.Imaging.ImageFormat.Bmp);
                    currentimage = filename;
                    myExt = dlg.FilterIndex;
                }

                if (dlg.FilterIndex == 2)
                {
                    string filename = dlg.FileName;
                    img.Save(filename, System.Drawing.Imaging.ImageFormat.Jpeg);
                    currentimage = filename;
                    myExt = dlg.FilterIndex;
                }
                if (dlg.FilterIndex == 3)
                {

                    string filename = dlg.FileName;
                    img.Save(filename, System.Drawing.Imaging.ImageFormat.Png);
                    currentimage = filename;
                    myExt = dlg.FilterIndex;
                }
                if (dlg.FilterIndex == 4)
                {

                    string filename = dlg.FileName;
                    img.Save(filename, System.Drawing.Imaging.ImageFormat.Gif);
                    currentimage = filename;
                    myExt = dlg.FilterIndex;
                }
            }
        }

        private void BlackWhite_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < img.Width; i++)
            {
                for (int j = 0; j < img.Height; j++)
                {
                    //picture data
                    int mean = (MatrixZero[i, j, 0] + MatrixZero[i, j, 1] + MatrixZero[i, j, 2]) / 3;
                    Matrix[i, j, 0] = mean;
                    Matrix[i, j, 1] = mean;
                    Matrix[i, j, 2] = mean;

                    System.Drawing.Color c = System.Drawing.Color.FromArgb(Matrix[i, j, 0], Matrix[i, j, 1], Matrix[i, j, 2]);
                    img.SetPixel(i, j, c);
                }
            }
            this.board2.Source = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(img.GetHbitmap(), IntPtr.Zero, System.Windows.Int32Rect.Empty, BitmapSizeOptions.FromWidthAndHeight(img.Width, img.Height));

        }

        private void Odwrocenie_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < img.Width; i++)
            {
                for (int j = 0; j < img.Height; j++)
                {
                    //picture data
                    Matrix[i, j, 0] = 255 - MatrixZero[i, j, 0];
                    Matrix[i, j, 1] = 255 - MatrixZero[i, j, 1];
                    Matrix[i, j, 2] = 255 - MatrixZero[i, j, 2];
                    
                    System.Drawing.Color c = System.Drawing.Color.FromArgb(Matrix[i, j, 0], Matrix[i, j, 1], Matrix[i, j, 2]);
                    img.SetPixel(i, j, c);
                }
            }
            this.board2.Source = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(img.GetHbitmap(), IntPtr.Zero, System.Windows.Int32Rect.Empty, BitmapSizeOptions.FromWidthAndHeight(img.Width, img.Height));

        }

        private void Lightning_Click(object sender, RoutedEventArgs e)
        {
            double o;
            if (double.TryParse(LightCoeficient.Text, out o))
            {
                if (int.Parse(LightCoeficient.Text) >= -255 && int.Parse(LightCoeficient.Text) <= 255)
                {
                    for (int i = 0; i < img.Width; i++)
                    {
                        for (int j = 0; j < img.Height; j++)
                        {
                            //picture data
                            if ((MatrixZero[i, j, 0] + int.Parse(LightCoeficient.Text)) > 255)
                                Matrix[i, j, 0] = 255;
                            else if ((MatrixZero[i, j, 0] + int.Parse(LightCoeficient.Text)) < 0)
                                Matrix[i, j, 0] = 0;
                            else
                                Matrix[i, j, 0] = MatrixZero[i, j, 0] + int.Parse(LightCoeficient.Text);

                            if ((MatrixZero[i, j, 1] + int.Parse(LightCoeficient.Text)) > 255)
                                Matrix[i, j, 1] = 255;
                            else if ((MatrixZero[i, j, 1] + int.Parse(LightCoeficient.Text)) < 0)
                                Matrix[i, j, 0] = 1;
                            else
                                Matrix[i, j, 1] = MatrixZero[i, j, 1] + int.Parse(LightCoeficient.Text);

                            if ((MatrixZero[i, j, 2] + int.Parse(LightCoeficient.Text)) > 255)
                                Matrix[i, j, 2] = 255;
                            else if ((MatrixZero[i, j, 2] + int.Parse(LightCoeficient.Text)) < 0)
                                Matrix[i, j, 2] = 0;
                            else
                                Matrix[i, j, 2] = MatrixZero[i, j, 2] + int.Parse(LightCoeficient.Text);
                            
                            System.Drawing.Color c = System.Drawing.Color.FromArgb(Matrix[i, j, 0], Matrix[i, j, 1], Matrix[i, j, 2]);
                            img.SetPixel(i, j, c);
                        }
                    }
                    this.board2.Source = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(img.GetHbitmap(), IntPtr.Zero, System.Windows.Int32Rect.Empty, BitmapSizeOptions.FromWidthAndHeight(img.Width, img.Height));
                }
                else
                    LightCoeficient.Text = "Zły";
            }
            else
                LightCoeficient.Text = "Zły"; 
        }

        private void ContrastMultiply_Click(object sender, RoutedEventArgs e)
        {
            double o;
            if (double.TryParse(ContrastCoeficient.Text, out o))
            {
                if (double.Parse(ContrastCoeficient.Text) >= 0 && double.Parse(ContrastCoeficient.Text) <= 255)
                {
                    for (int i = 0; i < img.Width; i++)
                    {
                        for (int j = 0; j < img.Height; j++)
                        {
                            //picture data
                            if ((MatrixZero[i, j, 0] * double.Parse(ContrastCoeficient.Text)) > 255)
                                Matrix[i, j, 0] = 255;
                            else if ((MatrixZero[i, j, 0] * double.Parse(ContrastCoeficient.Text)) < 0)
                                Matrix[i, j, 0] = 0;
                            else
                                Matrix[i, j, 0] = (int)(MatrixZero[i, j, 0] * double.Parse(ContrastCoeficient.Text));

                            if ((MatrixZero[i, j, 1] * double.Parse(ContrastCoeficient.Text)) > 255)
                                Matrix[i, j, 1] = 255;
                            else if ((MatrixZero[i, j, 1] * double.Parse(ContrastCoeficient.Text)) < 0)
                                Matrix[i, j, 0] = 1;
                            else
                                Matrix[i, j, 1] = (int)(MatrixZero[i, j, 1] * double.Parse(ContrastCoeficient.Text));

                            if ((MatrixZero[i, j, 2] * double.Parse(ContrastCoeficient.Text)) > 255)
                                Matrix[i, j, 2] = 255;
                            else if ((MatrixZero[i, j, 2] * double.Parse(ContrastCoeficient.Text)) < 0)
                                Matrix[i, j, 2] = 0;
                            else
                                Matrix[i, j, 2] = (int)(MatrixZero[i, j, 2] * double.Parse(ContrastCoeficient.Text));
                           
                            System.Drawing.Color c = System.Drawing.Color.FromArgb(Matrix[i, j, 0], Matrix[i, j, 1], Matrix[i, j, 2]);
                            img.SetPixel(i, j, c);
                        }
                    }
                    this.board2.Source = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(img.GetHbitmap(), IntPtr.Zero, System.Windows.Int32Rect.Empty, BitmapSizeOptions.FromWidthAndHeight(img.Width, img.Height));
                }
                else
                    ContrastCoeficient.Text = "Zły";
            }
            else
                ContrastCoeficient.Text = "Zły";
        }

        private void ContrastPower_Click(object sender, RoutedEventArgs e)
        {
            double o;
            if (double.TryParse(ContrastCoeficient.Text, out o))
            {
                if (double.Parse(ContrastCoeficient.Text) >= 0 && double.Parse(ContrastCoeficient.Text) <= 255)
                {
                    for (int i = 0; i < img.Width; i++)
                    {
                        for (int j = 0; j < img.Height; j++)
                        {
                            //picture data
                            if ((255 * Math.Pow(((double)MatrixZero[i, j, 0] / pmax), double.Parse(ContrastCoeficient.Text))) > 255)
                                Matrix[i, j, 0] = 255;
                            else if ((255 * Math.Pow(((double)MatrixZero[i, j, 0] / pmax), double.Parse(ContrastCoeficient.Text))) < 0)
                                Matrix[i, j, 0] = 0;
                            else
                                Matrix[i, j, 0] = (int)(255 * Math.Pow(((double)MatrixZero[i, j, 0]/pmax), double.Parse(ContrastCoeficient.Text)));

                            if ((255 * Math.Pow(((double)MatrixZero[i, j, 1] / pmax), double.Parse(ContrastCoeficient.Text))) > 255)
                                Matrix[i, j, 1] = 255;
                            else if ((255 * Math.Pow(((double)MatrixZero[i, j, 1] / pmax), double.Parse(ContrastCoeficient.Text))) < 0)
                                Matrix[i, j, 0] = 1;
                            else
                                Matrix[i, j, 1] = (int)(255 * Math.Pow(((double)MatrixZero[i, j, 1] / pmax), double.Parse(ContrastCoeficient.Text)));

                            if ((255 * Math.Pow(((double)MatrixZero[i, j, 2] / pmax), double.Parse(ContrastCoeficient.Text))) > 255)
                                Matrix[i, j, 2] = 255;
                            else if ((255 * Math.Pow(((double)MatrixZero[i, j, 2] / pmax), double.Parse(ContrastCoeficient.Text))) < 0)
                                Matrix[i, j, 2] = 0;
                            else
                                Matrix[i, j, 2] = (int)(255 * Math.Pow(((double)MatrixZero[i, j, 2] / pmax), double.Parse(ContrastCoeficient.Text)));

                            System.Drawing.Color c = System.Drawing.Color.FromArgb(Matrix[i, j, 0], Matrix[i, j, 1], Matrix[i, j, 2]);
                            img.SetPixel(i, j, c);
                        }
                    }
                    this.board2.Source = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(img.GetHbitmap(), IntPtr.Zero, System.Windows.Int32Rect.Empty, BitmapSizeOptions.FromWidthAndHeight(img.Width, img.Height));
                }
                else
                    ContrastCoeficient.Text = "Zły";
            }
            else
                ContrastCoeficient.Text = "Zły";
        }

        private void ContrastLogarithm_Click(object sender, RoutedEventArgs e)
        {
            double o;
            if (double.TryParse(ContrastCoeficient.Text, out o))
            {
                if (double.Parse(ContrastCoeficient.Text) >= 0 && double.Parse(ContrastCoeficient.Text) <= 255)
                {
                    for (int i = 0; i < img.Width; i++)
                    {
                        for (int j = 0; j < img.Height; j++)
                        {
                            //picture data
                            if ((255 * Math.Log(((1 + MatrixZero[i, j, 0])) / Math.Log(1 + pmax))) > 255)
                                Matrix[i, j, 0] = 255;
                            else if ((255 * Math.Log(((1 + MatrixZero[i, j, 0])) / Math.Log(1 + pmax))) < 0)
                                Matrix[i, j, 0] = 0;
                            else
                                Matrix[i, j, 0] = (int)(255 * Math.Log(((1 + MatrixZero[i, j, 0])) / Math.Log(1 + pmax)));

                            if ((255 * Math.Log(((1 + MatrixZero[i, j, 1])) / Math.Log(1 + pmax))) > 255)
                                Matrix[i, j, 1] = 255;
                            else if ((255 * Math.Log(((1 + MatrixZero[i, j, 1])) / Math.Log(1 + pmax))) < 0)
                                Matrix[i, j, 0] = 1;
                            else
                                Matrix[i, j, 1] = (int)(255 * Math.Log(((1 + MatrixZero[i, j, 1])) / Math.Log(1 + pmax)));

                            if ((255 * Math.Log(((1 + MatrixZero[i, j, 2])) / Math.Log(1 + pmax))) > 255)
                                Matrix[i, j, 2] = 255;
                            else if ((255 * Math.Log(((1 + MatrixZero[i, j, 2])) / Math.Log(1 + pmax))) < 0)
                                Matrix[i, j, 2] = 0;
                            else
                                Matrix[i, j, 2] = (int)(255 * Math.Log(((1 + MatrixZero[i, j, 2])) / Math.Log(1 + pmax)));

                            System.Drawing.Color c = System.Drawing.Color.FromArgb(Matrix[i, j, 0], Matrix[i, j, 1], Matrix[i, j, 2]);
                            img.SetPixel(i, j, c);
                        }
                    }
                    this.board2.Source = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(img.GetHbitmap(), IntPtr.Zero, System.Windows.Int32Rect.Empty, BitmapSizeOptions.FromWidthAndHeight(img.Width, img.Height));
                }
                else
                    ContrastCoeficient.Text = "Zły";
            }
            else
                ContrastCoeficient.Text = "Zły";
        }

        private void ProgowanieSlider_Click(object sender, RoutedEventArgs e)
        {
            double o;
            if (double.TryParse(ContrastCoeficient.Text, out o))
            {
                if (double.Parse(ContrastCoeficient.Text) >= 0 && double.Parse(ContrastCoeficient.Text) <= 255)
                {
                    for (int i = 0; i < img.Width; i++)
                    {
                        for (int j = 0; j < img.Height; j++)
                        {
                            //picture data
                            if ((MatrixZero[i, j, 0] < (int)slider.Value) || (MatrixZero[i, j, 1] < (int)slider.Value) || (MatrixZero[i, j, 2] < (int)slider.Value))
                            {
                                Matrix[i, j, 0] = 0;
                                Matrix[i, j, 1] = 0;
                                Matrix[i, j, 2] = 0;
                            }
                            else
                            {
                                Matrix[i, j, 0] = 255;
                                Matrix[i, j, 1] = 255;
                                Matrix[i, j, 2] = 255;
                            }

                            System.Drawing.Color c = System.Drawing.Color.FromArgb(Matrix[i, j, 0], Matrix[i, j, 1], Matrix[i, j, 2]);
                            img.SetPixel(i, j, c);
                        }
                    }
                    this.board2.Source = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(img.GetHbitmap(), IntPtr.Zero, System.Windows.Int32Rect.Empty, BitmapSizeOptions.FromWidthAndHeight(img.Width, img.Height));
                }
                else
                    ContrastCoeficient.Text = "Zły";
            }
            else
                ContrastCoeficient.Text = "Zły";
        }

        private void MetodaOdsu_Click(object sender, RoutedEventArgs e)
        {

        }

        private void MetodaBarensa_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
