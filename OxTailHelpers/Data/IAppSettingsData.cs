using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OxTailHelpers;

namespace OxTailHelpers.Data
{
    public interface IAppSettingsData : IData
    {
        AppSettings ReadAppSettings();
        int WriteAppSettings(AppSettings settings);
    }
}
