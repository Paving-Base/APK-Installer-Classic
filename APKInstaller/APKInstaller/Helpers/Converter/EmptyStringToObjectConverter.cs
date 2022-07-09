using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APKInstaller.Helpers.Converter
{
    /// <summary>
    /// This class converts a string value into a an object (if the value is null or empty returns the false value).
    /// Can be used to bind a visibility, a color or an image to the value of a string.
    /// </summary>
    public class EmptyStringToObjectConverter : EmptyObjectToObjectConverter
    {
        /// <summary>
        /// Checks string for emptiness.
        /// </summary>
        /// <param name="value">Value to be checked.</param>
        /// <returns>True if value is null or empty string, false otherwise.</returns>
        protected override bool CheckValueIsEmpty(object value)
        {
            return string.IsNullOrEmpty(value?.ToString());
        }
    }
}
