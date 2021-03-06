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
