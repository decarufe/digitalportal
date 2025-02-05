﻿using GHIElectronics.TinyCLR.UI;
using GHIElectronics.TinyCLR.UI.Input;
using GHIElectronics.TinyCLR.UI.Media;
using GHIElectronics.TinyCLR.UI.Controls;

using Bytewizer.TinyCLR.DigitalPortal.Client.Models;

namespace Bytewizer.TinyCLR.DigitalPortal
{
    public class OpenWeatherPage : SettingPage
    {
        private ToggleSwitch toggleUnits;
        private Text labelId;
        private TextBox textId;
        private TextBox textLocation;
        private Text buttonRefresh;

        private Text labelLat;
        private TextBox textLat;

        private Text labelLon;
        private TextBox textLon;

        public OpenWeatherPage(int width, int height)
            : base(width, height)
        {
            BackText = ResourcesProvider.UxNavigateBefore;
            Title = "Weather Settings";
            //NextText = ResourcesProvider.UxSave;
            
            BackClick += WeatherSettingsPage_BackClick;
            InitializePage();
        }

        public override void CreateBody()
        {
            var panelUnits = new StackPanel(Orientation.Horizontal)
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                Height = 40
            };

            var labelUnits = new Text()
            {
                TextContent = "Temperature Unit",
                Font = ResourcesProvider.SmallDigitalFont,
                ForeColor = SettingsProvider.Theme.Standard,
                VerticalAlignment = VerticalAlignment.Bottom,
                TextAlignment = TextAlignment.Left,
                Width = 480 - 40 - 60
            };

            bool unitState;
            switch (SettingsProvider.Flash.Units)
            {
                case Units.Imperial:
                    unitState = true;
                    break;

                default:
                    unitState = false;
                    break;
            }

            toggleUnits = new ToggleSwitch()
            {
                IsOn = unitState,
                Foreground = new SolidColorBrush(SettingsProvider.Theme.Highlighted),
                Background = new SolidColorBrush(SettingsProvider.Theme.Shadow),
                Width = 60,
                Height = 40
            };
            toggleUnits.Click += ToggleUnits_Click;

            panelUnits.Children.Add(labelUnits);
            panelUnits.Children.Add(toggleUnits);

            var panelId = new StackPanel(Orientation.Horizontal)
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                Height = 40
            };

            labelId = new Text()
            {
                TextContent = "App Id",
                Font = ResourcesProvider.SmallDigitalFont,
                ForeColor = SettingsProvider.Theme.Standard,
                VerticalAlignment = VerticalAlignment.Bottom,
                TextAlignment = TextAlignment.Left,
                Width = 140
            };

            textId = new TextBox()
            {
                Text = SettingsProvider.Flash.OwmAppId,
                Font = ResourcesProvider.SmallRobotoFont,
                Foreground = new SolidColorBrush(SettingsProvider.Theme.Standard),
                Background = new SolidColorBrush(SettingsProvider.Theme.Shadow),
                VerticalAlignment = VerticalAlignment.Bottom,
                TextAlign = TextAlignment.Center,
                Width = 300
            };

            panelId.Children.Add(labelId);
            panelId.Children.Add(textId);

            var panelLocation = new StackPanel(Orientation.Horizontal)
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                Height = 40
            };

            var labelLocation = new Text()
            {
                TextContent = "Location",
                Font = ResourcesProvider.SmallDigitalFont,
                ForeColor = SettingsProvider.Theme.Standard,
                VerticalAlignment = VerticalAlignment.Bottom,
                TextAlignment = TextAlignment.Left,
                Width = 100
            };

            textLocation = new TextBox()
            {
                Text = SettingsProvider.Flash.Location,
                Font = ResourcesProvider.SmallRobotoFont,
                Foreground = new SolidColorBrush(SettingsProvider.Theme.Standard),
                Background = new SolidColorBrush(SettingsProvider.Theme.Shadow),
                VerticalAlignment = VerticalAlignment.Bottom,
                TextAlign = TextAlignment.Center,
                Width = 300
            };
            textLocation.TextChanged += TextLocation_TextChanged;

            buttonRefresh = new Text()
            {
                TextContent = ResourcesProvider.UxCached,
                Font = ResourcesProvider.SmallUxIcons,
                ForeColor = SettingsProvider.Theme.Foreground,
                VerticalAlignment = VerticalAlignment.Bottom,
                TextAlignment = TextAlignment.Center,
                Width = 40
            };
            buttonRefresh.TouchUp += ButtonRefresh_TouchUp;

            panelLocation.Children.Add(labelLocation);
            panelLocation.Children.Add(buttonRefresh);
            panelLocation.Children.Add(textLocation);

            var panelLat = new StackPanel(Orientation.Horizontal)
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                Height = 40
            };

            labelLat = new Text()
            {
                TextContent = "Latitude",
                Font = ResourcesProvider.SmallDigitalFont,
                ForeColor = SettingsProvider.Theme.Standard,
                VerticalAlignment = VerticalAlignment.Bottom,
                TextAlignment = TextAlignment.Left,
                Width = 140
            };

            textLat = new TextBox()
            {
                Text = SettingsProvider.Flash.Lat,
                Font = ResourcesProvider.SmallRobotoFont,
                Foreground = new SolidColorBrush(SettingsProvider.Theme.Standard),
                Background = new SolidColorBrush(SettingsProvider.Theme.Shadow),
                VerticalAlignment = VerticalAlignment.Bottom,
                TextAlign = TextAlignment.Center,
                Width = 300
            };
            textLat.TextChanged += TextLat_TextChanged;

            panelLat.Children.Add(labelLat);
            panelLat.Children.Add(textLat);

            var panelLon = new StackPanel(Orientation.Horizontal)
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                Height = 40
            };

            labelLon = new Text()
            {
                TextContent = "Longitude",
                Font = ResourcesProvider.SmallDigitalFont,
                ForeColor = SettingsProvider.Theme.Standard,
                VerticalAlignment = VerticalAlignment.Bottom,
                TextAlignment = TextAlignment.Left,
                Width = 140
            };

            textLon = new TextBox()
            {
                Text = SettingsProvider.Flash.Lon,
                Font = ResourcesProvider.SmallRobotoFont,
                Foreground = new SolidColorBrush(SettingsProvider.Theme.Standard),
                Background = new SolidColorBrush(SettingsProvider.Theme.Shadow),
                VerticalAlignment = VerticalAlignment.Bottom,
                TextAlign = TextAlignment.Center,
                Width = 300
            };
            textLon.TextChanged += TextLon_TextChanged;

            panelLon.Children.Add(labelLon);
            panelLon.Children.Add(textLon);

            PageBody.Children.Add(panelUnits);
            PageBody.Children.Add(panelId);
            PageBody.Children.Add(panelLocation);
            PageBody.Children.Add(panelLat);
            PageBody.Children.Add(panelLon);
        }

        public override void OnActivate() 
        {
            bool unitState;
            switch (SettingsProvider.Flash.Units)
            {
                case Units.Imperial:
                    unitState = true;
                    break;

                default:
                    unitState = false;
                    break;
            }
            toggleUnits.IsOn = unitState;


            if (NetworkProvider.IsConnected == false)
            {
                buttonRefresh.Visibility = Visibility.Hidden;
            }
            else
            {
                buttonRefresh.Visibility = Visibility.Visible;
            }
        }

        private void WeatherSettingsPage_BackClick(object sender, RoutedEventArgs e)
        {
            WeatherProvider.Connect();
            Parent.Refresh();
            Parent.Activate(3);
        }

        private void ButtonRefresh_TouchUp(object sender, TouchEventArgs e)
        {
            if (NetworkProvider.IsConnected)
            {
                NetworkProvider.ConnectGeoLocation();
                NetworkProvider.ConnectNetworkTime();

                UXExtensions.DoThreadSafeAction(textLocation, () =>
                {
                    textLocation.Text = SettingsProvider.Flash.Location;
                    textLocation.Invalidate();
                });

                UXExtensions.DoThreadSafeAction(textLat, () =>
                {
                    textLat.Text = SettingsProvider.Flash.Lat;
                    textLat.Invalidate();
                });

                UXExtensions.DoThreadSafeAction(textLon, () =>
                {
                    textLon.Text = SettingsProvider.Flash.Lon;
                    textLon.Invalidate();
                });
            }
        }

        private void ToggleUnits_Click(object sender, RoutedEventArgs e)
        {
            var unit = ((ToggleSwitch)sender).IsOn;

            if (unit)
            {
                SettingsProvider.WriteUnits(Units.Metric);
            }
            else
            {
                SettingsProvider.WriteUnits(Units.Imperial);
            }
            
            UXExtensions.DoThreadSafeAction(toggleUnits, () =>
            {
                toggleUnits.IsOn = unit;
                toggleUnits.Invalidate();
            });

            Parent.Invalidate();
        }

        private void TextLocation_TextChanged(object sender, TextChangedEventArgs e)
        {
            var text = ((TextBox)sender).Text.Trim();

            if (text.Length > 0)
            {
                SettingsProvider.WriteLocation(text);
            }
        }

        private void TextLat_TextChanged(object sender, TextChangedEventArgs e)
        {
            var text = ((TextBox)sender).Text.Trim();

            if (text.Length > 0)
            {
                SettingsProvider.WriteLat(text);
            }
        }

        private void TextLon_TextChanged(object sender, TextChangedEventArgs e)
        {
            var text = ((TextBox)sender).Text.Trim();

            if (text.Length > 0)
            {
                SettingsProvider.WriteLon(text);
            }
        }
    }
}