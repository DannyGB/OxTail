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

namespace OxTailLogic.Persistance
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Reflection;

    static class ApplicationAttributes
    {
        static readonly Assembly _Assembly = null;

        static readonly AssemblyTitleAttribute _Title = null;
        static readonly AssemblyCompanyAttribute _Company = null;
        static readonly AssemblyCopyrightAttribute _Copyright = null;
        static readonly AssemblyProductAttribute _Product = null;

        public static string Title { get; private set; }
        public static string CompanyName { get; private set; }
        public static string Copyright { get; private set; }
        public static string ProductName { get; private set; }

        static Version _Version = null;
        public static string Version { get; private set; }

        static ApplicationAttributes()
        {
            try
            {
                Title = String.Empty;
                CompanyName = String.Empty;
                Copyright = String.Empty;
                ProductName = String.Empty;
                Version = String.Empty;

                _Assembly = Assembly.GetEntryAssembly();

                if (_Assembly != null)
                {
                    object[] attributes = _Assembly.GetCustomAttributes(false);

                    foreach (object attribute in attributes)
                    {
                        Type type = attribute.GetType();

                        if (type == typeof(AssemblyTitleAttribute)) _Title = (AssemblyTitleAttribute)attribute;
                        if (type == typeof(AssemblyCompanyAttribute)) _Company = (AssemblyCompanyAttribute)attribute;
                        if (type == typeof(AssemblyCopyrightAttribute)) _Copyright = (AssemblyCopyrightAttribute)attribute;
                        if (type == typeof(AssemblyProductAttribute)) _Product = (AssemblyProductAttribute)attribute;
                    }

                    _Version = _Assembly.GetName().Version;
                }

                if (_Title != null) Title = _Title.Title;
                if (_Company != null) CompanyName = _Company.Company;
                if (_Copyright != null) Copyright = _Copyright.Copyright;
                if (_Product != null) ProductName = _Product.Product;
                if (_Version != null) Version = _Version.ToString();
            }
            catch { }
        }
    }
}
