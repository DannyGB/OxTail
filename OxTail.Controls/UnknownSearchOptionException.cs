using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OxTailHelpers;

namespace OxTail.Controls
{
    [Serializable]
    public class UnknownSearchOptionException : Exception
    {
        public UnknownSearchOptionException() : this(ResourceHelper.GetStringFromStringResourceFile(Constants.UKNOWN_SEARCH_OPTION)) { }
        public UnknownSearchOptionException(string message) : base(message) { }
        public UnknownSearchOptionException(string message, Exception inner) : base(message, inner) { }
        protected UnknownSearchOptionException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}
