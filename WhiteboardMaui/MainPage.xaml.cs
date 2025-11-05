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
                    var share = await DisplayAlert("Success",
                        $"Drawing saved to:\n{e.FilePath}\n\nWould you like to share it?",
                        "Share", "Done");

                    if (share)
                    {
                        await Share.Default.RequestAsync(new ShareFileRequest
                        {
                            Title = "Share Whiteboard Drawing",
                            File = new ShareFile(e.FilePath)
                        });
                    }
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
                if (WhiteboardCanvas.LineCount == 0)
                {
                    await DisplayAlert("Info", "Nothing to save! Please draw something first.", "OK");
                    return;
                }

                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var filename = $"Whiteboard_{timestamp}.jpg";
                string targetPath;

#if ANDROID
                var picturesPath = Android.OS.Environment.GetExternalStoragePublicDirectory(
                    Android.OS.Environment.DirectoryPictures)?.AbsolutePath;

                if (string.IsNullOrEmpty(picturesPath))
                    picturesPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

                targetPath = Path.Combine(picturesPath, filename);
#elif IOS || MACCATALYST
                var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                targetPath = Path.Combine(documentsPath, filename);
#elif WINDOWS
                var picturesPath = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
                targetPath = Path.Combine(picturesPath, filename);
#else
                var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                targetPath = Path.Combine(documentsPath, filename);
#endif

                await WhiteboardCanvas.SaveAsync(targetPath);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
            }
        }

        #endregion
    }
}
