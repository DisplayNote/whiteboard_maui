using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Core.Views;
using CommunityToolkit.Maui.Views;
using WhiteboardMaui.Core.Models;

namespace WhiteboardMaui.Core.Controls
{
    /// <summary>
    /// A whiteboard canvas control that supports drawing, undo/redo, and saving
    /// </summary>
    public class WhiteboardCanvas : ContentView
    {
        private readonly DrawingView _drawingView;
        private readonly Stack<IDrawingLine> _redoStack = new();

        #region Public Events

        /// <summary>
        /// Fired when a line is drawn on the canvas
        /// </summary>
        public event EventHandler<LineDrawnEventArgs>? LineDrawn;

        /// <summary>
        /// Fired when the canvas is cleared
        /// </summary>
        public event EventHandler<CanvasClearedEventArgs>? CanvasCleared;

        /// <summary>
        /// Fired when undo/redo state changes
        /// </summary>
        public event EventHandler<UndoRedoStateChangedEventArgs>? UndoRedoStateChanged;

        /// <summary>
        /// Fired when a drawing is saved
        /// </summary>
        public event EventHandler<DrawingSavedEventArgs>? DrawingSaved;

        #endregion

        #region Bindable Properties

        public static readonly BindableProperty LineColorProperty =
            BindableProperty.Create(
                nameof(LineColor),
                typeof(Color),
                typeof(WhiteboardCanvas),
                Colors.Black,
                propertyChanged: OnLineColorChanged);

        public static readonly BindableProperty LineWidthProperty =
            BindableProperty.Create(
                nameof(LineWidth),
                typeof(float),
                typeof(WhiteboardCanvas),
                5f,
                propertyChanged: OnLineWidthChanged);

        public Color LineColor
        {
            get => (Color)GetValue(LineColorProperty);
            set => SetValue(LineColorProperty, value);
        }

        public float LineWidth
        {
            get => (float)GetValue(LineWidthProperty);
            set => SetValue(LineWidthProperty, value);
        }

        private static void OnLineColorChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is WhiteboardCanvas canvas && newValue is Color color)
            {
                canvas._drawingView.LineColor = color;
            }
        }

        private static void OnLineWidthChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is WhiteboardCanvas canvas && newValue is float width)
            {
                canvas._drawingView.LineWidth = width;
            }
        }

        #endregion

        #region Constructor

        public WhiteboardCanvas()
        {
            _drawingView = new DrawingView
            {
                BackgroundColor = Colors.White,
                LineColor = LineColor,
                LineWidth = LineWidth,
                IsMultiLineModeEnabled = true,
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Fill
            };

            _drawingView.DrawingLineCompleted += OnDrawingLineCompleted;

            Content = _drawingView;
        }

        #endregion

        #region Event Handlers

        private void OnDrawingLineCompleted(object? sender, DrawingLineCompletedEventArgs e)
        {
            _redoStack.Clear();
            LineDrawn?.Invoke(this, new LineDrawnEventArgs(_drawingView.Lines.Count));
            NotifyUndoRedoStateChanged();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Undo the last drawing action
        /// </summary>
        /// <returns>True if undo was successful, false if nothing to undo</returns>
        public bool Undo()
        {
            if (_drawingView.Lines.Count > 0)
            {
                var lastIndex = _drawingView.Lines.Count - 1;
                var line = _drawingView.Lines[lastIndex];
                _drawingView.Lines.RemoveAt(lastIndex);
                _redoStack.Push(line);
                NotifyUndoRedoStateChanged();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Redo the last undone action
        /// </summary>
        /// <returns>True if redo was successful, false if nothing to redo</returns>
        public bool Redo()
        {
            if (_redoStack.Count > 0)
            {
                var line = _redoStack.Pop();
                _drawingView.Lines.Add(line);
                NotifyUndoRedoStateChanged();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Clear all lines from the canvas
        /// </summary>
        public void Clear()
        {
            var previousCount = _drawingView.Lines.Count;
            _drawingView.Lines.Clear();
            _redoStack.Clear();
            CanvasCleared?.Invoke(this, new CanvasClearedEventArgs(previousCount));
            NotifyUndoRedoStateChanged();
        }

        /// <summary>
        /// Save the drawing as an image
        /// </summary>
        /// <param name="filePath">Path where to save the file</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if save was successful</returns>
        public async Task<bool> SaveAsync(string filePath, CancellationToken cancellationToken = default)
        {
            try
            {
                if (_drawingView.Lines.Count == 0)
                {
                    DrawingSaved?.Invoke(this, new DrawingSavedEventArgs(filePath, false, "Nothing to save"));
                    return false;
                }

                var bg = Colors.White.AsPaint();
                using var stream = await DrawingViewService.GetImageStream(
                    ImageLineOptions.FullCanvas(
                        [.. _drawingView.Lines],
                        new Size((int)_drawingView.Width, (int)_drawingView.Height),
                        bg,
                        new Size(_drawingView.Width, _drawingView.Height)),
                    cancellationToken);

                if (stream != null)
                {
                    var directory = Path.GetDirectoryName(filePath);
                    if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }

                    using var fileStream = File.Create(filePath);
                    stream.Seek(0, SeekOrigin.Begin);
                    await stream.CopyToAsync(fileStream, cancellationToken);

                    DrawingSaved?.Invoke(this, new DrawingSavedEventArgs(filePath, true));
                    return true;
                }

                DrawingSaved?.Invoke(this, new DrawingSavedEventArgs(filePath, false, "Failed to generate image stream"));
                return false;
            }
            catch (Exception ex)
            {
                DrawingSaved?.Invoke(this, new DrawingSavedEventArgs(filePath, false, ex.Message));
                return false;
            }
        }

        /// <summary>
        /// Save the drawing as an image
        /// </summary>
        /// <param name="fileStream">Stream to the file</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if save was successful</returns>
        public async Task<bool> SaveAsync(Stream fileStream, string fileFullPath, CancellationToken cancellationToken = default)
        {
            try
            {
                if (_drawingView.Lines.Count == 0)
                {
                    DrawingSaved?.Invoke(this, new DrawingSavedEventArgs(fileFullPath, false, "Nothing to save"));
                    return false;
                }

                var bg = Colors.White.AsPaint();
                using var stream = await DrawingViewService.GetImageStream(
                    ImageLineOptions.FullCanvas(
                        [.. _drawingView.Lines],
                        new Size((int)_drawingView.Width, (int)_drawingView.Height),
                        bg,
                        new Size(_drawingView.Width, _drawingView.Height)),
                    cancellationToken);

                if (stream != null)
                {
                    stream.Seek(0, SeekOrigin.Begin);
                    await stream.CopyToAsync(fileStream, cancellationToken);

                    DrawingSaved?.Invoke(this, new DrawingSavedEventArgs(fileFullPath, true));
                    return true;
                }

                DrawingSaved?.Invoke(this, new DrawingSavedEventArgs(fileFullPath, false, "Failed to generate image stream"));
                return false;
            }
            catch (Exception ex)
            {
                DrawingSaved?.Invoke(this, new DrawingSavedEventArgs(fileFullPath, false, ex.Message));
                return false;
            }
        }

        /// <summary>
        /// Get current number of lines on the canvas
        /// </summary>
        public int LineCount => _drawingView.Lines.Count;

        /// <summary>
        /// Check if undo is available
        /// </summary>
        public bool CanUndo => _drawingView.Lines.Count > 0;

        /// <summary>
        /// Check if redo is available
        /// </summary>
        public bool CanRedo => _redoStack.Count > 0;

        #endregion

        #region Private Methods

        private void NotifyUndoRedoStateChanged()
        {
            UndoRedoStateChanged?.Invoke(this, new UndoRedoStateChangedEventArgs(
                CanUndo,
                CanRedo,
                _drawingView.Lines.Count,
                _redoStack.Count));
        }

        #endregion
    }
}
