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
        public static IStringPatternMatching CreatePatternMatching()
        {
            return new StringPatternMatching();
        }

        public bool MatchPattern(string text, string pattern)
        {
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
