using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OxTail.Controls
{
    [Serializable]
    public class FileWatcherSeekOutOfPhaseException : Exception
    {
        public FileWatcherSeekOutOfPhaseException() { }
        public FileWatcherSeekOutOfPhaseException(string message) : base(message) { }
        public FileWatcherSeekOutOfPhaseException(string message, Exception inner) : base(message, inner) { }
        protected FileWatcherSeekOutOfPhaseException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}
