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
using OxTailHelpers.Data;
using OxTail.Data.SQLite;
using OxTailHelpers;
using System.Collections.ObjectModel;

namespace OxTailLogic.Data
{
    public class SavedExpressionData : ISavedExpressionsData
    {
        private SavedExpressionsDataHelper SavedExpressionsDataHelper { get; set; }

        public SavedExpressionData()
        {
            this.SavedExpressionsDataHelper = new SavedExpressionsDataHelper();
        }

        public ObservableCollection<Expression> Read()
        {
            return SavedExpressionsDataHelper.Read();
        }

        public ObservableCollection<Expression> Write(ObservableCollection<Expression> items)
        {
            return SavedExpressionsDataHelper.Write(items);
        }
    }
}
