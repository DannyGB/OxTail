﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Windows;

namespace OxTailHelpers
{
    public interface IApplication
    {
        Collection<ResourceDictionary> LanguageDictionary
        {
            get;
        }
    }
}
