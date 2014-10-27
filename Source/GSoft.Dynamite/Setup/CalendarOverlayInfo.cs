namespace GSoft.Dynamite.Setup
{
    using Microsoft.SharePoint;

    /// <summary>
    /// Calendar overlay colors
    /// </summary>
    public enum CalendarOverlayColor
    {
        /// <summary>
        /// The light yellow overlay color
        /// </summary>
        LightYellow = 1,

        /// <summary>
        /// The light green overlay color
        /// </summary>
        LightGreen = 2,

        /// <summary>
        /// The orange
        /// </summary>
        Orange = 3,

        /// <summary>
        /// The light turquoise overlay color
        /// </summary>
        LightTurquoise = 4,

        /// <summary>
        /// The pink overlay color
        /// </summary>
        Pink = 5,

        /// <summary>
        /// The light blue overlay color
        /// </summary>
        LightBlue = 6,

        /// <summary>
        /// The ice blue overlay color (1)
        /// </summary>
        IceBlue1 = 7,

        /// <summary>
        /// The ice blue overlay color (2)
        /// </summary>
        IceBlue2 = 8,

        /// <summary>
        /// The white overlay color
        /// </summary>
        White = 9
    }

    /// <summary>
    /// Calendar overlay information.
    /// </summary>
    public class CalendarOverlayInfo
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the color.
        /// </summary>
        /// <value>
        /// The color.
        /// </value>
        public CalendarOverlayColor Color { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [always show].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [always show]; otherwise, <c>false</c>.
        /// </value>
        public bool AlwaysShow { get; set; }

        /// <summary>
        /// Gets or sets the calendar URL.
        /// </summary>
        /// <value>
        /// The calendar URL.
        /// </value>
        public string CalendarUrl { get; set; }

        /// <summary>
        /// Gets or sets the name of the target view.
        /// </summary>
        /// <value>
        /// The name of the target view.
        /// </value>
        public string TargetViewName { get; set; }

        /// <summary>
        /// Gets or sets the name of the overlay view.
        /// </summary>
        /// <value>
        /// The name of the overlay view.
        /// </value>
        public string OverlayViewName { get; set; }
    }
}
