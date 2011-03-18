/*****************************************************************
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
using System.Windows;

namespace OxTailHelpers
{
    public static class ResourceHelper
    {
        /// <summary>
        /// Gets the <see cref="String"/> associated with the <paramref name="key"/> from the <see cref="ResourceDictionary"/>
        /// Specified in <see cref="Constants.STRING_RESOURCE_URI"/>
        /// </summary>
        /// <param name="key">The key of the text in the resource file</param>
        /// <returns>The corresponding <see cref="String"/> for the <paramref name="Key"/></returns>
        public static string GetStringFromStringResourceFile(string key)
        {
            ResourceDictionary myresourcedictionary;
            myresourcedictionary = new ResourceDictionary();
            myresourcedictionary.Source = new Uri(Constants.STRING_RESOURCE_URI, UriKind.RelativeOrAbsolute);

            return (string)myresourcedictionary[key];

        }
    }
}
