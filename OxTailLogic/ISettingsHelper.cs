﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OxTailHelpers;

namespace OxTailLogic
{
    public interface ISettingsHelper
    {
        IAppSettings AppSettings
        {
            get;
        }

        void WriteSettings();
    }
}
