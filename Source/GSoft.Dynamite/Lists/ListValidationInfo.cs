using System;

namespace GSoft.Dynamite.Lists
{
    /// <summary>
    /// ListValidationInfo object to hold a validation formula and a validation message
    /// </summary>
    public class ListValidationInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ListValidationInfo"/> class.
        /// </summary>
        public ListValidationInfo()
        {
            this.ValidationFormula = string.Empty;
            this.ValidationMessage = string.Empty;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ListValidationInfo"/> class.
        /// </summary>
        /// <param name="formula">The formula.</param>
        public ListValidationInfo(string formula)
        {
            this.ValidationFormula = formula;
            this.ValidationMessage = string.Empty;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ListValidationInfo"/> class.
        /// </summary>
        /// <param name="formula">The formula.</param>
        /// <param name="message">The message.</param>
        public ListValidationInfo(string formula, string message)
        {
            this.ValidationFormula = formula;
            this.ValidationMessage = message;
        }

        /// <summary>
        /// Gets or sets the validation formula.
        /// </summary>
        /// <value>
        /// The validation formula.
        /// </value>
        public string ValidationFormula { get; set; }

        /// <summary>
        /// Gets or sets the validation message.
        /// </summary>
        /// <value>
        /// The validation message.
        /// </value>
        public string ValidationMessage { get; set; }
    }
}
