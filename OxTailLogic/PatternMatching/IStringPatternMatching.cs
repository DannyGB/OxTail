﻿/*****************************************************************
*
* Copyright 2011 Dan Beavon
*
* This file is part of OXTail.
*
* OXTail is free software: you can redistribute it and/or modify
* it under the terms of the GNU General Public License as published by
* the Free Software Foundation, either version 3 of the License, or
* (at your option) any later version.
*
* OXTail is distributed in the hope that it will be useful,
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
    using System.Windows.Documents;
    using System.Text.RegularExpressions;

    public interface IStringPatternMatching
    {
        /// <summary>
        /// Match the pattern in the text
        /// </summary>
        /// <param name="text">The text to find the pattern in</param>
        /// <param name="pattern">The pattern to find in the text</param>
        /// <returns>A <see cref="bool"/> whether the pattern was found in the text</returns>
        bool MatchPattern(string text, string pattern);

        /// <summary>
        /// Matches the passed in pattern in the text and passes back a <see cref="MatcheCollection"/>
        /// </summary>
        /// <param name="text">The text to find the pattern in</param>
        /// <param name="pattern">The pattern to find in the text</param>
        /// <returns>A <see cref="MacthCollection"/> containig all of the found matches</returns>
        MatchCollection MatchPattern(string text, StringBuilder pattern);
    }
}
