using Microsoft.Maui.Storage;
using CommunityToolkit.Mvvm.Messaging;
using System.IO;

namespace PhotoboothParty;

public partial class SettingsPage : ContentPage
{
    private string _tempCustomFolderPath = "";
    private string _tempBgImagePath = "";

    private static readonly System.Collections.Generic.List<(string Hex, string Name)> ColorPresets = new()
    {
        ("#FFFFFF", "White"),
        ("#C0C0C0", "Silver"),
        ("#FFD700", "Gold"),
        ("#FF6B6B", "Coral"),
        ("#FF8E9E", "Rose"),
        ("#D6A2E8", "Lavender"),
        ("#4D96FF", "Sky Blue"),
        ("#6BCB77", "Mint"),
        ("#FF9F43", "Orange"),
        ("#00D2D3", "Cyan")
    };

    private string _line1Color = "#C0C0C0";
    private string _line2Color = "#FFFFFF";
    private string _line3Color = "#FFFFFF";
    private string _line4Color = "#808080";
    private string _flashMode = "Auto";

    public SettingsPage()
    {
        InitializeComponent();
        
        lblAppVersionHeader.Text = $"PhotoboothParty v{AppInfo.Current.VersionString}";

        entryCaptureTimer.Text = Preferences.Default.Get("capture_timer", 10).ToString();
        entryPreviewDuration.Text = Preferences.Default.Get("preview_duration", 5).ToString();
        entryInactivityTimeout.Text = Preferences.Default.Get("inactivity_timeout", 20).ToString();

        entryLine1Text.Text = Preferences.Default.Get("title_line1_text", "Noces d'Argent \U0001F4F8");
        _line1Color = Preferences.Default.Get("title_line1_color", "Silver");
        double size1 = Preferences.Default.Get("title_line1_size", 36.0);
        sliderLine1Size.Value = size1;
        lblLine1Size.Text = $"{(int)size1} px";

        entryLine2Text.Text = Preferences.Default.Get("title_line2_text", PhotoboothParty.Resources.Strings.AppResources.DefaultTitleLine2);
        _line2Color = Preferences.Default.Get("title_line2_color", "White");
        double size2 = Preferences.Default.Get("title_line2_size", 18.0);
        sliderLine2Size.Value = size2;
        lblLine2Size.Text = $"{(int)size2} px";

        entryLine3Text.Text = Preferences.Default.Get("title_line3_text", "");
        _line3Color = Preferences.Default.Get("title_line3_color", "White");
        double size3 = Preferences.Default.Get("title_line3_size", 16.0);
        sliderLine3Size.Value = size3;
        lblLine3Size.Text = $"{(int)size3} px";

        entryLine4Text.Text = Preferences.Default.Get("title_line4_text", "PhotoboothParty v" + AppInfo.Current.VersionString);
        _line4Color = Preferences.Default.Get("title_line4_color", "Gray");
        double size4 = Preferences.Default.Get("title_line4_size", 14.0);
        sliderLine4Size.Value = size4;
        lblLine4Size.Text = $"{(int)size4} px";

        InitializeColorPickers();

        _flashMode = Preferences.Default.Get("camera_flash_mode", "Auto");
        UpdateFlashModeSelection(_flashMode);

        switchKeepScreenOn.IsToggled = Preferences.Default.Get("keep_screen_on", true);
        switchPlaySound.IsToggled = Preferences.Default.Get("play_shutter_sound", true);
        switchPlayFlash.IsToggled = Preferences.Default.Get("play_flash_effect", true);
        
        bool useDefaultGallery = Preferences.Default.Get("use_default_gallery", true);
        switchUseDefaultGallery.IsToggled = useDefaultGallery;
        layoutCustomFolder.IsVisible = !useDefaultGallery;
        
        _tempCustomFolderPath = Preferences.Default.Get("custom_gallery_path", "");
        lblCustomFolderPath.Text = string.IsNullOrWhiteSpace(_tempCustomFolderPath) ? PhotoboothParty.Resources.Strings.AppResources.SettingsSelectFolder : _tempCustomFolderPath;

        _tempBgImagePath = Preferences.Default.Get("background_image_path", "");
        UpdateBgImageLabelAndButtons();

        WeakReferenceMessenger.Default.Register<ShutterPressedMessage>(this, (r, m) =>
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                lblBluetoothTest.Text = "Signal Re\u00E7u \u2705 !";
                lblBluetoothTest.TextColor = Colors.LightGreen;
            });
        });
    }

    private void InitializeColorPickers()
    {
        PopulateColorStack(stackLine1Colors, 1, _line1Color);
        PopulateColorStack(stackLine2Colors, 2, _line2Color);
        PopulateColorStack(stackLine3Colors, 3, _line3Color);
        PopulateColorStack(stackLine4Colors, 4, _line4Color);
    }

    private void PopulateColorStack(HorizontalStackLayout stack, int lineIndex, string currentColor)
    {
        stack.Children.Clear();
        foreach (var preset in ColorPresets)
        {
            bool isSelected = false;
            if (currentColor.StartsWith("#", StringComparison.Ordinal))
            {
                isSelected = string.Equals(preset.Hex, currentColor, StringComparison.OrdinalIgnoreCase);
            }
            else
            {
                isSelected = string.Equals(preset.Name, currentColor, StringComparison.OrdinalIgnoreCase);
            }

            var border = new Border
            {
                WidthRequest = 36,
                HeightRequest = 36,
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = new CornerRadius(18) },
                StrokeThickness = 3,
                Stroke = isSelected ? Color.Parse("#007ACC") : Colors.Transparent,
                BackgroundColor = Colors.Transparent,
                Padding = 2,
                VerticalOptions = LayoutOptions.Center
            };

            var innerCircle = new Border
            {
                WidthRequest = 26,
                HeightRequest = 26,
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = new CornerRadius(13) },
                StrokeThickness = 0,
                BackgroundColor = Color.Parse(preset.Hex),
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Center
            };

            border.Content = innerCircle;

            var tapGesture = new TapGestureRecognizer();
            tapGesture.Tapped += (s, e) =>
            {
                OnColorSelected(lineIndex, preset.Hex, stack);
            };
            border.GestureRecognizers.Add(tapGesture);

            stack.Children.Add(border);
        }
    }

    private void OnColorSelected(int lineIndex, string selectedHex, HorizontalStackLayout stack)
    {
        if (lineIndex == 1) _line1Color = selectedHex;
        else if (lineIndex == 2) _line2Color = selectedHex;
        else if (lineIndex == 3) _line3Color = selectedHex;
        else if (lineIndex == 4) _line4Color = selectedHex;

        PopulateColorStack(stack, lineIndex, selectedHex);
    }

    private void OnLine1SizeChanged(object sender, ValueChangedEventArgs e)
    {
        if (lblLine1Size != null)
            lblLine1Size.Text = $"{(int)e.NewValue} px";
    }

    private void OnLine2SizeChanged(object sender, ValueChangedEventArgs e)
    {
        if (lblLine2Size != null)
            lblLine2Size.Text = $"{(int)e.NewValue} px";
    }

    private void OnLine3SizeChanged(object sender, ValueChangedEventArgs e)
    {
        if (lblLine3Size != null)
            lblLine3Size.Text = $"{(int)e.NewValue} px";
    }

    private void OnLine4SizeChanged(object sender, ValueChangedEventArgs e)
    {
        if (lblLine4Size != null)
            lblLine4Size.Text = $"{(int)e.NewValue} px";
    }

    private void OnFlashModeClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is string mode)
        {
            UpdateFlashModeSelection(mode);
        }
    }

    private void UpdateFlashModeSelection(string mode)
    {
        _flashMode = mode;

        if (btnFlashAuto == null || btnFlashOn == null || btnFlashOff == null)
            return;

        // Reset backgrounds and text colors
        btnFlashAuto.BackgroundColor = Color.Parse("#2D2D2D");
        btnFlashAuto.TextColor = Colors.Silver;
        btnFlashAuto.FontAttributes = FontAttributes.None;

        btnFlashOn.BackgroundColor = Color.Parse("#2D2D2D");
        btnFlashOn.TextColor = Colors.Silver;
        btnFlashOn.FontAttributes = FontAttributes.None;

        btnFlashOff.BackgroundColor = Color.Parse("#2D2D2D");
        btnFlashOff.TextColor = Colors.Silver;
        btnFlashOff.FontAttributes = FontAttributes.None;

        // Highlight selected
        if (mode == "Auto")
        {
            btnFlashAuto.BackgroundColor = Color.Parse("#007ACC");
            btnFlashAuto.TextColor = Colors.White;
            btnFlashAuto.FontAttributes = FontAttributes.Bold;
        }
        else if (mode == "On")
        {
            btnFlashOn.BackgroundColor = Color.Parse("#007ACC");
            btnFlashOn.TextColor = Colors.White;
            btnFlashOn.FontAttributes = FontAttributes.Bold;
        }
        else if (mode == "Off")
        {
            btnFlashOff.BackgroundColor = Color.Parse("#007ACC");
            btnFlashOff.TextColor = Colors.White;
            btnFlashOff.FontAttributes = FontAttributes.Bold;
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        WeakReferenceMessenger.Default.Unregister<ShutterPressedMessage>(this);
    }

    private void OnDefaultGalleryToggled(object sender, ToggledEventArgs e)
    {
        layoutCustomFolder.IsVisible = !e.Value;
    }

    private async void OnSelectFolderClicked(object sender, EventArgs e)
    {
        try
        {
            var result = await CommunityToolkit.Maui.Storage.FolderPicker.Default.PickAsync(default);
            if (result.IsSuccessful)
            {
                _tempCustomFolderPath = result.Folder.Path;
                lblCustomFolderPath.Text = _tempCustomFolderPath;
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erreur", $"Impossible de s\u00E9lectionner le dossier : {ex.Message}", "OK");
        }
    }

    private void UpdateBgImageLabelAndButtons()
    {
        if (string.IsNullOrWhiteSpace(_tempBgImagePath) || !File.Exists(_tempBgImagePath))
        {
            lblBgImagePath.Text = PhotoboothParty.Resources.Strings.AppResources.SettingsNoImageSelected;
            btnDeleteBgImage.IsEnabled = false;
        }
        else
        {
            lblBgImagePath.Text = Path.GetFileName(_tempBgImagePath);
            btnDeleteBgImage.IsEnabled = true;
        }
    }

    private async void OnSelectBgImageClicked(object sender, EventArgs e)
    {
        try
        {
            var options = new PickOptions
            {
                PickerTitle = "Choisir une image de fond",
                FileTypes = FilePickerFileType.Images
            };
            var result = await FilePicker.Default.PickAsync(options);
            if (result != null)
            {
                string targetDir = FileSystem.Current.AppDataDirectory;
                string fileExt = Path.GetExtension(result.FileName);
                string targetPath = Path.Combine(targetDir, $"bg_image_{DateTime.Now.Ticks}{fileExt}");

                using (var sourceStream = await result.OpenReadAsync())
                using (var targetStream = File.OpenWrite(targetPath))
                {
                    await sourceStream.CopyToAsync(targetStream);
                }

                if (!string.IsNullOrWhiteSpace(_tempBgImagePath) && File.Exists(_tempBgImagePath))
                {
                    try { File.Delete(_tempBgImagePath); } catch { }
                }

                _tempBgImagePath = targetPath;
                UpdateBgImageLabelAndButtons();
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erreur", $"Impossible de s\u00E9lectionner l'image : {ex.Message}", "OK");
        }
    }

    private void OnDeleteBgImageClicked(object sender, EventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(_tempBgImagePath) && File.Exists(_tempBgImagePath))
        {
            try { File.Delete(_tempBgImagePath); } catch { }
        }
        _tempBgImagePath = "";
        UpdateBgImageLabelAndButtons();
    }

    private async void OnCancelClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        if (int.TryParse(entryCaptureTimer.Text, out int captureTimer))
            Preferences.Default.Set("capture_timer", captureTimer);
            
        if (int.TryParse(entryPreviewDuration.Text, out int previewDuration))
            Preferences.Default.Set("preview_duration", previewDuration);
            
        if (int.TryParse(entryInactivityTimeout.Text, out int inactivityTimeout))
            Preferences.Default.Set("inactivity_timeout", inactivityTimeout);

        Preferences.Default.Set("title_line1_text", entryLine1Text.Text ?? "");
        Preferences.Default.Set("title_line1_color", _line1Color);
        Preferences.Default.Set("title_line1_size", sliderLine1Size.Value);

        Preferences.Default.Set("title_line2_text", entryLine2Text.Text ?? "");
        Preferences.Default.Set("title_line2_color", _line2Color);
        Preferences.Default.Set("title_line2_size", sliderLine2Size.Value);

        Preferences.Default.Set("title_line3_text", entryLine3Text.Text ?? "");
        Preferences.Default.Set("title_line3_color", _line3Color);
        Preferences.Default.Set("title_line3_size", sliderLine3Size.Value);

        Preferences.Default.Set("title_line4_text", entryLine4Text.Text ?? "");
        Preferences.Default.Set("title_line4_color", _line4Color);
        Preferences.Default.Set("title_line4_size", sliderLine4Size.Value);

        Preferences.Default.Set("keep_screen_on", switchKeepScreenOn.IsToggled);
        DeviceDisplay.Current.KeepScreenOn = switchKeepScreenOn.IsToggled;
        
        Preferences.Default.Set("play_shutter_sound", switchPlaySound.IsToggled);
        Preferences.Default.Set("play_flash_effect", switchPlayFlash.IsToggled);
        Preferences.Default.Set("camera_flash_mode", _flashMode);

        Preferences.Default.Set("use_default_gallery", switchUseDefaultGallery.IsToggled);
        Preferences.Default.Set("custom_gallery_path", _tempCustomFolderPath);
        Preferences.Default.Set("background_image_path", _tempBgImagePath);

        await Navigation.PopAsync();
    }
}
