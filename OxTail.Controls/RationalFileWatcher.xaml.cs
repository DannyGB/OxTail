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

namespace OxTail.Controls
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using System.Text;
    using System.Threading;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Documents;
    using System.Windows.Threading;
    using OxTailHelpers;
    using OxTailLogic.PatternMatching;
    using System.Windows.Media;
    using System.Windows.Controls.Primitives;
    using OxTailLogic;
    using System.Media;
    using OxTailLogic.Audio;

    /// <summary>
    /// Interaction logic for RationalFileWatcher.xaml
    /// </summary>
    public partial class RationalFileWatcher : UserControl, IDisposable
    {
        #region events & delegates

        /// <summary>
        /// The find operation has reached the end of the file
        /// </summary>
        public event EventHandler<EventArgs> FindFinished;
        private delegate void StatusNotificationDelegate(int progressPercentage, FileWatcherProgressChangedUserState userState);
        private delegate void Update(string text);
        private delegate int ReturnInt();
        private delegate object ReturnObject();

        /// <summary>
        /// Create a custom routed event by first registering a RoutedEventID
        /// This event uses the bubbling routing strategy
        /// </summary>
        public static readonly RoutedEvent FileChangedEvent = EventManager.RegisterRoutedEvent("FileChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(RationalFileWatcher));

        #endregion events & delegates

        #region data members

        private HighlightCollection<HighlightItem> _patterns;
        private BackgroundWorker _bw = null;
        private bool _followTail = true;
        private Encoding _tailEncoding = Encoding.Default;
        private string _filename;
        private DoWorkEventArgs _doWorkEventArgs;
        private static IStringPatternMatching _patternMatching = StringPatternMatching.CreatePatternMatching();
        private int _interval = 1000;
        private int _chunkSize = 16384;
        private ScrollViewer _scrollViewer;
        private FindDetails _findDetails;
        private List<int> previouslySelectedItems;       

        #endregion data members

        #region properties

        private bool PlaySound
        {
            get;
            set;
        }

        private bool SoundPlayed
        {
            get;
            set;
        }

        private List<int> PreviouslySelectedItems
        {
            get
            {
                if (this.previouslySelectedItems == null)
                {
                    this.previouslySelectedItems = new List<int>();
                }

                return previouslySelectedItems;
            }

            set
            {
                previouslySelectedItems = value;
            }
        }

        public List<HighlightedItem> SelectedItem { get; private set; }
        private string SearchText { get; set; }
        private int LastSearchIndex { get; set; }
        private DateTime LastFileWriteTime { get; set; }
        private long LastPositionReadTo { get; set; }
        private bool IsListInSearchMode { get; set; }
        private Stream LastReadStream { get; set; }
        private List<HighlightedItem> CollectionBackBuffer { get; set; }

        
        public FindDetails FindDetails
        {
            set
            {
                this._findDetails = value;
                this._findDetails.Initiated += new EventHandler<FindEventArgs>(_findDetails_Initiated);
            }
        }        

        public HighlightCollection<HighlightItem> Patterns
        {
            get
            {
                return this._patterns;
            }

            set
            {
                this._patterns = value;
            }
        }

        public Encoding TailEncoding
        {
            get
            {
                return this._tailEncoding;
            }

            set
            {
                this._tailEncoding = value;
            }
        }

        public NewlineDetectionMode NewlineDetectionMode
        {
            get;
            set;
        }

        private int Interval
        {
            get { return int.Parse(SettingsHelper.AppSettings[AppSettings.REFRESH_INTERVAL_KEY]); }
        }

        /// <summary>
        /// Determines whether this file watcher follows the tail
        /// </summary>
        public bool IsFollowTail
        {
            get { return this._followTail; }
            set { this._followTail = value; }
        }

        private ItemCollection Lines
        {
            get { return this.colourfulListView.Items; }
        }

        private int HorizontalScrollbarVisibilityOffset
        {
            get { return 0; }
        }

        private ScrollViewer ScrollViewer
        {
            get
            {
                // If the tab is not visible then the GetVisualChildren call returns 0
                // So whilst opening multiple tabs most tabs are not visible during the load.

                return (ScrollViewer)this.Dispatcher.Invoke((ReturnObject)(() =>
                    {
                        if (this._scrollViewer == null)
                        {
                            List<ScrollViewer> scrollviewers = this.colourfulListView.GetVisualChildren<ScrollViewer>();
                            if (scrollviewers != null && scrollviewers.Count > 0)
                            {
                                foreach (ScrollViewer scrollviewer in scrollviewers)
                                {
                                    if (scrollviewer.IsVisible)
                                    {
                                        this._scrollViewer = scrollviewer;
                                        break;
                                    }
                                }
                            }
                        }
                        return this._scrollViewer;
                    }));
            }
        }

        #endregion

        #region methods

        /// <summary>
        /// Determine if the file has changed since last read.
        /// </summary>
        /// <returns></returns>
        private bool IsFileReadyForReread()
        {
            DateTime dt = File.GetLastWriteTime(this._filename);
            if (dt != this.LastFileWriteTime)
            {
                this.LastFileWriteTime = dt;

                return true;
            }

            return false;
        }

        private StreamReader OpenFile()
        {
            try
            {
                return new StreamReader(new FileStream(this._filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, this._chunkSize), this._tailEncoding, true, this._chunkSize);
            }
            catch (Exception)
            {
                Thread.SpinWait(1000);

                try
                {
                    return new StreamReader(new FileStream(this._filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, this._chunkSize), this._tailEncoding, true, this._chunkSize);
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Remove items from the view collection when they are removed from the file
        /// </summary>
        /// <param name="index"></param>
        private void RemoveHightlightedItem(int index)
        {
            if (index <= (this.CollectionBackBuffer.Count - 1))
            {
                this.CollectionBackBuffer.RemoveAt(index);
            }
        }

        /// <summary>
        /// Add items to the view collection when the file is updated
        /// </summary>
        private int AddHightLightedItem(string line, int location)
        {
            return (int)this.Dispatcher.Invoke((ReturnInt)(() =>
                {
                    int index = location;
                    HighlightedItem item = GetHighlighting(line);

                    if (location >= this.colourfulListView.Items.Count || location == -1)
                    {
                        this.CollectionBackBuffer.Add(item);

                        return this.CollectionBackBuffer.Count - 1;
                    }
                    else
                    {
                        this.CollectionBackBuffer.RemoveAt(index);
                        this.CollectionBackBuffer.Insert(index, item);

                        return index;
                    }
                }));
        }

        /// <summary>
        /// Get the <see cref="HighlightedItem"/> for the current text, taking into account Find mode etc
        /// </summary>
        /// <param name="line">The line of text to pattern match</param>
        /// <returns>A <see cref="HighlightedItem"/></returns>
        private HighlightedItem GetHighlighting(string line)
        {
            HighlightedItem item = null;
            item = this.GetHighlightedItem(line);

            if (item == null)
            {
                throw new Exception("Item cannot be null!");
            }

            return item;
        }

        /// <summary>
        /// Gets the highlight item of a found piece of text
        /// </summary>
        /// <param name="line">The line of text to match</param>
        /// <returns></returns>
        private bool GetFoundHighlightItem(ref HighlightedItem item)
        {
            HighlightItem special = new HighlightItem(this.SearchText, Constants.DEFAULT_FORECOLOUR, Constants.DEFAULT_BACKCOLOUR);

            if (!string.IsNullOrEmpty(special.Pattern) && !string.IsNullOrEmpty(item.Text))
            {
                if (item.Text == special.Pattern || _patternMatching.MatchPattern(item.Text, special.Pattern))
                {
                    item.Selected = true;

                    return true;
                }
                else
                {
                    item.Selected = false;

                    return false;
                }
            }

            return false;
        }

        /// <summary>
        /// Gets the <see cref="HighlightedItem"/> for the given text once it has either been searched for and found or pattern matched
        /// </summary>
        /// <param name="text">The line of text to find/pattern match</param>
        private HighlightedItem GetHighlightedItem(string text)
        {
            HighlightedItem highlighted = new HighlightedItem();
            highlighted.Text = text;

            highlighted.ForeColour = Constants.DEFAULT_FORECOLOUR;
            highlighted.BackColour = Constants.DEFAULT_BACKCOLOUR;

            foreach (HighlightItem highlight in FindFirstHighlightByText(this.Patterns, text))
            {
                if (highlight.BorderColour != Constants.DEFAULT_NULL_COLOUR)
                {
                    highlighted.ForeColour = Constants.DEFAULT_FORECOLOUR;
                    highlighted.BackColour = Constants.DEFAULT_BACKCOLOUR;
                    highlighted.BorderColour = highlight.BorderColour;
                }
                else
                {
                    highlighted.ForeColour = highlight.ForeColour;
                    highlighted.BackColour = highlight.BackColour;
                    highlighted.BorderColour = Constants.DEFAULT_NULL_COLOUR;
                }

                // Only play a sound if we are told to by ReadLines() and the user
                if (PlaySound && bool.Parse(SettingsHelper.AppSettings[AppSettings.PLAY_SOUND]))
                {
                    AudioHelper.Play(SettingsHelper.AppSettings[AppSettings.PLAY_SOUND_FILE]);
                    SoundPlayed = true;
                }

                break; // use the first one we come across in the list - items at the top are most important
            }

            return highlighted;
        }

        /// <summary>
        /// Finds the first <see cref="HighlightItem"/> where the text matches the reg ex pattern
        /// </summary>
        /// <param name="coll">Collection of HighlightItems</param>
        /// <param name="text">The string of text to match</param>
        /// <returns></returns>
        private IEnumerable<HighlightItem> FindFirstHighlightByText(IEnumerable<HighlightItem> coll, string text)
        {
            foreach (HighlightItem item in coll)
            {
                // Empty pattern should not exist (a blank line should be a "special" Highlight item?
                if (!string.IsNullOrEmpty(item.Pattern) && !string.IsNullOrEmpty(text))
                {
                    if (text == item.Pattern || _patternMatching.MatchPattern(text, item.Pattern))
                    {
                        yield return item;
                    }
                }
            }
        }

        private bool ReadLines(bool overrideFileReadyCheck)
        {
            lock (this)
            {
                if (overrideFileReadyCheck || this.IsFileReadyForReread())
                {
                    using (StreamReader streamReader = this.OpenFile())
                    {
                        // If there is no stream, or it's empty reset the collection and return
                        if (streamReader == null || streamReader.BaseStream.Length == 0)
                        {
                            this.LastReadStream = new MemoryStream();
                            this.CollectionBackBuffer.Clear();
                            CopyBackBufferToFront();

                            return false;
                        }

                        // We have a viable stream so we now must parse through it
                        StreamReader oldReader = new StreamReader(this.LastReadStream);
                        oldReader.BaseStream.Position = 0;
                        int location = 0;
                        int positionInList = 0;
                        bool skipSameContent = false;
                        int lastUsedIndex = 0;                        
                        this.SoundPlayed = false;
                        this.PlaySound = true;

                        while (streamReader.Peek() > -1)
                        {                            
                            string oldLine = oldReader.ReadLine();
                            string line = streamReader.ReadLine();

                            if (line != null)
                            {
                                // There is no previous stream to compare against so just add the content to the screen
                                if (oldReader.BaseStream.Length <= 0)
                                {
                                    this.SoundPlayed = true;
                                    skipSameContent = false;
                                    positionInList = location;
                                }
                                else
                                {
                                    // We have more lines in the new stream than the old so add it to the end of the collection
                                    if (oldLine == null)
                                    {
                                        // This results in a blank line being added to the end of the line collection
                                        positionInList = -1;
                                    }

                                    // the oldline is not null and is either different from the new line or empty so add it to the collection
                                    // at the current location
                                    else if ((oldLine != null) && (!line.Equals(oldLine) || oldLine == string.Empty) )
                                    {
                                        positionInList = location;
                                    }

                                    // Otherwise the old and new lines are the same so do nothing to the screen content
                                    else
                                    {
                                        this.PlaySound = false;
                                        skipSameContent = true;
                                        lastUsedIndex = location;
                                        HighlightedItem item = GetHighlighting(line);
                                        this.CollectionBackBuffer[location].BackColour = item.BackColour;
                                        this.CollectionBackBuffer[location].ForeColour = item.ForeColour;
                                        this.PlaySound = true;
                                    }
                                }

                                // if the old and new lines are different
                                if (!skipSameContent)
                                {
                                    line = line.TrimEnd(Constants.NULL_TERMINATOR).Replace(Constants.MAC_NEWLINE, Constants.CARRIAGE_RETURN).Replace(Constants.UNIX_NEWLINE, Constants.LINE_FEED); // in case of incorrectly selected line end;
                                    lastUsedIndex = this.AddHightLightedItem(line, positionInList);

                                    if (this.SoundPlayed)
                                    {
                                        this.PlaySound = false;
                                    }
                                }

                                skipSameContent = false;
                            }

                            location++;

                            if (location % 1000 == 0)
                            {
                                ReportProgress((int)(location * 100 / streamReader.BaseStream.Length), string.Format(LanguageHelper.GetLocalisedText((Application.Current as IApplication), Constants.LINES_IN_FILE), location), false, System.Windows.Visibility.Visible);
                            }
                        }

                        // if the lastUsedIndex has not been set above then
                        // default it to the location
                        if (lastUsedIndex == 0)
                        {
                            lastUsedIndex = location;
                        }
                        
                        // Remove any lines that have been deleted from the end of the file
                        int max = this.CollectionBackBuffer.Count - 1;
                        
                        if (lastUsedIndex != max)
                        {
                            for (int j = max; j > lastUsedIndex; j--)
                            {
                                this.RemoveHightlightedItem(j);
                            }
                        }

                        // Create the previous stream to compare with next time from the current stream this time
                        streamReader.BaseStream.Position = 0;
                        this.LastReadStream = new MemoryStream();
                        streamReader.BaseStream.CopyTo(this.LastReadStream);
                    }

                    // Copy the collection of ListView items to the ListView control
                    // Doing it this way should result in quicker page refreshes as the screen will only be updated once
                    CopyBackBufferToFront();

                    return true;
                }
            }

            return false;
        }

        private void CopyBackBufferToFront()
        {
            this.Dispatcher.Invoke((Action)(() =>
            {
                this.colourfulListView.Items.Clear();
                foreach (var item in this.CollectionBackBuffer)
                {
                    this.colourfulListView.Items.Add(item);
                }
            }));
        }    

        public RationalFileWatcher()
        {
            InitializeComponent();

            // setup the line ending detection combo box
            this.comboBoxNewlineDetection.ItemsSource = Enum.GetNames(typeof(NewlineDetectionMode));
            Binding bindingNewlineDetection = new Binding("NewlineDetectionMode");
            bindingNewlineDetection.Source = this;
            this.comboBoxNewlineDetection.SetBinding(ComboBox.TextProperty, bindingNewlineDetection);

            // setup the encoding combo box
            this.comboBoxEncoding.ItemsSource = Encoding.GetEncodings();
            Binding bindingEncoding = new Binding("TailEncoding");
            bindingEncoding.Source = this;
            this.comboBoxEncoding.SetBinding(ComboBox.SelectedItemProperty, bindingEncoding);
            comboBoxEncoding.DisplayMemberPath = "DisplayName";
            comboBoxEncoding.SelectedValuePath = "DisplayName";

            // bind the follow tail checkbox
            Binding bindingFollowTail = new Binding("IsFollowTail");
            bindingFollowTail.Source = this;
            this.checkBoxFollowTail.SetBinding(CheckBox.IsCheckedProperty, bindingFollowTail);

            this._bw = new BackgroundWorker();
            this._bw.WorkerSupportsCancellation = true;
            this._bw.DoWork += new DoWorkEventHandler(_bw_DoWork);
            this._bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(_bw_RunWorkerCompleted);

            this.SelectedItem = new List<HighlightedItem>();
            this.LastReadStream = new MemoryStream();
            this.CollectionBackBuffer = new List<HighlightedItem>();
        }

        void _bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            return;
        }

        /// <summary>
        /// Start the watching process
        /// </summary>
        /// <param name="filename">The file to watch</param>
        public void Start(string filename)
        {
            this._filename = filename;
            if (!this._bw.IsBusy)
            {
                this._bw.RunWorkerAsync();
            }
        }        

        private void ShowStatus(int progressPercentage, FileWatcherProgressChangedUserState userState)
        {
            this.textBlockMainStatus.Text = userState.MainStatusText;
            this.progressBarStatus.Visibility = userState.ProgressBarVisibility;
            this.progressBarStatus.IsIndeterminate = userState.ProgressBarIndeterminate;
            this.progressBarStatus.Value = progressPercentage;
        }

        private void _bw_DoWork(object sender, DoWorkEventArgs e)
        {
            this._doWorkEventArgs = e;
            while (!this._bw.CancellationPending)
            {
                this.Tail();
                if (!this.CancellationPending())
                {
                    Thread.Sleep(this.Interval);
                }
            }
            e.Result = "Completed";
            return;
        }

        /// <summary>
        /// Reads only the latest additional text that has been appended to the file
        /// </summary>
        private void Tail()
        {
            if(this.ReadLines(false) && this.IsFollowTail)
            {
                this.ScrollToEnd();
            }
        }

        private void ScrollToEnd()
        {
            this.Dispatcher.Invoke(DispatcherPriority.Loaded, (Action)(() =>
            {
                lock (this)
                {
                    if (this.ScrollViewer != null)
                    {
                        this.ScrollViewer.ScrollToVerticalOffset(this.colourfulListView.Items.Count);
                    }
                }
            }));
        }        

        /// <summary>
        /// Provide CLR accessors for the event
        /// </summary>
        public event RoutedEventHandler FileChanged
        {
            add { AddHandler(FileChangedEvent, value); }
            remove { RemoveHandler(FileChangedEvent, value); }
        }

        private void OnFileChanged()
        {
            Dispatcher.Invoke((Action)(() =>
            {
                RoutedEventArgs newEventArgs = new RoutedEventArgs(RationalFileWatcher.FileChangedEvent);
                this.RaiseEvent(new RoutedEventArgs(RationalFileWatcher.FileChangedEvent, this));
            }
            ));
        }           

        private bool CancellationPending()
        {
            if (this._bw.CancellationPending)
            {
                this._doWorkEventArgs.Cancel = true;
            }
            return this._bw.CancellationPending;
        }

        private void ReportProgress(int progressBarPercentage, string status, bool progressBarIndeterminate, System.Windows.Visibility progressBarVisibility)
        {
            this.Dispatcher.Invoke(new StatusNotificationDelegate(this.ShowStatus), DispatcherPriority.Normal, progressBarPercentage, new FileWatcherProgressChangedUserState(status, progressBarIndeterminate, progressBarVisibility));
        }

        internal void Stop()
        {
            if (this._bw.IsBusy)
            {
                this._bw.CancelAsync();
            }
            // sleep until the worker has terminated
            while (this._bw.IsBusy)
            {
                ReportProgress(0, LanguageHelper.GetLocalisedText((Application.Current as IApplication), "waitingForWatcher"), true, System.Windows.Visibility.Visible);
                Thread.Sleep(100);
            }
            return;
        }

        public void Dispose()
        {
        }

        private void buttonRefresh_Click(object sender, RoutedEventArgs e)
        {
            this.ReadLines(true);
        }               

        private void colourfulListView_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            ScrollViewer scrollViewer = e.OriginalSource as ScrollViewer;
            ScrollBar scroll = scrollViewer.Template.FindName("PART_VerticalScrollBar", scrollViewer) as ScrollBar;
            CalculateFollowTailFromScrollbarPosition(scroll);
        }

        private void colourfulListView_Scroll(object sender, ScrollEventArgs e)
        {
            ScrollBar scroll = e.OriginalSource as ScrollBar;
            CalculateFollowTailFromScrollbarPosition(scroll);
        }

        private void CalculateFollowTailFromScrollbarPosition(ScrollBar scroll)
        {
            if (scroll != null && scroll.Orientation == Orientation.Vertical)
            {
                if (scroll.Value != scroll.Maximum)
                {
                    this.checkBoxFollowTail.IsChecked = false;
                }
                else
                {
                    this.checkBoxFollowTail.IsChecked = true;
                }
            }
        }

        private void colourfulListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.SelectedItem.Clear();

            foreach (var item in e.AddedItems)
            {
                this.SelectedItem.Add((HighlightedItem)item);
            }
        }

        void Patterns_ListChanged(object sender, ListChangedEventArgs e)
        {
        }

        private void UpdateHighlighting()
        {
        }        

        private void UpdateFindHighlighting(string text)
        {
            this.Dispatcher.Invoke((Action)(() =>
                        {
                            foreach (int i in this.PreviouslySelectedItems)
                            {
                                this.colourfulListView.SelectedIndex = -1;
                                HighlightedItem item = new HighlightedItem(((HighlightedItem)this.colourfulListView.Items[i]).Text, ((HighlightedItem)this.colourfulListView.Items[i]).ForeColour, ((HighlightedItem)this.colourfulListView.Items[i]).BackColour);
                                item.Selected = false;
                                this.colourfulListView.Items[i] = item;
                            }

                            for (int i = LastSearchIndex; i < this.colourfulListView.Items.Count; i++)
                            {
                                HighlightedItem item = new HighlightedItem(((HighlightedItem)this.colourfulListView.Items[i]).Text, ((HighlightedItem)this.colourfulListView.Items[i]).ForeColour, ((HighlightedItem)this.colourfulListView.Items[i]).BackColour);
                                if (this.GetFoundHighlightItem(ref item))
                                {
                                    {
                                        this.colourfulListView.Items[i] = item;
                                        this.colourfulListView.SelectedIndex = i;                                        
                                        this.colourfulListView.ScrollIntoView(item);
                                    }

                                    LastSearchIndex = i + 1;

                                    this.previouslySelectedItems = new List<int>();
                                    this.previouslySelectedItems.Add(i);

                                    return;
                                }
                                else
                                {
                                    //HACK: need to ensure that blank lines are not null!
                                    if (item != null)
                                    {
                                        item.Selected = false;
                                        this.colourfulListView.Items[i] = item;
                                        this.colourfulListView.SelectedIndex = -1;
                                    }
                                }
                            }

                            
                            // We've not found anything, since the last find
                            LastSearchIndex = 0;
                            ThrowFindFinished();

                        }));
        }

        void _findDetails_Initiated(object sender, FindEventArgs e)
        {
            if (this.SearchText != e.FindDetails.FindCriteria)
            {
                this.LastSearchIndex = 0;
            }

            this.SearchText = e.FindDetails.FindCriteria;
            this.UpdateFindHighlighting(e.FindDetails.FindCriteria);
        }

        private void ThrowFindFinished()
        {
            if (this.FindFinished != null)
            {
                this.FindFinished(this, new EventArgs());
            }
        }

        internal void ResetSearchCriteria()
        {
            this.SearchText = string.Empty;
        }

        #endregion methods
    }
}
