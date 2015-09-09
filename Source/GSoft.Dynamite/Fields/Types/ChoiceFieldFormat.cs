using System.Diagnostics.CodeAnalysis;

namespace GSoft.Dynamite.Fields.Types
{
    /// <summary>
    /// Formats supported by <see cref="ChoiceFieldFormat"/>
    /// </summary>
    public enum ChoiceFieldFormat
    {
        /// <summary>
        /// Default setting, provides editing options through a drop-down selection
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "Dropdown", Justification = "Dropdown is not a compound word in this case.")]
        Dropdown = 0,

        /// <summary>
        /// Provides options through a group of radio buttons
        /// </summary>
        RadioButtons = 1        
    }
}
