using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToosameScan
{
    public enum AlertIcon
    {
        Question = 0,
        Info = 1,
        Error = 2,
        Ok = 3,
        Warn = 4,
        Star = 5,
        Like = 6,
        Timeout = 7,
        Tip = 8
    }

    public class UIHelper
    {
        private static readonly string[] Icon = {
            "\xE11B",
            "\xE783",
            "\xEA39",
            "\xE930",
            "\xE946",
            "\xE208",
            "\xE8E1",
            "\xE2AD",
            "\xEB50"
        };

        /// <summary>
        /// 显示提示对话框
        /// </summary>
        /// <param name="content">内容</param>
        /// <param name="icon">图标</param>
        public static void ShowDialog(string content, AlertIcon icon)
        {
            AlertPopup notifyPopup = new AlertPopup(content, Icon[Convert.ToInt32(icon)]);
            notifyPopup.Show();
        }
    }
}
