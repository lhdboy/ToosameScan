using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using ZXing;
using Windows.Storage;
using Windows.Graphics.Imaging;
using System.Threading.Tasks;
using ZXing.QrCode.Internal;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace ToosameScan
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class BuildPage : Page
    {
        public BuildPage()
        {
            this.InitializeComponent();
        }

        private void BuildBtn_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(contentBox.Text))
            {
                UIHelper.ShowDialog("请输入二维码内容", AlertIcon.Question);
                return;
            }
            if (string.IsNullOrWhiteSpace(contentBox.Text))
            {
                UIHelper.ShowDialog("请输入有意义的二维码内容", AlertIcon.Question);
                return;
            }

            ZXing.Common.EncodingOptions qrEncodeOption = new ZXing.Common.EncodingOptions();
            qrEncodeOption.Height = 400;
            qrEncodeOption.Width = 400;
            qrEncodeOption.Margin = 1; // 设置周围空白边距
            
            BarcodeWriter write = new BarcodeWriter();
            write.Format = BarcodeFormat.QR_CODE;
            write.Options = qrEncodeOption;
            var w = write.Write(contentBox.Text);
            WriteableBitmap bitmap = (WriteableBitmap)w.ToBitmap();
            qrcodeImage.Source = bitmap;
            //}
        }

        private async void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            WriteableBitmap bitmap = (WriteableBitmap)qrcodeImage.Source;
            if(bitmap == null)
            {
                UIHelper.ShowDialog("请先生成二维码", AlertIcon.Ok);
                return;
            }
            StorageFile file = await KnownFolders.SavedPictures.CreateFileAsync(Guid.NewGuid() + ".jpg");
            await SaveToStorageFile(bitmap, file);
        }

        /// <summary>
        /// 将图像写入到文件
        /// </summary>
        /// <param name="writeableBitmap"></param>
        /// <param name="storageFile"></param>
        /// <returns></returns>
        public async Task SaveToStorageFile(WriteableBitmap writeableBitmap, IStorageFile storageFile, double dpi = 100.0)
        {
            var bitmap = new RenderTargetBitmap();
            await bitmap.RenderAsync(qrcodeImage);

            var writeableBmp = await bitmap.GetPixelsAsync();

            using (var writestream = await storageFile.OpenAsync(FileAccessMode.ReadWrite))
            {
                var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.JpegEncoderId, writestream);

                using (var pixelStream = writeableBitmap.PixelBuffer.AsStream())
                {
                    var pixels = new byte[pixelStream.Length];
                    await pixelStream.ReadAsync(pixels, 0, pixels.Length);

                    encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Ignore,
                       (uint)writeableBitmap.PixelWidth, (uint)writeableBitmap.PixelHeight, dpi, dpi, pixels);

                    await encoder.FlushAsync();
                    UIHelper.ShowDialog("保存在相册：保存的图片 中", AlertIcon.Ok);
                }
            }
        }

        //public async static Task<WriteableBitmap> EncoderBitmap(ByteMatrix matrix)
        //{
        //    using (InMemoryRandomAccessStream inStream = new InMemoryRandomAccessStream())
        //    {
        //        //using(Stream s = inStream.AsStreamForWrite())
        //        //{
        //        //    await s.WriteAsync(bytes, 0, bytes.Length);
        //        matrix.
        //        var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.JpegEncoderId, inStream);
        //        encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Ignore, 300, 300, 96.0, 96.0, bytes);
        //        await encoder.FlushAsync();

        //        SoftwareBitmap s = new SoftwareBitmap(BitmapPixelFormat.Bgra8, 300, 300);
        //        encoder.SetSoftwareBitmap(s);

        //        WriteableBitmap w = new WriteableBitmap(300, 300);
        //        s.CopyToBuffer(w.PixelBuffer);
        //        return w;
        //        //}
        //    }
        //}
    }
}
