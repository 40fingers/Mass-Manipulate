using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DotNetNuke.Entities.Tabs;

namespace FortyFingers.DnnMassManipulate.ManipulatorModules.Settings
{
    public class SettingsPostModel
    {
        public List<ModuleEnabledModel> Modules { get; set; }
    }

    public class ModuleEnabledModel
    {
        public string Module { get; set; }
        public bool Enabled { get; set; }
    }
}