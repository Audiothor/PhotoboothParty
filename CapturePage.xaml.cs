using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Maui.Core;
using System.Threading;
using System.IO;

namespace PhotoboothParty;

public partial class CapturePage : ContentPage
{
    private CancellationTokenSource? _inactivityTokenSource;
    private bool _isProcessingCapture = false;
    private bool _isInPreviewMode = false;

    public CapturePage()
    {
        InitializeComponent();

        photoCamera.HandlerChanged += async (s, e) =>
        {
            await SetupFrontCameraAsync();
        };

        WeakReferenceMessenger.Default.Register<ShutterPressedMessage>(this, (r, m) =>
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await ProcessRemoteInput();
            });
        });
    }

    private async Task SetupFrontCameraAsync()
    {
        if (photoCamera.Handler?.MauiContext != null)
        {
            try
            {
                var cameraProvider = photoCamera.Handler.MauiContext.Services.GetService<ICameraProvider>();
                if (cameraProvider != null)
                {
                    if (cameraProvider.AvailableCameras == null || cameraProvider.AvailableCameras.Count == 0)
                        await cameraProvider.RefreshAvailableCameras(CancellationToken.None);

                    var frontCam = cameraProvider.AvailableCameras?.FirstOrDefault(c => c.Position == CameraPosition.Front);
                    var targetCam = frontCam ?? cameraProvider.AvailableCameras?.FirstOrDefault();
                    
                    MainThread.BeginInvokeOnMainThread(async () =>
                    {
                        var rearCam = cameraProvider.AvailableCameras?.FirstOrDefault(c => c.Position == CameraPosition.Rear);
                        
                        // Si la camÃ©ra native se fige sur la camÃ©ra arriÃ¨re par dÃ©faut, forcer une bascule
                        if (photoCamera.SelectedCamera != null && photoCamera.SelectedCamera.Position == CameraPosition.Front)
                        {
                            // dÃ©jÃ  bon, on ne fait rien
                        }
                        else
                        {
                            // SÃ©lectionner la camÃ©ra frontale
                            photoCamera.SelectedCamera = targetCam;
                        }

                        // Petite pause pour s'assurer que le composant natif a bien reÃ§u l'instruction
                        await Task.Delay(150);
                        photoCamera.SelectedCamera = targetCam;
                    });
                }
            }
            catch { }
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        StartInactivityCountdown();
        await Task.Delay(100); // Laisse le temps au Layout de s'afficher
        await SetupFrontCameraAsync();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        CancelInactivityCountdown();
        WeakReferenceMessenger.Default.Unregister<ShutterPressedMessage>(this);
    }

    private async Task ProcessRemoteInput()
    {
        if (_isProcessingCapture) return; 

        CancelInactivityCountdown(); 

        if (_isInPreviewMode)
        {
            // RÃˆGLE : Nouveau clic pendant l'aperÃ§u de 5s = interruption et nouvelle capture immÃ©diate
            _isInPreviewMode = false;
            gridPreview.IsVisible = false;
            gridCamera.IsVisible = true;
            await Task.Delay(100); // Laisse le temps Ã  la camÃ©ra de redevenir visible
            await SetupFrontCameraAsync();
        }

        await ExecuteCaptureSequence();
    }

    private async Task ExecuteCaptureSequence()
    {
        _isProcessingCapture = true;
        lblCaptureCountdown.IsVisible = true;

        int captureTimer = Microsoft.Maui.Storage.Preferences.Default.Get("capture_timer", 10);

        for (int i = captureTimer; i > 0; i--)
        {
            lblCaptureCountdown.Text = i.ToString();
            await Task.Delay(1000);
        }
        lblCaptureCountdown.IsVisible = false;
        
        // DÃ©clenchement du flash (blanc immÃ©diat)
        bool playFlash = Microsoft.Maui.Storage.Preferences.Default.Get("play_flash_effect", true);
        if (playFlash)
        {
            flashOverlay.Opacity = 1;
        }

        bool playSound = Microsoft.Maui.Storage.Preferences.Default.Get("play_shutter_sound", true);
        if (playSound)
        {
#if ANDROID
            try {
                var sound = new Android.Media.MediaActionSound();
                sound.Play(Android.Media.MediaActionSoundType.ShutterClick);
            } catch { }
#endif
        }

        var imageStream = await photoCamera.CaptureImage(CancellationToken.None);
        _isProcessingCapture = false;

        if (imageStream != null)
        {
            try
            {
                var ms = new MemoryStream();
                await imageStream.CopyToAsync(ms);
                ms.Position = 0;

                bool useDefaultGallery = Microsoft.Maui.Storage.Preferences.Default.Get("use_default_gallery", true);
                string customPath = Microsoft.Maui.Storage.Preferences.Default.Get("custom_gallery_path", "");

                string targetFolder = "";
                if (useDefaultGallery || string.IsNullOrWhiteSpace(customPath))
                {
#if ANDROID
                    var mediaDirs = Android.App.Application.Context.GetExternalMediaDirs();
                    if (mediaDirs != null && mediaDirs.Length > 0)
                    {
                        targetFolder = Path.Combine(mediaDirs[0].AbsolutePath, "PhotoboothParty");
                    }
                    else
                    {
                        targetFolder = Path.Combine(Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryPictures).AbsolutePath, "PhotoboothParty");
                    }
#else
                    targetFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "PhotoboothParty");
#endif
                }
                else
                {
                    targetFolder = customPath;
                }

                if (!Directory.Exists(targetFolder))
                    Directory.CreateDirectory(targetFolder);

                string filename = $"Photo_{DateTime.Now:yyyyMMdd_HHmmss}.jpg";
                string filePath = Path.Combine(targetFolder, filename);

                using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                {
                    await ms.CopyToAsync(fileStream);
                }
                ms.Position = 0;

#if ANDROID
                // IMPORTANT: Force the Android system to index the new image so it shows up in the Gallery app immediately
                Android.Media.MediaScannerConnection.ScanFile(Android.App.Application.Context, new string[] { filePath }, null, null);
#endif

                imgPreview.Source = ImageSource.FromStream(() => ms);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving photo: {ex.Message}");
                // Fallback direct preview sans sauvegarde si erreur de permission
                imgPreview.Source = ImageSource.FromStream(() => imageStream);
            }

            gridCamera.IsVisible = false;
            gridPreview.IsVisible = true;
            
            // Fait disparaÃ®tre le flash trÃ¨s rapidement (fade out)
            if (playFlash)
            {
                flashOverlay.FadeTo(0, 150, Easing.CubicOut);
            }

            await RunPreviewPersistenceTimer();
        }
        else
        {
            StartInactivityCountdown();
        }
    }

    private async Task RunPreviewPersistenceTimer()
    {
        _isInPreviewMode = true;

        int previewDuration = Microsoft.Maui.Storage.Preferences.Default.Get("preview_duration", 5);

        for (int i = previewDuration; i > 0; i--)
        {
            if (!_isInPreviewMode) return;
            lblPreviewCountdown.Text = $"{i}s";
            await Task.Delay(1000);
        }

        if (_isInPreviewMode)
        {
            // Au lieu de retourner au menu, on revient Ã  l'appareil photo !
            _isInPreviewMode = false;
            gridPreview.IsVisible = false;
            gridCamera.IsVisible = true;
            
            // Relancer le chronomÃ¨tre d'inactivitÃ© (s'ils ne font rien, retour Ã  l'accueil)
            StartInactivityCountdown();
            
            await Task.Delay(100);
            await SetupFrontCameraAsync();
        }
    }

    private void StartInactivityCountdown()
    {
        CancelInactivityCountdown();
        _inactivityTokenSource = new CancellationTokenSource();
        var token = _inactivityTokenSource.Token;

        int inactivityTimeout = Microsoft.Maui.Storage.Preferences.Default.Get("inactivity_timeout", 20);

        Task.Run(async () =>
        {
            try
            {
                await Task.Delay(inactivityTimeout * 1000, token);
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    if (Shell.Current.CurrentPage is CapturePage)
                    {
                        await Navigation.PopAsync();
                    }
                });
            }
            catch (TaskCanceledException) { }
        });
    }

    private void CancelInactivityCountdown()
    {
        _inactivityTokenSource?.Cancel();
        _inactivityTokenSource?.Dispose();
        _inactivityTokenSource = null;
    }
}
