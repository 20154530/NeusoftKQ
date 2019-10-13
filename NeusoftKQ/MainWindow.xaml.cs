using AforgeNumVerify;
using AforgeNumVerify.AForge.Core;
using NeusoftKQ.Model;
using NeusoftKQ.Services;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media.Imaging;
using WebAccessorCore.ApiClient;

namespace NeusoftKQ {
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window {

        public MainWindow() {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e) {
  
        }

        private void Button_Click(object sender, RoutedEventArgs e) {
            new KQSer(new HttpClientAccessor()).DoKQ();
        }
    }
}
