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
using OxTailLogic.PatternMatching;

namespace OxTail.Controls
{
    /// <summary>
    /// Find options
    /// </summary>
    public enum FindOptions
    {
        CurrentDocument,
        AllOpenDocuments
    }

    public class FindDetails
    {
        public event EventHandler<FindEventArgs> Initiated;
        public string FindCriteria { get; private set; }
        public FindOptions Options { get; private set; }
        public int LastFindIndex { get; set; }

        public FindDetails(string findCriteria, FindOptions options)
            : this(findCriteria, options, 0)
        {
        }

        public FindDetails(string findCriteria, FindOptions options, int lastFindIndex)
        {
            this.FindCriteria = findCriteria;
            this.LastFindIndex = lastFindIndex;
            this.Options = options;
        }

        public void InitiateSearch()
        {
            if (this.Initiated != null)
            {
                this.Initiated(this, new FindEventArgs(this));
            }
        }
    }
}
