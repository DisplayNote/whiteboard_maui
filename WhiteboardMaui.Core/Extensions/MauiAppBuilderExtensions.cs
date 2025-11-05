using CommunityToolkit.Maui;

namespace WhiteboardMaui.Core.Extensions
{
    /// <summary>
    /// Extension methods for configuring Whiteboard services
    /// </summary>
    public static class MauiAppBuilderExtensions
    {
        /// <summary>
        /// Configures the application to use Whiteboard services and controls
        /// </summary>
        /// <param name="builder">The MauiAppBuilder instance</param>
        /// <returns>The MauiAppBuilder for method chaining</returns>
        public static MauiAppBuilder UseWhiteboardServices(this MauiAppBuilder builder)
        {
            // Ensure CommunityToolkit.Maui is registered
            builder.UseMauiCommunityToolkit();

            // Register any additional handlers or services here if needed in the future

            return builder;
        }
    }
}
