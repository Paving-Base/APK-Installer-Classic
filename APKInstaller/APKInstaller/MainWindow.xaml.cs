﻿using APKInstaller.Pages;
using MicaWPF.Controls;
using System.Windows;

namespace APKInstaller
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MicaWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            MainPage MainPage = new();
            Content = MainPage;
        }
    }
}
