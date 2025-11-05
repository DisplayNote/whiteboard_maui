namespace WhiteboardMaui.Core.Models
{
    /// <summary>
    /// Event args for when a line is drawn on the canvas
    /// </summary>
    public class LineDrawnEventArgs : EventArgs
    {
        public LineDrawnEventArgs(int totalLines)
        {
            TotalLines = totalLines;
            DrawnAt = DateTime.Now;
        }

        /// <summary>
        /// Total number of lines currently on the canvas
        /// </summary>
        public int TotalLines { get; }

        /// <summary>
        /// Timestamp when the line was drawn
        /// </summary>
        public DateTime DrawnAt { get; }
    }

    /// <summary>
    /// Event args for when undo/redo state changes
    /// </summary>
    public class UndoRedoStateChangedEventArgs : EventArgs
    {
        public UndoRedoStateChangedEventArgs(bool canUndo, bool canRedo, int linesCount, int redoCount)
        {
            CanUndo = canUndo;
            CanRedo = canRedo;
            LinesCount = linesCount;
            RedoCount = redoCount;
        }

        /// <summary>
        /// Whether undo operation is available
        /// </summary>
        public bool CanUndo { get; }

        /// <summary>
        /// Whether redo operation is available
        /// </summary>
        public bool CanRedo { get; }

        /// <summary>
        /// Current number of lines on canvas
        /// </summary>
        public int LinesCount { get; }

        /// <summary>
        /// Number of lines available for redo
        /// </summary>
        public int RedoCount { get; }
    }

    /// <summary>
    /// Event args for when the canvas is cleared
    /// </summary>
    public class CanvasClearedEventArgs : EventArgs
    {
        public CanvasClearedEventArgs(int previousLineCount)
        {
            PreviousLineCount = previousLineCount;
            ClearedAt = DateTime.Now;
        }

        /// <summary>
        /// Number of lines that were on the canvas before clearing
        /// </summary>
        public int PreviousLineCount { get; }

        /// <summary>
        /// Timestamp when the canvas was cleared
        /// </summary>
        public DateTime ClearedAt { get; }
    }

    /// <summary>
    /// Event args for when a drawing is saved
    /// </summary>
    public class DrawingSavedEventArgs : EventArgs
    {
        public DrawingSavedEventArgs(string filePath, bool success, string? errorMessage = null)
        {
            FilePath = filePath;
            Success = success;
            ErrorMessage = errorMessage;
            SavedAt = DateTime.Now;
        }

        /// <summary>
        /// Path where the drawing was saved
        /// </summary>
        public string FilePath { get; }

        /// <summary>
        /// Whether the save operation was successful
        /// </summary>
        public bool Success { get; }

        /// <summary>
        /// Error message if save failed
        /// </summary>
        public string? ErrorMessage { get; }

        /// <summary>
        /// Timestamp when the drawing was saved
        /// </summary>
        public DateTime SavedAt { get; }
    }
}
