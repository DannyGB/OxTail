using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OxTail.Controls
{
    /// <summary>
    /// Specifies how OxTail determines the end of lines
    /// </summary>
    public enum NewlineDetectionMode
    {
        /// <summary>
        /// Automatically selects the first line terminator encountered in the file
        /// </summary>
        Auto, 
        /// <summary>
        /// crlf - Carriage Return Line Feed 
        /// </summary>
        Windows,
        /// <summary>
        /// lf - Line Feed
        /// </summary>
        Unix,
        /// <summary>
        /// cr - Carriage Return
        /// </summary>
        Mac
    }
}
