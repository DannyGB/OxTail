using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace OxTail.Controls
{
    class FileWatcherProgressChangedUserState
    {
        public string MainStatusText { get; set; }
        public bool ProgressBarIndeterminate { get; set; }
        public Visibility ProgressBarVisibility { get; set; }
        public FileWatcherProgressChangedUserState(string status, bool progressBarIndeterminate, Visibility progressBarVisibility)
        {
            this.MainStatusText = status;
            this.ProgressBarIndeterminate = progressBarIndeterminate;
            this.ProgressBarVisibility = progressBarVisibility;
        }
    }
}
