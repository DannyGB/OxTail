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
using System.Threading;
using System.Reflection;
using System.IO;

namespace OxTailHelpers
{
    public static class LanguageHelper
    {
        private static System.Globalization.CultureInfo currentCulture = System.Globalization.CultureInfo.CurrentCulture;

        public static string GetLocalisedText(IApplication app, string key)
        {
            return app.LanguageDictionary[0][key].ToString();
        }

        /// <summary>
        /// Loads the Assembly from disk whos name is matches the assemblyNameForCulture 
        /// parameter Name. A also loads the contained loose XAML Culture Resource 
        /// Dictionary into the  current Applications Merged Dictionaries.
        /// </summary>
        /// <param name="assemblyNameForCulture">The assmebly name to load from disk, 
        /// that contains the culture resources</param>
        public static Uri LoadAssemblyAndGetResourceCultureFile()
        {
            if (currentCulture.Name.Substring(0, currentCulture.Name.IndexOf('-')) == "en")
            {
                return null;
            }

            string culturePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, string.Format(@"Culture\Culture_{0}.dll", currentCulture.Name));

            if (!File.Exists(culturePath))
            {
                MessageBox.Show(@"Your language is not available, defaulting to English (Why not write a translation? Visit: https://sourceforge.net/projects/oxtail/)", "Warning", MessageBoxButton.OK);

                return null;
            }

            Assembly ass = Assembly.LoadFile(culturePath);
            string assName = string.Format("Culture_{0}", currentCulture.Name);

            return GetResourceFromDll(assName, string.Format("StringResources_{0}.xaml", currentCulture.Name));
        }

        /// <summary>
        /// Gets a correctly formed Uri string for the assemblyName &  
        /// resourceFileName parameters
        /// </summary>
        /// <param name="assemblyName">Assembly name</param>
        /// <param name="resourceFileName">Assembly resource file name</param>
        /// <returns>Correctly correctly formed Uri string for the assemblyName & 
        /// resourceFileName parameters</returns>
        private static Uri GetResourceFromDll(string assemblyName, string resourceFileName)
        {
            string packUri = String.Format(@"/{0};component/Resources/{1}", assemblyName, resourceFileName);
            return new Uri(packUri, UriKind.Relative);
        }
    }
}
