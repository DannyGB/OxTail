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

        /// <summary>
        /// Gets the localised text for the given <paramref name="key"/>
        /// </summary>
        /// <param name="app">The current application as a <see cref="IApplication"/></param>
        /// <param name="key">The key to the text in the resource file</param>
        /// <returns>A <see cref="String"/> containing the text identified by the <paramref name="key"/></returns>
        public static string GetLocalisedText(IApplication app, string key)
        {
            return app.LanguageDictionary[0][key].ToString();
        }

        /// <summary>
        /// Loads the Assembly from disk whos name matches the assemblyNameForCulture 
        /// parameter Name. A also loads the contained loose XAML Culture Resource 
        /// Dictionary into the  current Applications Merged Dictionaries.
        /// </summary>
        /// <param name="assemblyNameForCulture">The assmebly name to load from disk, 
        /// that contains the culture resources</param>
        public static Uri LoadAssemblyAndGetResourceCultureFile()
        {
            if (currentCulture.Name.Substring(0, currentCulture.Name.IndexOf('-')) == Constants.DEFAULT_LANGUAGE)
            {
                return null;
            }

            string culturePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, string.Format(Constants.CULTURE_PATH, currentCulture.Name));

            if (!File.Exists(culturePath))
            {
                MessageBox.Show(Constants.NO_LANGUAGE, LanguageHelper.GetLocalisedText((System.Windows.Application.Current as IApplication), Constants.WARNING), MessageBoxButton.OK);

                return null;
            }

            Assembly ass = Assembly.LoadFile(culturePath);
            string assName = string.Format(Constants.CULTURE_TEMPLATE, currentCulture.Name);

            return GetResourceFromDll(assName, string.Format(Constants.STRING_RESOURCES_FILENAME, currentCulture.Name));
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
            string packUri = String.Format(Constants.RESOURCES_URI_TEMPLATE, assemblyName, resourceFileName);
            return new Uri(packUri, UriKind.Relative);
        }
    }
}
