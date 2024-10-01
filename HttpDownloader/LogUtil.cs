using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HttpDownloader
{
    internal class LogUtil
    {
        private TextBox _txtBox = null;
        public LogUtil(TextBox txtBox)
        {
            _txtBox = txtBox;
        }

        public void AddLog(string msg)
        {
            string newMsg = $"[{DateTime.Now.ToLongTimeString()}] {msg}\r\n";
            _txtBox.Text += newMsg;
            _txtBox.SelectionStart = _txtBox.Text.Length;
            _txtBox.SelectionLength = 0;
        }

        public void ClearLog()
        {
            _txtBox.Clear();
        }
    }
}
