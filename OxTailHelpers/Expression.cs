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

namespace OxTailHelpers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    [Serializable]
    public class Expression : IExpression
    {
        public int ID { get; set; }
        public string Text { get; set; }
        public string Name { get; set; }

        public Expression(int id, string text, string name)
        {
            this.ID = id;
            this.Text = text;
            this.Name = name;
        }

        public Expression()
            : this(0, string.Empty, string.Empty)
        {
        }

        public override string ToString()
        {
            return this.Name;
        }
    }
}
