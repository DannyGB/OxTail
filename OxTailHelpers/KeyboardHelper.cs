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
using System.Windows.Input;
using System.Collections;

namespace OxTailHelpers
{
    public static class KeyboardHelper
    {
        public delegate void Action();
        /// <summary>
        /// Returns whether the left control key is pressed down on the keyboard
        /// </summary>
        /// <returns>A <see cref="bool"/></returns>
        public static bool IsLeftControlDown()
        {
            return Keyboard.IsKeyDown(Key.LeftCtrl);
        }

        /// <summary>
        /// Calls the appropriate action for the key that has been pressed
        /// </summary>
        /// <param name="key">The <see cref="Key"/> that has been pressed</param>
        /// <param name="delegates">The <see cref="IDictionary"/> of methods to call identifed by the key</param>
        public static void GetPressedKey(Key key, IDictionary<Key, Action> delegates)
        {
            CallAction(key, delegates);
        }

        public static void StandardControlKeyCombinationPressed(Key key, IMainWindowKeyPressMethods mainWindow)
        {
            if (KeyboardHelper.IsLeftControlDown())
            {
                IDictionary<Key, KeyboardHelper.Action> dict = new Dictionary<Key, KeyboardHelper.Action>(4);
                dict.Add(Key.C, mainWindow.CopyText);
                dict.Add(Key.F, mainWindow.OpenFindScreen);
                dict.Add(Key.O, mainWindow.OpenFile);
                dict.Add(Key.I, mainWindow.OpenHightlightScreen);

                KeyboardHelper.GetPressedKey(key, dict);
            }
        }

        private static void CallAction(Key key, IDictionary<Key, Action> delegates)
        {
            if (delegates.ContainsKey(key) && delegates[key] != null)
            {
                delegates[key]();
            }
        }
    }
}
