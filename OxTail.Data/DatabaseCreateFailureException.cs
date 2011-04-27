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

namespace OxTail.Data
{
    [Serializable]
    public class DatabaseCreateFailureException : Exception
    {
        public string FilePath { get; set; }

        public DatabaseCreateFailureException() { }
        public DatabaseCreateFailureException(string filePath, string message) : base(message) { FilePath = filePath; }
        public DatabaseCreateFailureException(string message) : base(message) { }
        public DatabaseCreateFailureException(string message, Exception inner) : base(message, inner) { }
        public DatabaseCreateFailureException(string filePath, string message, Exception inner) : base(message, inner) { FilePath = filePath; }
        protected DatabaseCreateFailureException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }   
}
