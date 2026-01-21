using WhiteboardMaui.Core.Models;

namespace WhiteboardMaui
{
    public partial class MainPage : ContentPage
    {
        private Border? _selectedColorButton;
        private Button? _selectedThicknessButton;

        public MainPage()
        {
            InitializeComponent();
            SelectColorButton(BlackButton);
            SelectThicknessButton(MediumButton);
        }

        #region Library Event Handlers

        private void OnLineDrawn(object? sender, LineDrawnEventArgs e)
        {
        }

        private void OnCanvasCleared(object? sender, CanvasClearedEventArgs e)
        {
        }

        private void OnUndoRedoStateChanged(object? sender, UndoRedoStateChangedEventArgs e)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                UndoButton.IsEnabled = e.CanUndo;
                RedoButton.IsEnabled = e.CanRedo;
            });
        }

        private void OnDrawingSaved(object? sender, DrawingSavedEventArgs e)
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                if (e.Success)
                {
                    await DisplayAlert("Success",
                        $"Drawing saved to:\n{e.FilePath}",
                        "Done");
                }
                else
                {
                    await DisplayAlert("Error", e.ErrorMessage ?? "Failed to save drawing", "OK");
                }
            });
        }

        #endregion

        #region UI Event Handlers

        private void OnColorSelected(object? sender, EventArgs e)
        {
            if (sender is Border colorButton)
            {
                SelectColorButton(colorButton);
                WhiteboardCanvas.LineColor = colorButton.BackgroundColor;
            }
        }

        private void SelectColorButton(Border colorButton)
        {
            if (_selectedColorButton != null)
            {
                _selectedColorButton.Stroke = Colors.Transparent;
                _selectedColorButton.StrokeThickness = 3;
            }

            colorButton.Stroke = Color.FromArgb("#2196F3");
            colorButton.StrokeThickness = 4;
            _selectedColorButton = colorButton;
        }

        private void OnThicknessSelected(object? sender, EventArgs e)
        {
            if (sender is Button thicknessButton)
            {
                SelectThicknessButton(thicknessButton);
                WhiteboardCanvas.LineWidth = thicknessButton.Text switch
                {
                    "Thin" => 2f,
                    "Medium" => 5f,
                    "Thick" => 10f,
                    _ => 5f
                };
            }
        }

        private void SelectThicknessButton(Button thicknessButton)
        {
            if (_selectedThicknessButton != null)
            {
                _selectedThicknessButton.BackgroundColor = Colors.White;
                _selectedThicknessButton.TextColor = Color.FromArgb("#333");
                _selectedThicknessButton.BorderColor = Color.FromArgb("#DDD");
            }

            thicknessButton.BackgroundColor = Color.FromArgb("#2196F3");
            thicknessButton.TextColor = Colors.White;
            thicknessButton.BorderColor = Color.FromArgb("#2196F3");
            _selectedThicknessButton = thicknessButton;
        }

        private void OnUndoClicked(object? sender, EventArgs e)
        {
            WhiteboardCanvas.Undo();
        }

        private void OnRedoClicked(object? sender, EventArgs e)
        {
            WhiteboardCanvas.Redo();
        }

        private void OnClearClicked(object? sender, EventArgs e)
        {
            WhiteboardCanvas.Clear();
        }

        private async void OnSaveClicked(object? sender, EventArgs e)
        {
            try
            {
#if ANDROID
                if (!await EnsureStoragePermissionAsync())
                {
                    await DisplayAlert("Error", "Storage permission is required to save drawings.", "OK");
                    return;
                }
#endif

                if (WhiteboardCanvas.LineCount == 0)
                {
                    await DisplayAlert("Info", "Nothing to save! Please draw something first.", "OK");
                    return;
                }

                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var filename = $"Whiteboard_{timestamp}.jpg";
                string? targetPath;

#if ANDROID
                targetPath = await SaveImageToGalleryAsync(filename);
                if (targetPath == null)
                    throw new Exception("Failed to save image to gallery.");
#elif IOS || MACCATALYST
                var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                targetPath = Path.Combine(documentsPath, filename);
                await WhiteboardCanvas.SaveAsync(targetPath);
#elif WINDOWS
                var picturesPath = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
                targetPath = Path.Combine(picturesPath, filename);
                await WhiteboardCanvas.SaveAsync(targetPath);
#else
                var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                targetPath = Path.Combine(documentsPath, filename);
                await WhiteboardCanvas.SaveAsync(targetPath);
#endif
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
            }
        }

#if ANDROID
        async Task<bool> EnsureStoragePermissionAsync()
        {
            var status = await Permissions.CheckStatusAsync<Permissions.StorageWrite>();
            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.StorageWrite>();
            }
            return status == PermissionStatus.Granted;
        }

        private async Task<string?> SaveImageToGalleryAsync(string filename)
        {
            try
            {
                var resolver = Android.App.Application.Context.ContentResolver;
                if (resolver == null)
                {
                    await DisplayAlert("Error", "ContentResolver is null", "OK");
                    return null;
                }

                var externalContentUri = Android.Provider.MediaStore.Images.Media.ExternalContentUri;
                if (externalContentUri == null)
                {
                    await DisplayAlert("Error", "ExternalContentUri is null", "OK");
                    return null;
                }

                var contentValues = new Android.Content.ContentValues();
                contentValues.Put(Android.Provider.MediaStore.IMediaColumns.DisplayName, filename);
                contentValues.Put(Android.Provider.MediaStore.IMediaColumns.MimeType, "image/jpeg");
                contentValues.Put(Android.Provider.MediaStore.IMediaColumns.RelativePath, Android.OS.Environment.DirectoryPictures);

                var uri = resolver.Insert(externalContentUri, contentValues);
                if (uri == null)
                {
                    await DisplayAlert("Error", "Failed to create new MediaStore record.", "OK");
                    return null;
                }

                using (var stream = resolver.OpenOutputStream(uri))
                {
                    if (stream == null)
                    {
                        await DisplayAlert("Error", "Failed to get output stream.", "OK");
                        return null;
                    }

                    await WhiteboardCanvas.SaveAsync(stream, uri.ToString());
                }

                return uri.ToString();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
                return null;
            }
        }
#endif

        #endregion
    }
}
