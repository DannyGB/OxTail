using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OxTailHelpers.Data;
using OxTailHelpers;
using OxTail.Data.SQLite;

namespace OxTailLogic.Data
{
    public class AppSettingsData : IAppSettingsData
    {
        private IAppSettingsData AppSettingsDataHelper;

        public AppSettingsData()
        {
            this.AppSettingsDataHelper = new AppSettingsDataHelper();
        }

        public AppSettings ReadAppSettings()
        {
            return this.AppSettingsDataHelper.ReadAppSettings();
        }

        public int WriteAppSettings(AppSettings settings)
        {
            return this.AppSettingsDataHelper.WriteAppSettings(settings);
        }
    }
}
