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
using System.Reflection;
using System.IO;

namespace RandomDataGenerator
{
    internal class CreateRandomText
    {
        internal string GetFromResources(string resourceName)
        {
            Assembly assem = this.GetType().Assembly;
            using (Stream stream = assem.GetManifestResourceStream(resourceName))
            {
                using (var reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        internal void Create(string filename, bool emptyFile)
        {
            string text = this.GetFromResources("RandomDataGenerator.LoremIpsumText.txt");
            string jumble = this.Jumble(text);

            FileMode mode = FileMode.Append;
            if (emptyFile)
            {
                mode = FileMode.Open;
            }

            using (FileStream stream = File.Open(filename, mode, FileAccess.Write, FileShare.Write))
            using (TextWriter tw = new StreamWriter(stream))
            {
                tw.WriteLine(jumble);
            }
        }

        private string Jumble(string text)
        {
            string[] split = text.Split(' ');
            StringBuilder jumble = new StringBuilder();
            Random r = new Random();

            for(int i = 0; i <= split.Length; i++)
            {                
                int j = r.Next(0, split.Length - 1);

                if (i != 0 && i % 40 == 0)
                {
                    jumble.AppendLine(split[j]);
                }

                else
                {
                    jumble.Append(split[j]);
                    jumble.Append(" ");
                }
            }

            return jumble.ToString();
        }
    }
}
