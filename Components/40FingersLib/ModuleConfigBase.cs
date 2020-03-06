using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Services.Localization;

namespace FortyFingers.Library
{
    public abstract class ModuleConfigBase
    {

        private Hashtable _settings = null;
        protected Hashtable Settings { get { return _settings; } }
        private int _moduleId = Null.NullInteger;
        protected int ModuleId { get { return _moduleId; } }
        private int _tabModuleId = Null.NullInteger;
        protected int TabModuleId { get { return _tabModuleId; } }

        protected ModuleConfigBase(Hashtable settings, int moduleId, int tabModuleId)
        {
            _settings = settings;
            _moduleId = moduleId;
            _tabModuleId = tabModuleId;
        }

        protected string GetDefault(string setting)
        {
            return Localization.GetString(setting, ConfigDefaultsResourceFile());
        }

        protected bool GetSettingBool(string setting, bool useDefault = true)
        {
            bool b = Null.NullBoolean;
            string settingValue = "";
            if (_settings.ContainsKey(setting))
            {
                settingValue = _settings[setting].ToString();
            }
            else if (useDefault)
            {
                settingValue = GetDefault(setting);
            }
            bool.TryParse(settingValue, out b);

            return b;
        }

        protected int GetSettingInt(string setting, bool useDefault = true)
        {
            int i = Null.NullInteger;
            string settingValue = "";
            if (_settings.ContainsKey(setting))
            {
                settingValue = _settings[setting].ToString();
            }
            else if (useDefault)
            {
                settingValue = GetDefault(setting);
            }
            int.TryParse(settingValue, out i);

            return i;
        }

        protected string GetSetting(string setting, bool useDefault = true)
        {
            string settingValue = "";
            if (_settings.ContainsKey(setting))
            {
                settingValue = _settings[setting].ToString();
            }
            else if (useDefault)
            {
                settingValue = GetDefault(setting);
            }

            return settingValue;
        }

        protected PortalSettings Ps
        {
            get { return PortalSettings.Current; }
        }

        private ModuleController _moduleCtrl;
        protected ModuleController ModuleCtrl
        {
            get
            {
                if (_moduleCtrl == null)
                {
                    _moduleCtrl = new ModuleController();
                }

                return _moduleCtrl;
            }
        }

        /// <summary>
        /// Return the resourcefiles containing the defaults for the settings.
        /// </summary>
        /// <returns>E.g. "~/DesktopModules/40Fingers/DnnMassManipulate/App_LocalResources/DnnMassManipulateConfigDefaults.resx"</returns>
        protected abstract string ConfigDefaultsResourceFile();

    }
}