using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OxTailHelpers.Data;

namespace OxTailLogic.Data
{
    public class DataService<T> where T : IData
    {
        public static IData InitialiseDataService()
        {
            return (IData)System.Activator.CreateInstance(typeof(T));
        }
    }
}
