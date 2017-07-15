using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace ToosameScan
{
    public sealed partial class ResultPopup : UserControl
    {
        private Popup m_Popup;
        public double CenterGridWidth { get; set; }
        public VerticalAlignment CenterAlignment { get; set; } = VerticalAlignment.Center;
        public static bool isOpen = false;

        public ResultPopup()
        {
            if (Window.Current.Bounds.Width < 720)
            {
                CenterAlignment = VerticalAlignment.Top;
                CenterGridWidth = Window.Current.Bounds.Width;
            }
            m_Popup = new Popup();
            m_Popup.IsLightDismissEnabled = true;
            this.InitializeComponent();
            Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().VisibleBoundsChanged += (s, e) =>
            {
                MeasurePopupSize();
            };
            MeasurePopupSize();
            m_Popup.Child = this;
            m_Popup.Closed += M_Popup_Closed;
            this.Loaded += MessagePopupWindow_Loaded;
            this.Unloaded += MessagePopupWindow_Unloaded;
        }

        private void M_Popup_Closed(object sender, object e)
        {
            isOpen = false;
        }

        public ResultPopup(string text) : this()
        {
            resultBox.Text = text;
        }

        private void MeasurePopupSize()
        {
            this.Width = Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().VisibleBounds.Width;

            double marginTop = 0;
            if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
                marginTop = Windows.UI.ViewManagement.StatusBar.GetForCurrentView().OccludedRect.Height;
            this.Height = Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().VisibleBounds.Height;
            this.Margin = new Thickness(0, marginTop, 0, 0);
        }

        private void MessagePopupWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Window.Current.SizeChanged += MessagePopupWindow_SizeChanged; ;
        }

        private void MessagePopupWindow_SizeChanged(object sender, Windows.UI.Core.WindowSizeChangedEventArgs e)
        {
            this.Width = e.Size.Width;
            this.Height = e.Size.Height;
        }

        private void MessagePopupWindow_Unloaded(object sender, RoutedEventArgs e)
        {
            Window.Current.SizeChanged -= MessagePopupWindow_SizeChanged;
        }

        public void ShowWIndow()
        {
            isOpen = true;
            m_Popup.IsOpen = true;
        }

        private void DismissWindow()
        {
            isOpen = false;
            m_Popup.IsOpen = false;
        }

        private void OutBorder_Tapped(object sender, TappedRoutedEventArgs e)
        {
            DismissWindow();
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            DismissWindow();
        }

        private void CopyBtn_Click(object sender, RoutedEventArgs e)
        {
            DataPackage dataPackage = new DataPackage();
            try
            {
                dataPackage.SetText(resultBox.Text);
                Clipboard.SetContent(dataPackage);
                UIHelper.ShowDialog("复制成功", AlertIcon.Ok);
            }
            catch (Exception)
            {
                UIHelper.ShowDialog("复制失败", AlertIcon.Ok);
            }
        }
    }
}
