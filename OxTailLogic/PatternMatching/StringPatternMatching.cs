/*****************************************************************
* This file is part of OxTail.
*
* OxTail is free software: you can redistribute it and/or modify
* it under the terms of the GNU General Public License as published by
* the Free Software Foundation, either version 3 of the License, or
* (at your option) any later version.
*
* OxTail is distributed in the hope that it will be useful,
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
* GNU General Public License for more details.
*
* You should have received a copy of the GNU General Public License
* along with OxTail.  If not, see <http://www.gnu.org/licenses/>.
* ********************************************************************/

namespace OxTailLogic.PatternMatching
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Windows.Documents;
    using System.Windows.Controls;

    public class StringPatternMatching : IStringPatternMatching
    {
        /// <summary>
        /// Factory to create the <see cref="IStringPatternMatching"/> interface
        /// </summary>
        /// <returns></returns>
        public static IStringPatternMatching CreatePatternMatching()
        {
            return new StringPatternMatching();
        }

        /// <summary>
        /// <para>
        /// Match the pattern in the text.       
        /// </para>
        /// </summary>
        /// <param name="text">The text to find the pattern in</param>
        /// <param name="pattern">The pattern to find in the text, cannot be null</param>
        /// <returns>A <see cref="bool"/> whether the pattern was found in the text, cannot not be null</returns>
        /// <exception cref="TextToMatchWasNullException"/>
        /// <exception cref="PatternToMatchWasNullException"/>
        public bool MatchPattern(string text, string pattern)
        {
            if (text == null)
            {
                throw new TextToMatchWasNullException();
            }

            if (pattern == null)
            {
                throw new PatternToMatchWasNullException();
            }

            // blank pattern means match everything
            if (string.IsNullOrEmpty(pattern) || (string.IsNullOrEmpty(text) && string.IsNullOrEmpty(pattern)))
            {
                return true;
            }

            if (text == null)
            {
                text = string.Empty;
            }

            Regex regEx = new Regex(pattern);
            return regEx.IsMatch(text);
        }
    }
}
