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

/*
 * This code is taken from http://www.codeproject.com/KB/WPF/RecentFileList.aspx and is covered by the http://www.codeproject.com/info/cpol10.aspx
 */ 

namespace OxTailLogic.Persistance
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public interface IPersist
    {
        List<string> RecentFiles(int max);
        void InsertFile(string filepath, int max);
        void RemoveFile(string filepath, int max);
    }
}
