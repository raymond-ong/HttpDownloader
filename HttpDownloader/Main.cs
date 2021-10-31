using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HttpDownloader
{
    public partial class Main : Form
    {
        LogUtil logUtil;
        HttpDownloadUtil httpUtil;
        public Main()
        {
            InitializeComponent();
            logUtil = new LogUtil(this.txtLogs);
            httpUtil = new HttpDownloadUtil(logUtil);
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string filePath = saveFileDialog1.FileName;
                txtFilePath.Text = filePath;
            }
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtUrl.Text) ||
                string.IsNullOrEmpty(txtHeaders.Text) ||
                string.IsNullOrEmpty(txtFilePath.Text))
            {
                MessageBox.Show("Invalid Input Parameters. Please don't leave any field blank.");
                return;
            }

            httpUtil.StartDownloadAsync(txtUrl.Text, txtHeaders.Text, txtFilePath.Text);
        }
    }
}
