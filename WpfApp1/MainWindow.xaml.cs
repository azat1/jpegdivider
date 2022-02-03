using Microsoft.Win32;
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

namespace JpegDividerApp
{
    /// <summary>
    /// 
    /// </summary>
    public partial class MainWindow : Window
    {
        public int Hcount { get; set; }
        public int Vcount { get; set; }
        int totalscanlines = -1;
        public MainWindow()
        {
            InitializeComponent();
            Hcount = 2;
            Vcount = 2;
        }

        private void BFileSelect_Click(object sender, RoutedEventArgs e)
        {
            SelectFile();
        }

        private void SelectFile()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "JPEG files|*.jpg;*.jpeg";
            if (ofd.ShowDialog(this)??false)
            {
                tbFileName.Text = ofd.FileName;
            }
            
        }

        private void BMakeDivide_Click(object sender, RoutedEventArgs e)
        {
            Hcount = int.Parse(tbHCount.Text);
            Vcount = int.Parse(tbVCount.Text);
            MakedDivide(tbFileName.Text, tbTabFileName.Text);
            label2.Content = label2.Content + " завершено!";
        }

        private async void MakedDivide(string text, string tabname)
        {
            IProgress<int> progress = new Progress<int>(
                scanline => { if (totalscanlines < 0) totalscanlines = scanline; else label2.Content = scanline.ToString()+"/"+totalscanlines.ToString(); });
            bMakeDivide.IsEnabled = false;
            await Task.Run(()=> JPEGDivider.DivideJpeg(text,Hcount,Vcount, progress,tabname));
            bMakeDivide.IsEnabled = true;
        }

        private void BFileSelectTab_Click(object sender, RoutedEventArgs e)
        {
            TabFileSelect();
        }

        private void TabFileSelect()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "TAB files|*.tab";
            if (ofd.ShowDialog(this) ?? false)
            {
                tbTabFileName.Text = ofd.FileName;
            }
        }
    }
}
