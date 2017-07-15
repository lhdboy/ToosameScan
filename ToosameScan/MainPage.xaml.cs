using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.DataTransfer;
using Windows.Devices.Enumeration;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.Graphics.Imaging;
using Windows.Media;
using Windows.Media.Capture;
using Windows.Media.Devices;
using Windows.Media.MediaProperties;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using ZXing;

// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板

namespace ToosameScan
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private MediaCapture _mediaCapture;
        private DispatcherTimer _timer;
        private bool IsBusy;
        private bool _isPreviewing = false;
        private bool _isInitVideo = false;
        BarcodeReader barcodeReader;

        private static readonly Guid RotationKey = new Guid("C380465D-2271-428C-9B83-ECEA3B4A85C1");

        public MainPage()
        {
            barcodeReader = new BarcodeReader
            {
                AutoRotate = true,
                Options = new ZXing.Common.DecodingOptions { TryHarder = true }
            };
            SystemNavigationManager.GetForCurrentView().BackRequested += App_BackRequested;
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Required;
            Application.Current.Suspending += Application_Suspending;
            Application.Current.Resuming += Application_Resuming;
        }

        private void App_BackRequested(object sender, BackRequestedEventArgs e)
        {
            if (Frame.CanGoBack)
            {
                e.Handled = true;
                Frame.GoBack();
                return;
            }
        }

        private async void Application_Suspending(object sender, SuspendingEventArgs e)
        {
            // Handle global application events only if this page is active
            if (Frame.CurrentSourcePageType == typeof(MainPage))
            {
                var deferral = e.SuspendingOperation.GetDeferral();

                await CleanupCameraAsync();

                deferral.Complete();
            }
        }

        private void Application_Resuming(object sender, object o)
        {
            // Handle global application events only if this page is active
            if (Frame.CurrentSourcePageType == typeof(MainPage))
            {
                InitVideoCapture();
            }
        }

        protected override async void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            // Handling of this event is included for completenes, as it will only fire when navigating between pages and this sample only includes one page
            await CleanupCameraAsync();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            InitVideoCapture();
        }

        private async Task CleanupCameraAsync()
        {
            if (_isPreviewing)
            {
                await StopPreviewAsync();
            }
            _timer.Stop();
            if (_mediaCapture != null)
            {
                _mediaCapture.Dispose();
                _mediaCapture = null;
            }
        }

        private void InitVideoTimer()
        {
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += _timer_Tick;
            _timer.Start();
        }

        private async Task StopPreviewAsync()
        {
            _isPreviewing = false;
            await _mediaCapture.StopPreviewAsync();

            // Use the dispatcher because this method is sometimes called from non-UI threads
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                VideoCapture.Source = null;
            });
        }

        private async void _timer_Tick(object sender, object e)
        {
            try
            {
                if (!IsBusy)
                {
                    IsBusy = true;
                    var previewProperties = _mediaCapture.VideoDeviceController.GetMediaStreamProperties(MediaStreamType.VideoPreview) as VideoEncodingProperties;

                    VideoFrame videoFrame = new VideoFrame(BitmapPixelFormat.Bgra8, (int)previewProperties.Width, (int)previewProperties.Height);
                    VideoFrame previewFrame = await _mediaCapture.GetPreviewFrameAsync(videoFrame);
                    WriteableBitmap bitmap = new WriteableBitmap(previewFrame.SoftwareBitmap.PixelWidth, previewFrame.SoftwareBitmap.PixelHeight);

                    previewFrame.SoftwareBitmap.CopyToBuffer(bitmap.PixelBuffer);

                    await Task.Factory.StartNew(async () =>
                    {
                        await ScanBitmap(bitmap);
                    });
                }
                IsBusy = false;
                await Task.Delay(50);
            }
            catch (Exception)
            {
                IsBusy = false;
            }
        }

        /// <summary>
        /// 解析二维码图片
        /// </summary>
        /// <param name="writeableBmp">图片</param>
        /// <returns></returns>
        private async Task ScanBitmap(WriteableBitmap writeableBmp)
        {
            try
            {
                await writeableBmp.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                  {
                      Result _result = null;

                      _result = barcodeReader.Decode(writeableBmp);

                      if (_result != null)
                      {
                          if (autoCopy.IsChecked)
                          {
                              DataPackage dataPackage = new DataPackage();
                              try
                              {
                                  dataPackage.SetText(_result.Text);
                                  Clipboard.SetContent(dataPackage);
                              }
                              catch (Exception)
                              {
                                  return;
                              }
                          }
                          if (Uri.TryCreate(_result.Text, UriKind.Absolute, out Uri url) && autoNav.IsChecked)
                          {
                              await Windows.System.Launcher.LaunchUriAsync(url);
                          }
                          else
                          {
                              if (!ResultPopup.isOpen)
                              {
                                  ResultPopup popup = new ResultPopup(_result.Text);
                                  popup.ShowWIndow();
                              }
                          }
                      }
                  });
                //await LayoutRoot.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                // {
                //     _result = barcodeReader.Decode(writeableBmp.PixelBuffer.ToArray(), writeableBmp.PixelWidth, writeableBmp.PixelHeight, RGBLuminanceSource.BitmapFormat.Unknown);
                // });

                //await recScanning.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                //{

                //});
            }
            catch (Exception)
            {
                return;
            }
        }

        private async void InitVideoCapture()
        {
            ///摄像头的检测  
            var cameraDevice = await FindCameraDeviceByPanelAsync(Windows.Devices.Enumeration.Panel.Back);

            if (cameraDevice == null)
            {
                Frame.Navigate(typeof(BuildPage));
                UIHelper.ShowDialog("你的设备没有摄像头，老天爷来了也扫描不了，所以只能生成二维码", AlertIcon.Error);
                return;
            }
            var settings = new MediaCaptureInitializationSettings
            {
                StreamingCaptureMode = StreamingCaptureMode.Video,
                MediaCategory = MediaCategory.Other,
                AudioProcessing = AudioProcessing.Default,
                VideoDeviceId = cameraDevice.Id
            };
            _mediaCapture = new MediaCapture();
            await _mediaCapture.InitializeAsync(settings);

            VideoCapture.Source = _mediaCapture;
            await _mediaCapture.StartPreviewAsync();

            var props = _mediaCapture.VideoDeviceController.GetMediaStreamProperties(MediaStreamType.VideoPreview);
            props.Properties.Add(RotationKey, 90);

            await _mediaCapture.SetEncodingPropertiesAsync(MediaStreamType.VideoPreview, props, null);

            var focusControl = _mediaCapture.VideoDeviceController.FocusControl;

            if (focusControl.Supported)
            {
                await focusControl.UnlockAsync();
                var setting = new FocusSettings { Mode = FocusMode.Continuous, AutoFocusRange = AutoFocusRange.FullRange };
                focusControl.Configure(setting);
                await focusControl.FocusAsync();
            }

            _isPreviewing = true;
            _isInitVideo = true;
            InitVideoTimer();
        }

        private static async Task<DeviceInformation> FindCameraDeviceByPanelAsync(Windows.Devices.Enumeration.Panel desiredPanel)
        {
            var allVideoDevices = await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture);

            DeviceInformation desiredDevice = allVideoDevices.FirstOrDefault(x => x.EnclosureLocation != null && x.EnclosureLocation.Panel == desiredPanel);

            return desiredDevice ?? allVideoDevices.FirstOrDefault();
        }

        private void BuildQrcode_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(BuildPage));
        }

        private void AboutQrcode_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(AboutPage));
        }
    }
}
