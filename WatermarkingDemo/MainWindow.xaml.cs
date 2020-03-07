using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using HiddenWatermark;
using Microsoft.Win32;
using System.Windows.Forms;

namespace WatermarkingDemo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string _imageLocation;
        private string _watermarkImageLocation;
        private string _recoveredWatermarkLocation;

        private Watermark _watermark;

        public MainWindow()
        {
            InitializeComponent();

            if (!Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + "\\dist\\"))
            {
                Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "\\dist\\");
            }

            _imageLocation = AppDomain.CurrentDomain.BaseDirectory + "";
            //_watermarkImageLocation = AppDomain.CurrentDomain.BaseDirectory + "/dist/embeddedwatermark.jpg";
            _watermarkImageLocation = "";
            _recoveredWatermarkLocation = AppDomain.CurrentDomain.BaseDirectory + "/dist/recoveredwatermark.jpg";

            //var fileBytes = File.ReadAllBytes(_imageLocation);
            //RenderImageBytes(OriginalImage, fileBytes);

            _watermark = new Watermark(true);
        }

        private void BtnLoadImage_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog ofd = new Microsoft.Win32.OpenFileDialog();
            ofd.Filter = "Image|*.jpg;*.png;*.gif;*.bmp";
            if (ofd.ShowDialog() == true)
            {
                _imageLocation = ofd.FileName;

                var fileBytes = File.ReadAllBytes(_imageLocation);
                RenderImageBytes(OriginalImage, fileBytes);
            }
        }

        private void BtnLoadWatermarkedImage_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog ofd = new Microsoft.Win32.OpenFileDialog();
            ofd.Filter = "Image|*.jpg;*.png;*.gif;*.bmp";
            if (ofd.ShowDialog() == true)
            {
                _watermarkImageLocation = ofd.FileName;
                var fileBytes = File.ReadAllBytes(_watermarkImageLocation);
                RenderImageBytes(WatermarkedImage, fileBytes);
            }
        }

        private void BtnSaveWatermarkedImage_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_watermarkImageLocation) || !File.Exists(_watermarkImageLocation))
            {
                System.Windows.Forms.MessageBox.Show("请先载入原图并处理", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                Microsoft.Win32.SaveFileDialog sfd = new Microsoft.Win32.SaveFileDialog();
                sfd.Filter = "Image|*.jpg;*.png;*.gif;*.bmp";
                if (sfd.ShowDialog() == true)
                {
                    File.Copy(_watermarkImageLocation, sfd.FileName);
                }
            }
        }

        private void BtnEmbedWatermark_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_imageLocation) || !File.Exists(_imageLocation))
            {
                System.Windows.Forms.MessageBox.Show("请先载入原图", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                var fileBytes = File.ReadAllBytes(_imageLocation);

                var sw = Stopwatch.StartNew();
                var embeddedBytes = _watermark.EmbedWatermark(fileBytes);
                //var embeddedBytes = _watermark.RetrieveAndEmbedWatermark(fileBytes).WatermarkedImage;
                sw.Stop();

                EmbedTime.Text = String.Format("{0}ms", sw.ElapsedMilliseconds);
                _watermarkImageLocation = AppDomain.CurrentDomain.BaseDirectory + "dist\\embeddedwatermark.jpg";

                File.WriteAllBytes(_watermarkImageLocation, embeddedBytes);
                RenderImageBytes(WatermarkedImage, embeddedBytes);
            }
        }

        private void BtnRetrieveWatermark_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_watermarkImageLocation) || !File.Exists(_watermarkImageLocation))
            {
                System.Windows.Forms.MessageBox.Show("请先载入处理过的图片", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                var fileBytes = File.ReadAllBytes(_watermarkImageLocation);

                var sw = Stopwatch.StartNew();
                var result = _watermark.RetrieveWatermark(fileBytes);
                sw.Stop();

                RetrieveTime.Text = String.Format("{0}ms", sw.ElapsedMilliseconds);
                SimilarityText.Text = String.Format("Similarity: {0}%", result.Similarity);

                if (result.WatermarkDetected)
                {
                    SuccessImage.Visibility = Visibility.Visible;
                    FailureImage.Visibility = Visibility.Collapsed;
                }
                else
                {
                    SuccessImage.Visibility = Visibility.Collapsed;
                    FailureImage.Visibility = Visibility.Visible;
                }

                File.WriteAllBytes(_recoveredWatermarkLocation, result.RecoveredWatermark);
                RenderImageBytes(RetrievedWatermark, result.RecoveredWatermark);
            }
        }

        private void RenderImageBytes(System.Windows.Controls.Image control, byte[] bytes)
        {
            MemoryStream byteStream = new MemoryStream(bytes);
            BitmapImage imageSource = new BitmapImage();
            imageSource.BeginInit();
            imageSource.StreamSource = byteStream;
            imageSource.EndInit();

            control.Source = imageSource;
        }
    }
}