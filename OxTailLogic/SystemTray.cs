using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using OxTailHelpers;
using System.Reflection;

namespace OxTailLogic
{
    public class SystemTray : ISystemTray
    {
        private readonly Icon icon;
        private readonly ContextMenu contextMenu;

        public SystemTray(IApplication application)
        {
            this.icon = new Icon(ResourceHelper.GetStreamFromApplication("OxTailLogic.Images.OxTail.ico", Assembly.GetExecutingAssembly()));
            this.contextMenu = new ContextMenu();

            System.Windows.Forms.MenuItem menuItem = new System.Windows.Forms.MenuItem(LanguageHelper.GetLocalisedText(application, Constants.DISABLE_SOUND_CONTEXT_MENU));
            menuItem.Checked = bool.Parse(SettingsHelper.AppSettings[AppSettings.PLAY_SOUND]);
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new System.Windows.Forms.MenuItem(LanguageHelper.GetLocalisedText(application, Constants.MINIMISE_TO_TRAY));
            menuItem.Checked = bool.Parse(SettingsHelper.AppSettings[AppSettings.MINIMISE_TO_TRAY]);
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new System.Windows.Forms.MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new System.Windows.Forms.MenuItem(LanguageHelper.GetLocalisedText(application, Constants.EXIT_CONTEXT_MENU));
            contextMenu.MenuItems.Add(menuItem);
        }

        public Icon Icon
        {
            get
            {
                return icon;
            }
        }

        public ContextMenu ContextMenu
        {
            get
            {
                return contextMenu;
            }
        }
    }
}
