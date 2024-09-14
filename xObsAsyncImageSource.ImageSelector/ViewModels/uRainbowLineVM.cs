﻿using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows.Media;
using xObsAsyncImageSource.ImageSelector.Interfaces;

namespace xObsAsyncImageSource.ImageSelector.ViewModels
{
    public partial class uRainbowLineVM : ObservableObject, IuRainbowLineVM
    {
        private readonly LinearGradientBrush defBrush = new LinearGradientBrush()
        {
            StartPoint = new(0, 0),
            EndPoint = new(1, 0),
            GradientStops = new()
            {
                new GradientStop((Color)ColorConverter.ConvertFromString("#009fd9"), 1.000),
                new GradientStop((Color)ColorConverter.ConvertFromString("#65b849"), 0.834),
                new GradientStop((Color)ColorConverter.ConvertFromString("#f7b423"), 0.667),
                new GradientStop((Color)ColorConverter.ConvertFromString("#f58122"), 0.500),
                new GradientStop((Color)ColorConverter.ConvertFromString("#de3a3c"), 0.334),
                new GradientStop((Color)ColorConverter.ConvertFromString("#943f96"), 0.137),
                new GradientStop((Color)ColorConverter.ConvertFromString("#009fd9"), 0.000),
            }
        };

        [ObservableProperty]
        private Brush brushColor;
        [ObservableProperty]
        private double width;
        [ObservableProperty]
        private double height;

        public uRainbowLineVM()
        {
            this.BrushColor = defBrush;
            this.Width = double.NaN;
            this.Height = 1;
        }
    }
}
