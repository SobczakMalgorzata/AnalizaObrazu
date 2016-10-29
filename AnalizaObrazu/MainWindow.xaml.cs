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
        int[] hist_h;
        int[] hist_v;
        int[] hist_c;
        int[] hist_c_mod;
        List<Vector3D> cSpace;
        int[,] MBlur = new int[3, 3];
        int dBlur = 9;
        int[,] MGaussianSmoothing = new int[3, 3];
        int dGaussianSmoothing = 8;
        int[,] MSharpenFilter = new int[3, 3];
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
                    MSharpenFilter[i, j] = -1;
                    MEdgeDetectionLeft[i, j] = 0;
                    MEdgeDetectionRight[i, j] = 0;
                    //MEmboss[i, j] = 1;
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
            MSharpenFilter[1, 1] = 13;

            //MSharpenFilter[0, 0] = 0;
            //MSharpenFilter[0, 2] = 0;
            //MSharpenFilter[2, 0] = 0;
            //MSharpenFilter[2, 2] = 0;
            //MSharpenFilter[1, 1] = 5;

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

        public void histTrnaslation()
        {
            for (int i = 0; i < img.Width; i++)
            {
                for (int j = 0; j < img.Height; j++)
                {
                    //Mod for hist change
                }
            }
        }

        public void getRGB()
        {
            Matrix = new int[img.Width, img.Height, 4];
            MatrixZero = new int[img.Width, img.Height, 4];
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

        }

        private void MetodaBarensa_Click(object sender, RoutedEventArgs e)
        {

        }

        private void SaveHistogram_Click(object sender, RoutedEventArgs e)
        {

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
            Convolution(MSharpenFilter, 1, 0);
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
    }
    
}
