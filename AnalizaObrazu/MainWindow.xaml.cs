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
using System.IO;

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
        int pmin = 255;
        int pgmax = 0;
        int pgmin = 255;
        int[] hist_h;
        int[] hist_v;
        int[] hist_c;
        int[] hist_c_mod;
        int threshold;
        List<Vector3D> cSpace;
        int[,] MBlur = new int[3, 3];
        int dBlur = 9;
        int[,] MGaussianSmoothing = new int[3, 3];
        int dGaussianSmoothing = 16;
        int[,] MSharpenFilter = new int[3, 3];
        int[,] MLaplasjanFilter = new int[3, 3];
        int[,] MEdgeDetectionLeft = new int[3, 3];
        int[,] MEdgeDetectionRight = new int[3, 3];


        public MainWindow()
        {
            InitializeComponent();
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    MBlur[i, j] = 1;
                    MGaussianSmoothing[i, j] = 1;
                    MSharpenFilter[i, j] = 1;
                    MEdgeDetectionLeft[i, j] = 0;
                    MEdgeDetectionRight[i, j] = 0;
                    MLaplasjanFilter[i, j] = -1;
                }
            }
            MGaussianSmoothing[1, 0] = 2;
            MGaussianSmoothing[0, 1] = 2;
            MGaussianSmoothing[2, 1] = 2;
            MGaussianSmoothing[1, 2] = 2;
            MGaussianSmoothing[1, 1] = 4;

            MSharpenFilter[1, 0] = -2;
            MSharpenFilter[0, 1] = -2;
            MSharpenFilter[2, 1] = -2;
            MSharpenFilter[1, 2] = -2;
            MSharpenFilter[1, 1] = 6;

            MLaplasjanFilter[1, 0] = -2;
            MLaplasjanFilter[0, 1] = -2;
            MLaplasjanFilter[2, 1] = -2;
            MLaplasjanFilter[1, 2] = -2;
            MLaplasjanFilter[1, 1] = 13;

            MEdgeDetectionLeft[0, 0] = 1;
            MEdgeDetectionLeft[1, 1] = -1;
            MEdgeDetectionLeft[0, 1] = 1;
            MEdgeDetectionLeft[1, 0] = -1;
        }

        private void LoadImage(object sender, RoutedEventArgs e)
        {

            colorSpace = new int[256, 256, 256, 2];

            OpenFileDialog op = new OpenFileDialog();
            op.Title = "Select a picture";
            op.Filter = 
                "JPEG (*.jpg;*.jpeg)|*.jpg;*.jpeg|" +
                "Bit Map (*.bmp)|*.bmp|" +
                "Portable Network Graphic (*.png)|*.png|" +
                "Portable Network Graphic (*.gif)|*.gif";
            op.DefaultExt = ".jpg";
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
                            getColorHistogram();
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

        public void getColorHistogram()
        {
            hist_c = new int[256];
            for (int i = 0; i < img.Width; i++)
            {
                for (int j = 0; j < img.Height; j++)
                {
                    for (int c = 0; c < 256; c++)
                    {
                        int mean = (MatrixZero[i, j, 0] + MatrixZero[i, j, 1] + MatrixZero[i, j, 2]) / 3;

                        if (mean == c)
                        {
                            hist_c[c]++;
                        }
                    }
                }
            }
        }

        public void histTranslation()
        {
            for (int i = 0; i < img.Width; i++)
            {
                for (int j = 0; j < img.Height; j++)
                {
                    if (hist_c[MatrixZero[i, j, 0]]>0 || hist_c[MatrixZero[i, j, 1]]>0 || hist_c[MatrixZero[i, j, 2]]>0)
                    {
                        int mean = (MatrixZero[i, j, 0] + MatrixZero[i, j, 1] + MatrixZero[i, j, 2]) /3;
                        double d = (255 * (double)((double)hist_c_mod[mean] / (double)hist_c[mean]));

                        Matrix[i, j, 0] = (int)d;
                        Matrix[i, j, 1] = (int)d;
                        Matrix[i, j, 2] = (int)d;

                    }
                    else
                    {
                        Matrix[i, j, 0] = 0;
                        Matrix[i, j, 1] = 0;
                        Matrix[i, j, 2] = 0;
                    }
                }
            }
        }

        public void getRGB()
        {
            Matrix = new int[img.Width, img.Height, 4];
            MatrixZero = new int[img.Width, img.Height, 4];
            pmax = 0;
            pgmax = 0;
            pmin = 255;
            pgmin = 255;
            hist_h = new int[img.Width];
            hist_v = new int[img.Height];
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
                        int mean = (pixel.R + pixel.G + pixel.B) / 3;
                        if (mean > 0 && pmin > mean)
                            pmin = mean;
                        if (pmax < mean)
                            pmax = mean;

                        if (pixel.R > 0 && pgmin > pixel.R)
                            pgmin = pixel.R;
                        if (pgmax < pixel.R)
                            pgmax = pixel.R;

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
                                Matrix[i, j, 1] = 0;
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
                            int mean = (MatrixZero[i, j, 0] + MatrixZero[i, j, 1] + MatrixZero[i, j, 2]) / 3;

                            if (meanInput.IsChecked == true)
                                {
                                if (mean < (int)slider.Value)
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
                            }
                            else
                            {
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
            int total = img.Width * img.Height;

            double sum = 0;
            for (int t = 0; t < 256; t++) sum += t * hist_c[t];

            double sumB = 0;
            int getB = 0;
            int getF = 0;

            double Max = 0;
            threshold = 0;
            for (int t = 0; t < 256; t++)
            {
                // Background
                getB += hist_c[t];               
                if (getB == 0) continue;
                // Foreground
                getF = total - getB;                 
                if (getF == 0) break;

                //Background sum
                sumB += (double)(t * hist_c[t]);

                //Means calculation
                double meanB = sumB / getB;
                double meanF = (sum - sumB) / getF;

                // Variance between foreground and background
                double v = (double)getB * (double)getF * (meanB - meanF) * (meanB - meanF);

                // Check if new maximum found
                if (v > Max)
                {
                    Max = v;
                    threshold = t;
                }
            }
            for (int i = 0; i < img.Width; i++)
            {
                for (int j = 0; j < img.Height; j++)
                {
                    //picture data
                    int mean = (MatrixZero[i, j, 0] + MatrixZero[i, j, 1] + MatrixZero[i, j, 2]) / 3;

                    if (meanInput.IsChecked == true)
                    {
                        if (mean < threshold)
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
                    }
                    else
                    {
                        if ((MatrixZero[i, j, 0] < threshold) || (MatrixZero[i, j, 1] < threshold) || (MatrixZero[i, j, 2] < threshold))
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
                    }

                    System.Drawing.Color c = System.Drawing.Color.FromArgb(Matrix[i, j, 0], Matrix[i, j, 1], Matrix[i, j, 2]);
                    img.SetPixel(i, j, c);
                }
            }
            this.board2.Source = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(img.GetHbitmap(), IntPtr.Zero, System.Windows.Int32Rect.Empty, BitmapSizeOptions.FromWidthAndHeight(img.Width, img.Height));

        }
        
        private void SaveHistogram_Click(object sender, RoutedEventArgs e)
        {
            string name = HistogramName.Text;
            string filePath = name + "_h.csv";
            if (!File.Exists(filePath))
            {
                File.Create(filePath).Close();
            }
            string delimter = ";";
            //flexible part ... add as many object as you want based on your app logic
            
            int length = hist_h.Length;

            using (System.IO.TextWriter writer = File.CreateText(filePath))
            {
                writer.WriteLine("Numer" + delimter + "Wartosc");
                for (int index = 0; index < length; index++)
                {
                    writer.WriteLine(index + delimter + hist_h[index]);
                }
            }

            filePath = name + "_v.csv";
            if (!File.Exists(filePath))
            {
                File.Create(filePath).Close();
            }

            length = hist_v.Length;

            using (System.IO.TextWriter writer = File.CreateText(filePath))
            {
                writer.WriteLine("Numer" + delimter + "Wartosc");
                for (int index = 0; index < length; index++)
                {
                    writer.WriteLine(index + delimter + hist_v[index]);
                }
            }
        }

        private void GenerateHistogram_Click(object sender, RoutedEventArgs e)
        {

            for (int j = 0; j < img.Height; j++)
            {
                hist_v[j] = 0;
            }
            for (int i = 0; i < img.Width; i++)
            {
                hist_h[i] = 0;
            }
            for (int i = 0; i < img.Width; i++)
            {
                for (int j = 0; j < img.Height; j++)
                {
                    //picture data
                    int mean = (MatrixZero[i, j, 0] + MatrixZero[i, j, 1] + MatrixZero[i, j, 2]) / 3;

                    if (mean < 128)
                    {
                        Matrix[i, j, 0] = 0;
                        Matrix[i, j, 1] = 0;
                        Matrix[i, j, 2] = 0;
                        hist_h[i]++;
                        hist_v[j]++;
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

        public void Convolution(int[,] f, int d, int offset)
    {
        for (int i = 0; i < img.Width; i++)
        {
            for (int j = 0; j < img.Height; j++)
            {
                int a1 = 0;
                int a2 = 0;
                int a3 = 0;
                int a0 = 0;
                for (int k = -1; k <= 1; k++)
                {
                    for (int l = -1; l <= 1; l++)
                    {

                        if (i == 0 && k == -1 && j == 0 && l == -1)
                        {
                            a0 += (f[k + 1, l + 1] * MatrixZero[i + 1 + k, j + 1 + l, 3]);
                            a1 += (f[k + 1, l + 1] * MatrixZero[i + 1 + k, j + 1 + l, 0]);
                            a2 += (f[k + 1, l + 1] * MatrixZero[i + 1 + k, j + 1 + l, 1]);
                            a3 += (f[k + 1, l + 1] * MatrixZero[i + 1 + k, j + 1 + l, 2]);
                        }
                        else if (i == 0 && k == -1 && j == img.Height - 1 && l == 1)
                        {
                            a0 += (f[k + 1, l + 1] * MatrixZero[i + 1 + k, j - 1 + l, 3]);
                            a1 += (f[k + 1, l + 1] * MatrixZero[i + 1 + k, j - 1 + l, 0]);
                            a2 += (f[k + 1, l + 1] * MatrixZero[i + 1 + k, j - 1 + l, 1]);
                            a3 += (f[k + 1, l + 1] * MatrixZero[i + 1 + k, j - 1 + l, 2]);
                        }
                        else if (i == img.Width - 1 && k == 1 && j == 0 && l == -1)
                        {
                            a0 += (f[k + 1, l + 1] * MatrixZero[i - 1 + k, j + 1 + l, 3]);
                            a1 += (f[k + 1, l + 1] * MatrixZero[i - 1 + k, j + 1 + l, 0]);
                            a2 += (f[k + 1, l + 1] * MatrixZero[i - 1 + k, j + 1 + l, 1]);
                            a3 += (f[k + 1, l + 1] * MatrixZero[i - 1 + k, j + 1 + l, 2]);
                        }
                        else if (i == img.Width - 1 && k == 1 && j == img.Height - 1 && l == 1)
                        {
                            a0 += (f[k + 1, l + 1] * MatrixZero[i - 1 + k, j - 1 + l, 3]);
                            a1 += (f[k + 1, l + 1] * MatrixZero[i - 1 + k, j - 1 + l, 0]);
                            a2 += (f[k + 1, l + 1] * MatrixZero[i - 1 + k, j - 1 + l, 1]);
                            a3 += (f[k + 1, l + 1] * MatrixZero[i - 1 + k, j - 1 + l, 2]);
                        }
                        else if (i == 0 && k == -1)
                        {
                            a0 += (f[k + 1, l + 1] * MatrixZero[i + 1 + k, j + l, 3]);
                            a1 += (f[k + 1, l + 1] * MatrixZero[i + 1 + k, j + l, 0]);
                            a2 += (f[k + 1, l + 1] * MatrixZero[i + 1 + k, j + l, 1]);
                            a3 += (f[k + 1, l + 1] * MatrixZero[i + 1 + k, j + l, 2]);
                        }
                        else if (j == 0 && l == -1)
                        {
                            a0 += (f[k + 1, l + 1] * MatrixZero[i + k, j + 1 + l, 3]);
                            a1 += (f[k + 1, l + 1] * MatrixZero[i + k, j + 1 + l, 0]);
                            a2 += (f[k + 1, l + 1] * MatrixZero[i + k, j + 1 + l, 1]);
                            a3 += (f[k + 1, l + 1] * MatrixZero[i + k, j + 1 + l, 2]);
                        }
                        else if (i == img.Width - 1 && k == 1)
                        {
                            a0 += (f[k + 1, l + 1] * MatrixZero[i - 1 + k, j + l, 3]);
                            a1 += (f[k + 1, l + 1] * MatrixZero[i - 1 + k, j + l, 0]);
                            a2 += (f[k + 1, l + 1] * MatrixZero[i - 1 + k, j + l, 1]);
                            a3 += (f[k + 1, l + 1] * MatrixZero[i - 1 + k, j + l, 2]);
                        }
                        else if (j == img.Height - 1 && l == 1)
                        {
                            a0 += (f[k + 1, l + 1] * MatrixZero[i + k, j - 1 + l, 3]);
                            a1 += (f[k + 1, l + 1] * MatrixZero[i + k, j - 1 + l, 0]);
                            a2 += (f[k + 1, l + 1] * MatrixZero[i + k, j - 1 + l, 1]);
                            a3 += (f[k + 1, l + 1] * MatrixZero[i + k, j - 1 + l, 2]);
                        }
                        else
                        {
                            a0 += (f[k + 1, l + 1] * MatrixZero[i + k, j + l, 3]);
                            a1 += (f[k + 1, l + 1] * MatrixZero[i + k, j + l, 0]);
                            a2 += (f[k + 1, l + 1] * MatrixZero[i + k, j + l, 1]);
                            a3 += (f[k + 1, l + 1] * MatrixZero[i + k, j + l, 2]);
                        }
                    }
                }
                Matrix[i, j, 0] = ((a1 / d) + offset);
                Matrix[i, j, 1] = ((a2 / d) + offset);
                Matrix[i, j, 2] = ((a3 / d) + offset);
                Matrix[i, j, 3] = ((a0 / d) + offset);
            }
        }
    }

        public void ConvolutionRobertsCross(int[,] f1, int[,] f, int d, int offset)
        {
            for (int i = 0; i < img.Width; i++)
            {
                for (int j = 0; j < img.Height; j++)
                {
                    int a1 = 0;
                    int a2 = 0;
                    int a3 = 0;
                    int a0 = 0;
                    for (int k = -1; k <= 1; k++)
                    {
                        for (int l = -1; l <= 1; l++)
                        {

                            if (i == 0 && k == -1 && j == 0 && l == -1)
                            {
                                a0 += (f1[k + 1, l + 1] * MatrixZero[i + 1 + k, j + 1 + l, 3]);
                                a1 += (f1[k + 1, l + 1] * MatrixZero[i + 1 + k, j + 1 + l, 0]);
                                a2 += (f1[k + 1, l + 1] * MatrixZero[i + 1 + k, j + 1 + l, 1]);
                                a3 += (f1[k + 1, l + 1] * MatrixZero[i + 1 + k, j + 1 + l, 2]);
                            }
                            else if (i == 0 && k == -1 && j == img.Height - 1 && l == 1)
                            {
                                a0 += (f1[k + 1, l + 1] * MatrixZero[i + 1 + k, j - 1 + l, 3]);
                                a1 += (f1[k + 1, l + 1] * MatrixZero[i + 1 + k, j - 1 + l, 0]);
                                a2 += (f1[k + 1, l + 1] * MatrixZero[i + 1 + k, j - 1 + l, 1]);
                                a3 += (f1[k + 1, l + 1] * MatrixZero[i + 1 + k, j - 1 + l, 2]);
                            }
                            else if (i == img.Width - 1 && k == 1 && j == 0 && l == -1)
                            {
                                a0 += (f1[k + 1, l + 1] * MatrixZero[i - 1 + k, j + 1 + l, 3]);
                                a1 += (f1[k + 1, l + 1] * MatrixZero[i - 1 + k, j + 1 + l, 0]);
                                a2 += (f1[k + 1, l + 1] * MatrixZero[i - 1 + k, j + 1 + l, 1]);
                                a3 += (f1[k + 1, l + 1] * MatrixZero[i - 1 + k, j + 1 + l, 2]);
                            }
                            else if (i == img.Width - 1 && k == 1 && j == img.Height - 1 && l == 1)
                            {
                                a0 += (f1[k + 1, l + 1] * MatrixZero[i - 1 + k, j - 1 + l, 3]);
                                a1 += (f1[k + 1, l + 1] * MatrixZero[i - 1 + k, j - 1 + l, 0]);
                                a2 += (f1[k + 1, l + 1] * MatrixZero[i - 1 + k, j - 1 + l, 1]);
                                a3 += (f1[k + 1, l + 1] * MatrixZero[i - 1 + k, j - 1 + l, 2]);
                            }
                            else if (i == 0 && k == -1)
                            {
                                a0 += (f1[k + 1, l + 1] * MatrixZero[i + 1 + k, j + l, 3]);
                                a1 += (f1[k + 1, l + 1] * MatrixZero[i + 1 + k, j + l, 0]);
                                a2 += (f1[k + 1, l + 1] * MatrixZero[i + 1 + k, j + l, 1]);
                                a3 += (f1[k + 1, l + 1] * MatrixZero[i + 1 + k, j + l, 2]);
                            }
                            else if (j == 0 && l == -1)
                            {
                                a0 += (f1[k + 1, l + 1] * MatrixZero[i + k, j + 1 + l, 3]);
                                a1 += (f1[k + 1, l + 1] * MatrixZero[i + k, j + 1 + l, 0]);
                                a2 += (f1[k + 1, l + 1] * MatrixZero[i + k, j + 1 + l, 1]);
                                a3 += (f1[k + 1, l + 1] * MatrixZero[i + k, j + 1 + l, 2]);
                            }
                            else if (i == img.Width - 1 && k == 1)
                            {
                                a0 += (f1[k + 1, l + 1] * MatrixZero[i - 1 + k, j + l, 3]);
                                a1 += (f1[k + 1, l + 1] * MatrixZero[i - 1 + k, j + l, 0]);
                                a2 += (f1[k + 1, l + 1] * MatrixZero[i - 1 + k, j + l, 1]);
                                a3 += (f1[k + 1, l + 1] * MatrixZero[i - 1 + k, j + l, 2]);
                            }
                            else if (j == img.Height - 1 && l == 1)
                            {
                                a0 += (f1[k + 1, l + 1] * MatrixZero[i + k, j - 1 + l, 3]);
                                a1 += (f1[k + 1, l + 1] * MatrixZero[i + k, j - 1 + l, 0]);
                                a2 += (f1[k + 1, l + 1] * MatrixZero[i + k, j - 1 + l, 1]);
                                a3 += (f1[k + 1, l + 1] * MatrixZero[i + k, j - 1 + l, 2]);
                            }
                            else
                            {
                                a0 += (f1[k + 1, l + 1] * MatrixZero[i + k, j + l, 3]);
                                a1 += (f1[k + 1, l + 1] * MatrixZero[i + k, j + l, 0]);
                                a2 += (f1[k + 1, l + 1] * MatrixZero[i + k, j + l, 1]);
                                a3 += (f1[k + 1, l + 1] * MatrixZero[i + k, j + l, 2]);
                            }
                            if (i == 0 && k == -1 && j == 0 && l == -1)
                            {
                                a0 += (f[k + 1, l + 1] * MatrixZero[i + 1 + k, j + 1 + l, 3]);
                                a1 += (f[k + 1, l + 1] * MatrixZero[i + 1 + k, j + 1 + l, 0]);
                                a2 += (f[k + 1, l + 1] * MatrixZero[i + 1 + k, j + 1 + l, 1]);
                                a3 += (f[k + 1, l + 1] * MatrixZero[i + 1 + k, j + 1 + l, 2]);
                            }
                            else if (i == 0 && k == -1 && j == img.Height - 1 && l == 1)
                            {
                                a0 += (f[k + 1, l + 1] * MatrixZero[i + 1 + k, j - 1 + l, 3]);
                                a1 += (f[k + 1, l + 1] * MatrixZero[i + 1 + k, j - 1 + l, 0]);
                                a2 += (f[k + 1, l + 1] * MatrixZero[i + 1 + k, j - 1 + l, 1]);
                                a3 += (f[k + 1, l + 1] * MatrixZero[i + 1 + k, j - 1 + l, 2]);
                            }
                            else if (i == img.Width - 1 && k == 1 && j == 0 && l == -1)
                            {
                                a0 += (f[k + 1, l + 1] * MatrixZero[i - 1 + k, j + 1 + l, 3]);
                                a1 += (f[k + 1, l + 1] * MatrixZero[i - 1 + k, j + 1 + l, 0]);
                                a2 += (f[k + 1, l + 1] * MatrixZero[i - 1 + k, j + 1 + l, 1]);
                                a3 += (f[k + 1, l + 1] * MatrixZero[i - 1 + k, j + 1 + l, 2]);
                            }
                            else if (i == img.Width - 1 && k == 1 && j == img.Height - 1 && l == 1)
                            {
                                a0 += (f[k + 1, l + 1] * MatrixZero[i - 1 + k, j - 1 + l, 3]);
                                a1 += (f[k + 1, l + 1] * MatrixZero[i - 1 + k, j - 1 + l, 0]);
                                a2 += (f[k + 1, l + 1] * MatrixZero[i - 1 + k, j - 1 + l, 1]);
                                a3 += (f[k + 1, l + 1] * MatrixZero[i - 1 + k, j - 1 + l, 2]);
                            }
                            else if (i == 0 && k == -1)
                            {
                                a0 += (f[k + 1, l + 1] * MatrixZero[i + 1 + k, j + l, 3]);
                                a1 += (f[k + 1, l + 1] * MatrixZero[i + 1 + k, j + l, 0]);
                                a2 += (f[k + 1, l + 1] * MatrixZero[i + 1 + k, j + l, 1]);
                                a3 += (f[k + 1, l + 1] * MatrixZero[i + 1 + k, j + l, 2]);
                            }
                            else if (j == 0 && l == -1)
                            {
                                a0 += (f[k + 1, l + 1] * MatrixZero[i + k, j + 1 + l, 3]);
                                a1 += (f[k + 1, l + 1] * MatrixZero[i + k, j + 1 + l, 0]);
                                a2 += (f[k + 1, l + 1] * MatrixZero[i + k, j + 1 + l, 1]);
                                a3 += (f[k + 1, l + 1] * MatrixZero[i + k, j + 1 + l, 2]);
                            }
                            else if (i == img.Width - 1 && k == 1)
                            {
                                a0 += (f[k + 1, l + 1] * MatrixZero[i - 1 + k, j + l, 3]);
                                a1 += (f[k + 1, l + 1] * MatrixZero[i - 1 + k, j + l, 0]);
                                a2 += (f[k + 1, l + 1] * MatrixZero[i - 1 + k, j + l, 1]);
                                a3 += (f[k + 1, l + 1] * MatrixZero[i - 1 + k, j + l, 2]);
                            }
                            else if (j == img.Height - 1 && l == 1)
                            {
                                a0 += (f[k + 1, l + 1] * MatrixZero[i + k, j - 1 + l, 3]);
                                a1 += (f[k + 1, l + 1] * MatrixZero[i + k, j - 1 + l, 0]);
                                a2 += (f[k + 1, l + 1] * MatrixZero[i + k, j - 1 + l, 1]);
                                a3 += (f[k + 1, l + 1] * MatrixZero[i + k, j - 1 + l, 2]);
                            }
                            else
                            {
                                a0 += (f[k + 1, l + 1] * MatrixZero[i + k, j + l, 3]);
                                a1 += (f[k + 1, l + 1] * MatrixZero[i + k, j + l, 0]);
                                a2 += (f[k + 1, l + 1] * MatrixZero[i + k, j + l, 1]);
                                a3 += (f[k + 1, l + 1] * MatrixZero[i + k, j + l, 2]);
                            }
                        }
                    }
                    Matrix[i, j, 0] = ((a1 / d) + offset)/2;
                    Matrix[i, j, 1] = ((a2 / d) + offset)/2;
                    Matrix[i, j, 2] = ((a3 / d) + offset)/2;
                    Matrix[i, j, 3] = ((a0 / d) + offset)/2;
                }
            }
        }

        public void ComeBack()
        {
            for (int i = 0; i < img.Width; i++)
            {
                for (int j = 0; j < img.Height; j++)
                {
                    if (Matrix[i, j, 3] < 0)
                        Matrix[i, j, 3] = 0;
                    if (Matrix[i, j, 2] < 0)
                        Matrix[i, j, 2] = 0;
                    if (Matrix[i, j, 1] < 0)
                        Matrix[i, j, 1] = 0;
                    if (Matrix[i, j, 0] < 0)
                        Matrix[i, j, 0] = 0;
                    if (Matrix[i, j, 3] > 255)
                        Matrix[i, j, 3] = 255;
                    if (Matrix[i, j, 2] > 255)
                        Matrix[i, j, 2] = 255;
                    if (Matrix[i, j, 1] > 255)
                        Matrix[i, j, 1] = 255;
                    if (Matrix[i, j, 0] > 255)
                        Matrix[i, j, 0] = 255;
                    System.Drawing.Color c = System.Drawing.Color.FromArgb(Matrix[i, j, 3], Matrix[i, j, 0], Matrix[i, j, 1], Matrix[i, j, 2]);
                    img.SetPixel(i, j, c);
                }
            }
            this.board2.Source = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(img.GetHbitmap(), IntPtr.Zero, System.Windows.Int32Rect.Empty, BitmapSizeOptions.FromWidthAndHeight(img.Width, img.Height));
        }

        private void MeanFilter_Click(object sender, RoutedEventArgs e)
        {
            Convolution(MBlur, dBlur, 0);
            ComeBack();
        }

        private void GaussFilter_Click(object sender, RoutedEventArgs e)
        {
            Convolution(MGaussianSmoothing, dGaussianSmoothing, 0);
            ComeBack();
        }

        private void SharpenFilter_Click(object sender, RoutedEventArgs e)
        {
            Convolution(MSharpenFilter, 2, 0);
            ComeBack();
        }

        private void RobertsCross_Click(object sender, RoutedEventArgs e)
        {
            ConvolutionRobertsCross(MEdgeDetectionLeft, MEdgeDetectionRight, 1, 100);
            ComeBack();
        }

        private void SobelOperator0_Click(object sender, RoutedEventArgs e)
        {
            int[,] MSobel = new int[3, 3];
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    MSobel[i, j] = 0;
                }
            }
            MSobel[0, 0] = -1;
            MSobel[0, 1] = -2;
            MSobel[0, 2] = -1;
            MSobel[2, 0] = 1;
            MSobel[2, 1] = 2;
            MSobel[2, 2] = 1;
            Convolution(MSobel, 1, 0);
            ComeBack();
        }

        private void SobelOperator45_Click(object sender, RoutedEventArgs e)
        {
            int[,] MSobel = new int[3, 3];
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    MSobel[i, j] = 0;
                }
            }
            MSobel[1, 2] = -1;
            MSobel[0, 1] = -1;
            MSobel[0, 2] = -2;
            MSobel[2, 0] = 2;
            MSobel[2, 1] = 1;
            MSobel[1, 0] = 1;
            Convolution(MSobel, 1, 0);
            ComeBack();
        }

        private void SobelOperator90_Click(object sender, RoutedEventArgs e)
        {
            int[,] MSobel = new int[3, 3];
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    MSobel[i, j] = 0;
                }
            }
            MSobel[0, 0] = 1;
            MSobel[1, 0] = 2;
            MSobel[2, 0] = 1;
            MSobel[0, 2] = -1;
            MSobel[1, 2] = -2;
            MSobel[2, 2] = -1;
            Convolution(MSobel, 1, 0);
            ComeBack();
        }

        private void SobelOperator135_Click(object sender, RoutedEventArgs e)
        {
            int[,] MSobel = new int[3, 3];
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    MSobel[i, j] = 0;
                }
            }
            MSobel[0, 0] = 2;
            MSobel[1, 0] = 1;
            MSobel[0, 1] = 1;
            MSobel[2, 1] = -1;
            MSobel[1, 2] = -1;
            MSobel[2, 2] = -2;
            Convolution(MSobel, 1, 0);
            ComeBack();
        }

        private void SobelOperator180_Click(object sender, RoutedEventArgs e)
        {
            int[,] MSobel = new int[3, 3];
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    MSobel[i, j] = 0;
                }
            }
            MSobel[0, 0] = 1;
            MSobel[0, 1] = 2;
            MSobel[0, 2] = 1;
            MSobel[2, 0] = -1;
            MSobel[2, 1] = -2;
            MSobel[2, 2] = -1;
            Convolution(MSobel, 1, 0);
            ComeBack();
        }

        private void SobelOperator225_Click(object sender, RoutedEventArgs e)
        {
            int[,] MSobel = new int[3, 3];
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    MSobel[i, j] = 0;
                }
            }
            MSobel[1, 2] = 1;
            MSobel[0, 1] = 1;
            MSobel[0, 2] = 2;
            MSobel[2, 0] = -2;
            MSobel[2, 1] = -1;
            MSobel[1, 0] = -1;
            Convolution(MSobel, 1, 0);
            ComeBack();
        }

        private void SobelOperator270_Click(object sender, RoutedEventArgs e)
        {
            int[,] MSobel = new int[3, 3];
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    MSobel[i, j] = 0;
                }
            }
            MSobel[0, 0] = -1;
            MSobel[1, 0] = -2;
            MSobel[2, 0] = -1;
            MSobel[0, 2] = 1;
            MSobel[1, 2] = 2;
            MSobel[2, 2] = 1;
            Convolution(MSobel, 1, 0);
            ComeBack();
        }

        private void SobelOperator315_Click(object sender, RoutedEventArgs e)
        {
            int[,] MSobel = new int[3, 3];
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    MSobel[i, j] = 0;
                }
            }
            MSobel[0, 0] = -2;
            MSobel[1, 0] = -1;
            MSobel[0, 1] = -1;
            MSobel[2, 1] = 1;
            MSobel[1, 2] = 1;
            MSobel[2, 2] = 2;
            Convolution(MSobel, 1, 0);
            ComeBack();
        }

        private void Laplasjan_Click(object sender, RoutedEventArgs e)
        {

            Convolution(MLaplasjanFilter, 1, 0);
            ComeBack();
        }

        private void Equalization_Click(object sender, RoutedEventArgs e)
        {
            hist_c_mod = new int[256];
            int cdf = 0;
            for (int i = 0; i < 256; i++)
            {
                cdf = cdf + hist_c[i];
                double d = 255 * ((double)((double)cdf - (double)hist_c[pmin]) / ((double)(((double)img.Width * (double)img.Height)) - (double)hist_c[pmin]));
                hist_c_mod[i] =(int)d;
            }
            histTranslation();
            ComeBack();
        }

        private void Normalization_Click(object sender, RoutedEventArgs e)
        {
            int[] LUT = new int[256];
            for (int i = 0; i < 256; i++)
            {
                double d = (double)(((double)255 / ((double)pmax - (double)pmin)) * ((double)i - (double)pmin));
                if (255 < (int)d)
                    LUT[i] = 255;
                else if (0 > (int)d)
                    LUT[i] = 0;
                else
                    LUT[i] = (int)d;
            }
            for (int i = 0; i < img.Width; i++)
            {
                for (int j = 0; j < img.Height; j++)
                {
                    //picture data
                    int mean = (MatrixZero[i, j, 0] + MatrixZero[i, j, 1] + MatrixZero[i, j, 2]) / 3;
                    Matrix[i, j, 0] = LUT[mean];
                    Matrix[i, j, 1] = LUT[mean];
                    Matrix[i, j, 2] = LUT[mean];

                    System.Drawing.Color c = System.Drawing.Color.FromArgb(Matrix[i, j, 0], Matrix[i, j, 1], Matrix[i, j, 2]);
                    img.SetPixel(i, j, c);
                }
            }
            this.board2.Source = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(img.GetHbitmap(), IntPtr.Zero, System.Windows.Int32Rect.Empty, BitmapSizeOptions.FromWidthAndHeight(img.Width, img.Height));

        }

        private void IrisDetection_Click(object sender, RoutedEventArgs e)
        {
            int[] LUT = new int[256];
            int[,,] MatrixT;
            MatrixT = new int[img.Width, img.Height, 3];
            int[] hist_iris_v = new int[img.Height];
            int[] hist_iris_h = new int[img.Width];
            int[] hist_pupil_v = new int[img.Height];
            int[] hist_pupil_h = new int[img.Width];

            for (int j = 0; j < img.Height; j++)
            {
                hist_iris_v[j] = 0;
                hist_pupil_v[j] = 0;
            }
            for (int i = 0; i < img.Width; i++)
            {
                hist_iris_h[i] = 0;
                hist_pupil_h[i] = 0;
            }


            for (int i = 0; i < 256; i++)
            {
                double d = (double)(((double)255 / ((double)pgmax - (double)pgmin)) * ((double)i - (double)pgmin));
                if (255 < (int)d)
                    LUT[i] = 255;
                else if (0 > (int)d)
                    LUT[i] = 0;
                else
                    LUT[i] = (int)d;
            }
            for (int i = 0; i < img.Width; i++)
            {
                for (int j = 0; j < img.Height; j++)
                {

                    //picture data
                    //int mean = Matrix[i, j, 1];
                    //Matrix[i, j, 0] = LUT[mean];
                    //Matrix[i, j, 1] = LUT[mean];
                    //Matrix[i, j, 2] = LUT[mean];

                    //mean = Matrix[i, j, 0];
                    int mean = LUT[Matrix[i, j, 0]];

                    int upper = 160;
                    int lower = 20;
                    if (mean < upper)
                    {
                        //Matrix[i, j, 0] = 0;
                        hist_iris_h[i]++;
                        hist_iris_v[j]++;
                    }
                    //else
                    //{
                    //    Matrix[i, j, 0] = 255;
                    //}
                    if (mean < lower)
                    {
                        //MatrixT[i, j, 0] = 0;
                        hist_pupil_h[i]++;
                        hist_pupil_v[j]++;
                    }
                    //else
                    //{
                    //    MatrixT[i, j, 0] = 255;
                    //}


                    //Matrix[i, j, 0] = MatrixT[i, j, 0];// + MatrixT[i, j, 0];
                    //Matrix[i, j, 1] = Matrix[i, j, 0];
                    //Matrix[i, j, 2] = Matrix[i, j, 0];
                }
            }
            int noice_v = 20;
            int noice_h = 30;
            int iris_mean_v = 0;
            int iris_mean_h = 0;
            int pupil_mean_v = 0;
            int pupil_mean_h = 0;
            int iris_start_v = 0;
            int iris_start_h = 0;
            int pupil_start_v = 0;
            int pupil_start_h = 0;
            int iris_end_v = img.Height;
            int iris_end_h = img.Width;
            int pupil_end_v = img.Height;
            int pupil_end_h = img.Width;

            for (int j = 0; j < img.Height; j++)
            {
                iris_mean_v += hist_iris_v[j];
                pupil_mean_v += hist_pupil_v[j];
            }
            for (int i = 0; i < img.Width; i++)
            {
                iris_mean_h += hist_iris_h[i];
                pupil_mean_h += hist_pupil_h[i];
            }
            iris_mean_v = iris_mean_v/ img.Height;
            iris_mean_h = iris_mean_h/ img.Width;
            pupil_mean_v = pupil_mean_v/ img.Height;
            pupil_mean_h = pupil_mean_h/ img.Width;

            iris_mean_v = noice_v;
            iris_mean_h = noice_h;
            pupil_mean_v = noice_v;
            pupil_mean_h = noice_h;

            for (int j = 1; j < (img.Height -1); j++)
            {
                if (iris_mean_v < hist_iris_v[j])
                {
                    if (iris_mean_v >= hist_iris_v[j - 1])
                        iris_start_v = j;
                    if (iris_mean_v >= hist_iris_v[j + 1])
                        iris_end_v = j;
                }
                if (pupil_mean_v < hist_pupil_v[j])
                {
                    if (pupil_mean_v >= hist_pupil_v[j - 1])
                        pupil_start_v = j;
                    if (pupil_mean_v >= hist_pupil_v[j + 1])
                        pupil_end_v = j;
                }
            }
            for (int i = 1; i < (img.Width-1); i++)
            {
                
                if (iris_mean_h < hist_iris_h[i])
                {
                    if (iris_mean_h > hist_iris_h[i - 1])
                        iris_start_h = i;
                    if (iris_mean_h > hist_iris_h[i + 1])
                        iris_end_h = i;
                }
                
                if (pupil_mean_h < hist_pupil_h[i])
                {
                    if (pupil_mean_h > hist_pupil_h[i - 1])
                        pupil_start_h = i;
                    if (pupil_mean_h > hist_pupil_h[i + 1])
                        pupil_end_h = i;
                }
            }
            int pupil_v = pupil_end_v - pupil_start_v;
            int pupil_h = pupil_end_h - pupil_start_h;
            int iris_v = iris_end_v - iris_start_v;
            int iris_h = iris_end_h - iris_start_h;
            int pupil_r = (pupil_h) / 2;
            int iris_r = (iris_h) / 2;
            pupil_v = pupil_end_v - (pupil_r);
            pupil_h = pupil_end_h - (pupil_r);

            for (int i = 0; i < img.Width; i++)
            {
                for (int j = 0; j < img.Height; j++)
                {
                    double distance_i = Math.Sqrt(Math.Pow(pupil_h - i, 2) + Math.Pow(pupil_v - j, 2));
                    double distance_p = Math.Sqrt(Math.Pow(pupil_h - i, 2) + Math.Pow(pupil_v - j, 2));

                    if (distance_i < iris_r && distance_p > pupil_r)
                    {
                        Matrix[i, j, 0] = MatrixZero[i, j, 0];
                        Matrix[i, j, 1] = MatrixZero[i, j, 1];
                        Matrix[i, j, 2] = MatrixZero[i, j, 2];
                    }
                    else
                    {
                        Matrix[i, j, 0] = 255;
                        Matrix[i, j, 1] = 0;
                        Matrix[i, j, 2] = 255;
                    }

                    System.Drawing.Color c = System.Drawing.Color.FromArgb(Matrix[i, j, 0], Matrix[i, j, 1], Matrix[i, j, 2]);
                    img.SetPixel(i, j, c);
                }
            }

            this.board2.Source = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(img.GetHbitmap(), IntPtr.Zero, System.Windows.Int32Rect.Empty, BitmapSizeOptions.FromWidthAndHeight(img.Width, img.Height));

        }

        private void KMM_Click(object sender, RoutedEventArgs e)
        {
            Matrix =  KMMAlgorithm(MatrixZero);
            ComeBack();
        }

        int[,,] KMMAlgorithm(int[,,] Mat)
        {
            int[,] MatN = new int[Mat.GetLength(0), Mat.GetLength(1)];
            int[,] MatP = new int[Mat.GetLength(0), Mat.GetLength(1)];

            for (int i = 0; i < Mat.GetLength(0); i++)
            {
                for (int j = 0; j < Mat.GetLength(1); j++)
                {
                    if (Mat[i, j, 0] == 0 && Mat[i, j, 1] == 0 && Mat[i, j, 2] == 0)
                    {
                        MatN[i, j] = 1;
                        MatP[i, j] = 0;
                    }
                    else
                    {
                        MatN[i, j] = 0;
                        MatP[i, j] = 0;
                    }
                }
            }

            while (!eq(MatP, MatN)) //MatP != MatN)
            {
                MatP = (int[,])MatN.Clone();
                //MatP = MatN;

                for (int i = 1; i < (MatN.GetLength(0) - 1); i++)
                {
                    for (int j = 1; j < (MatN.GetLength(1) - 1); j++)
                    {
                        if (MatN[i, j] == 1 && (MatN[i + 1, j] == 0 || MatN[i, j + 1] == 0 || MatN[i - 1, j] == 0 || MatN[i, j - 1] == 0))
                        {
                            MatN[i, j] = 2;
                        }
                    }
                }
                for (int i = 1; i < (MatN.GetLength(0) - 1); i++)
                {
                    for (int j = 1; j < (MatN.GetLength(1) - 1); j++)
                    {
                        if (MatN[i, j] == 1 && (MatN[i + 1, j + 1] == 0 || MatN[i - 1, j + 1] == 0 || MatN[i - 1, j - 1] == 0 || MatN[i + 1, j - 1] == 0))
                        {
                            MatN[i, j] = 3;
                        }
                    }
                }
                for (int i = 1; i < (MatN.GetLength(0) - 1); i++)
                {
                    for (int j = 1; j < (MatN.GetLength(1) - 1); j++)
                    {
                        if (MatN[i, j] == 2)
                        {
                            bool[] n = new bool[8];
                            int nu = 0;

                            if (MatN[i + 1, j] != 0)
                            { 
                                n[4] = true;
                                nu++;
                            }
                            else
                                n[4] = false;

                            if (MatN[i, j + 1] != 0)
                            {
                                n[6] = true;
                                nu++;
                            }
                            else
                                n[6] = false;

                            if (MatN[i - 1, j] != 0)
                            {
                                n[0] = true;
                                nu++;
                            }
                            else
                                n[0] = false;

                            if (MatN[i, j - 1] != 0)
                            { 
                                n[2] = true;
                                nu++;
                            }
                            else
                                n[2] = false;

                            if (MatN[i + 1, j + 1] != 0)
                            { 
                                n[5] = true;
                                nu++;
                            }
                            else
                                n[5] = false;

                            if (MatN[i - 1, j + 1] != 0)
                            { 
                                n[7] = true;
                                nu++;
                            }
                            else
                                n[7] = false;

                            if (MatN[i - 1, j - 1] != 0)
                            { 
                                n[1] = true;
                                nu++;
                            }
                            else
                                n[1] = false;

                            if (MatN[i + 1, j - 1] != 0)
                            { 
                                n[3] = true;
                                nu++;
                            }
                            else
                                n[3] = false;
                            if (nu == 2 || nu == 3 || nu == 4)
                            {
                                if (n[0] && n[1] && n[2] && n[3] || n[1] && n[2] && n[3] && n[4] ||
                                      n[2] && n[3] && n[4] && n[5] || n[6] && n[3] && n[4] && n[5] ||
                                      n[6] && n[7] && n[4] && n[5] || n[6] && n[7] && n[0] && n[5] ||
                                      n[6] && n[7] && n[1] && n[0] || n[7] && n[0] && n[1] && n[2] ||
                                      n[0] && n[1] && n[2] || n[3] && n[1] && n[2] || n[3] && n[4] && n[2] ||
                                      n[3] && n[4] && n[5] || n[6] && n[4] && n[5] || n[6] && n[7] && n[5] ||
                                      n[6] && n[7] && n[0] || n[7] && n[0] && n[1] ||
                                      n[0] && n[1] || n[1] && n[2] || n[2] && n[3] || n[3] && n[4] ||
                                      n[4] && n[5] || n[5] && n[6] || n[6] && n[6] || n[7] && n[0])
                                    MatN[i, j] = 4;
                            }
                        }
                    }
                }
                int[,] mask = new int[3, 3] { { 128, 64, 32 }, { 1, 0, 16 }, { 2, 4, 8 } };
                //int[,] mask = new int[3, 3] { { 128, 1, 2 }, { 64, 0, 4 }, { 32, 16, 8 } };
                //int[,] mask = new int[3, 3] { { 8, 16, 32 }, { 4, 0, 64 }, { 2, 1, 128 } };

                int[] tab_us = { 3, 5, 7, 12, 13, 14, 15, 20, 21, 22, 23, 28, 29, 30,
                31, 48, 52, 53, 54, 55, 56, 60, 61, 62, 63, 65, 67, 69, 71, 77,
                79, 80, 81, 83, 84, 85, 86, 87, 88, 89, 91, 92, 93, 94, 95, 97,
                99, 101, 103, 109, 111, 112, 113, 115, 116, 117, 118, 119, 120,
                121, 123, 124, 125, 126, 127, 131, 133, 135, 141, 143, 149, 151,
                157, 159, 181, 183, 189, 191, 192, 193, 195, 197, 199, 205, 207,
                208, 209, 211, 212, 213, 214, 215, 216, 217, 219, 220, 221, 222,
                223, 224, 225, 227, 229, 231, 237, 239, 240, 241, 243, 244, 245,
                246, 247, 248, 249, 251, 252, 253, 254, 255 };

                for (int i = 1; i < (MatN.GetLength(0) - 1); i++)
                {
                    for (int j = 1; j < (MatN.GetLength(1) - 1); j++)
                    {
                        if (MatN[i, j] == 4)
                        {
                            int[,] m1 = new int[3, 3] { { 0, 0, 0 }, { 0, 0, 0 }, { 0, 0, 0 } };
                            if (MatN[i + 1, j] != 0)
                                m1[2, 1] = 1;
                            if (MatN[i, j + 1] != 0)
                                m1[1, 2] = 1;
                            if (MatN[i - 1, j] != 0)
                                m1[0, 1] = 1;
                            if (MatN[i, j - 1] != 0)
                                m1[1, 0] = 1;
                            if (MatN[i + 1, j + 1] != 0)
                                m1[2, 2] = 1;
                            if (MatN[i - 1, j + 1] != 0)
                                m1[0, 2] = 1;
                            if (MatN[i - 1, j - 1] != 0)
                                m1[0, 0] = 1;
                            if (MatN[i + 1, j - 1] != 0)
                                m1[2, 0] = 1;
                            int m = m1[2, 1] * mask[2, 1] +
                                            m1[1, 2] * mask[1, 2] +
                                            m1[0, 1] * mask[0, 1] +
                                            m1[1, 0] * mask[1, 0] +
                                            m1[2, 2] * mask[2, 2] +
                                            m1[0, 2] * mask[0, 2] +
                                            m1[0, 0] * mask[0, 0] +
                                            m1[2, 0] * mask[2, 0];
                            bool remove = false;
                            for (int k = 0; k < tab_us.GetLength(0); k++)
                            {
                                if (m == tab_us[k])
                                    remove = true;
                            }
                            if (remove)
                                MatN[i, j] = 0;
                            else
                                MatN[i, j] = 1;
                        }
                    }
                }

                for (int i = 1; i < (MatN.GetLength(0) - 1); i++)
                {
                    for (int j = 1; j < (MatN.GetLength(1) - 1); j++)
                    {
                        if (MatN[i, j] == 2)
                        {
                            int[,] m1 = new int[3, 3] { { 0, 0, 0 }, { 0, 0, 0 }, { 0, 0, 0 } };
                            if (MatN[i + 1, j] != 0)
                                m1[2, 1] = 1;
                            if (MatN[i, j + 1] != 0)
                                m1[1, 2] = 1;
                            if (MatN[i - 1, j] != 0)
                                m1[0, 1] = 1;
                            if (MatN[i, j - 1] != 0)
                                m1[1, 0] = 1;
                            if (MatN[i + 1, j + 1] != 0)
                                m1[2, 2] = 1;
                            if (MatN[i - 1, j + 1] != 0)
                                m1[0, 2] = 1;
                            if (MatN[i - 1, j - 1] != 0)
                                m1[0, 0] = 1;
                            if (MatN[i + 1, j - 1] != 0)
                                m1[2, 0] = 1;
                            int m = m1[2, 1] * mask[2, 1] +
                                            m1[1, 2] * mask[1, 2] +
                                            m1[0, 1] * mask[0, 1] +
                                            m1[1, 0] * mask[1, 0] +
                                            m1[2, 2] * mask[2, 2] +
                                            m1[0, 2] * mask[0, 2] +
                                            m1[0, 0] * mask[0, 0] +
                                            m1[2, 0] * mask[2, 0];
                            bool remove = false;
                            for (int k = 0; k < tab_us.GetLength(0); k++)
                            {
                                if (m == tab_us[k])
                                    remove = true;
                            }
                            if (remove)
                                MatN[i, j] = 0;
                            else
                                MatN[i, j] = 1;
                        }
                    }
                }

                for (int i = 1; i < (MatN.GetLength(0) - 1); i++)
                {
                    for (int j = 1; j < (MatN.GetLength(1) - 1); j++)
                    {
                        if (MatN[i, j] == 3)
                        {
                            int[,] m1 = new int[3, 3] { { 0, 0, 0 }, { 0, 0, 0 }, { 0, 0, 0 } };
                            if (MatN[i + 1, j] != 0)
                                m1[2, 1] = 1;
                            if (MatN[i, j + 1] != 0)
                                m1[1, 2] = 1;
                            if (MatN[i - 1, j] != 0)
                                m1[0, 1] = 1;
                            if (MatN[i, j - 1] != 0)
                                m1[1, 0] = 1;
                            if (MatN[i + 1, j + 1] != 0)
                                m1[2, 2] = 1;
                            if (MatN[i - 1, j + 1] != 0)
                                m1[0, 2] = 1;
                            if (MatN[i - 1, j - 1] != 0)
                                m1[0, 0] = 1;
                            if (MatN[i + 1, j - 1] != 0)
                                m1[2, 0] = 1;
                            int m = m1[2, 1] * mask[2, 1] +
                                            m1[1, 2] * mask[1, 2] +
                                            m1[0, 1] * mask[0, 1] +
                                            m1[1, 0] * mask[1, 0] +
                                            m1[2, 2] * mask[2, 2] +
                                            m1[0, 2] * mask[0, 2] +
                                            m1[0, 0] * mask[0, 0] +
                                            m1[2, 0] * mask[2, 0];
                            bool remove = false;
                            for (int k = 0; k < tab_us.GetLength(0); k++)
                            {
                                if (m == tab_us[k])
                                    remove = true;
                            }
                            if (remove)
                                MatN[i, j] = 0;
                            else
                                MatN[i, j] = 1;
                        }
                    }
                }
            }
            for (int i = 0; i < Mat.GetLength(0); i++)
            {
                for (int j = 0; j < Mat.GetLength(1); j++)
                {
                    if (MatN[i, j] == 0)
                    {
                        Mat[i, j, 0] = 255;
                        Mat[i, j, 1] = 255;
                        Mat[i, j, 2] = 255;
                    }
                    else
                    {
                        Mat[i, j, 0] = 0;
                        Mat[i, j, 1] = 0;
                        Mat[i, j, 2] = 0;
                    }
                }
            }
            return Mat;
        }

        bool eq(int[,] m1, int[,] m2)
        {

            for (int i = 0; i < (m1.GetLength(0)); i++)
            {
                for (int j = 0; j < (m2.GetLength(1)); j++)
                {
                    if (m1[i, j] == m2[i, j])
                    {
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }
    
}
