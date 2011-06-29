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
using System.Windows.Forms;
using System.Collections.Specialized;

namespace OxTailHelpers
{
    public class AppSettings : Dictionary<string, string>, IAppSettings
    {
        public const string REFRESH_INTERVAL_KEY = "RefreshInterval";
        public const string MAX_OPEN_FILES = "MaxOpenFiles";
        public const string MAX_MRU_FILES = "MaMruFiles";
        public const string REOPEN_FILES = "ReopenFiles";
        public const string PLAY_SOUND = "PlaySound";
        public const string PLAY_SOUND_FILE = "PlaySoundFile";
        public const string MINIMISE_TO_TRAY = "MinimiseToTray";
        public const string PAUSE_ON_FOUND = "PauseOnFound";

        public EventHandler<EventArgs> AppSettingsChanged;

        public AppSettings()
        {
        }

        /// <summary>
        /// Initialize the collection with default values if non are stored in the db
        /// </summary>
        public void Initialize()
        {
            base.Add(AppSettings.REFRESH_INTERVAL_KEY, Constants.REFRESH_INTERVAL);
            base.Add(AppSettings.MAX_OPEN_FILES, Constants.MAX_OPEN_FILES);
            base.Add(AppSettings.MAX_MRU_FILES, Constants.MAX_MRU_LIST);
            base.Add(AppSettings.REOPEN_FILES, Constants.REOPEN_FILES);
            base.Add(AppSettings.PLAY_SOUND, false.ToString());
            base.Add(AppSettings.PLAY_SOUND_FILE, string.Empty);
            base.Add(AppSettings.MINIMISE_TO_TRAY, false.ToString());
            base.Add(AppSettings.PAUSE_ON_FOUND, false.ToString());
        }        
    }
}
