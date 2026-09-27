using Infra.Display;

namespace Cli.Presenter
{
    /// <summary>
    /// Formats all text mode output: paths, renders, creation and removal reports.
    /// Commands decide what happens, this class decides how it looks.
    /// </summary>
    public sealed class TextPresenter
    {
        private readonly IDisplay _display;

        /// <summary>
        /// Initializes a presenter writing to the given display.
        /// </summary>
        /// <param name="display">The display for output.</param>
        public TextPresenter(IDisplay display)
        {
            ArgumentNullException.ThrowIfNull(display);
            _display = display;
        }

        /// <summary>
        /// Prints the current path.
        /// </summary>
        /// <param name="path">The current path.</param>
        public void ShowPath(string path) => _display.WriteLine(path);

        /// <summary>
        /// Prints the new position after navigation.
        /// </summary>
        /// <param name="path">The current path.</param>
        public void ShowMoved(string path) => _display.WriteLine($"Current: {path}.");

        /// <summary>
        /// Writes rendered document text as is.
        /// </summary>
        /// <param name="text">The rendered text.</param>
        public void ShowText(string text) => _display.Write(text);

        /// <summary>
        /// Reports a created element.
        /// </summary>
        /// <param name="kindName">The element class name.</param>
        /// <param name="displayName">The element display name.</param>
        /// <param name="id">The assigned id.</param>
        /// <param name="path">The creation position.</param>
        public void ShowCreated(string kindName, string displayName, string id, string path) =>
            _display.WriteLine($"Created {kindName} '{displayName}' [{id}] in {path}.");

        /// <summary>
        /// Reports a removed child element.
        /// </summary>
        /// <param name="displayName">The removed display name.</param>
        public void ShowRemoved(string displayName) =>
            _display.WriteLine($"Removed '{displayName}'.");

        /// <summary>
        /// Reports a removed current section with the new position.
        /// </summary>
        /// <param name="displayName">The removed display name.</param>
        /// <param name="path">The new current path.</param>
        public void ShowRemovedCurrent(string displayName, string path) =>
            _display.WriteLine($"Removed '{displayName}'. Current: {path}.");

        /// <summary>
        /// Reports a saved document file.
        /// </summary>
        /// <param name="path">The destination file path.</param>
        public void ShowSaved(string path) =>
            _display.WriteLine($"Saved to '{path}'.");

        /// <summary>
        /// Reports a loaded document file.
        /// </summary>
        /// <param name="path">The source file path.</param>
        public void ShowLoaded(string path) =>
            _display.WriteLine($"Loaded from '{path}'. Current: /.");
    }
}
