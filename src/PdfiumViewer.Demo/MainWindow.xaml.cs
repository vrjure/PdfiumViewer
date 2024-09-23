using Microsoft.Win32;
using PdfiumViewer.Core;
using PdfiumViewer.Demo.Annotations;
using PdfiumViewer.Demo.ViewModels;
using PdfiumViewer.Drawing;
using PdfiumViewer.Enums;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace PdfiumViewer.Demo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            var version = GetType().Assembly.GetName().Version.ToString(3);
            Title = $"WPF PDFium Viewer Demo v{version}";
            DataContext = new MainWindowViewModel();

        }
    
    }
}
