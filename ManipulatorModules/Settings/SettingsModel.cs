using FortyFingers.DnnMassManipulate.Components;
using System.Collections.Generic;

namespace FortyFingers.DnnMassManipulate.ManipulatorModules.Settings
{
    public class SettingsModel
    {
        public SettingsModel()
        {
        }
        public ContextHelper Context { get; set; }
        public List<KeyValuePair<string, (string, bool)>> Modules { get; set; } = new List<KeyValuePair<string, (string, bool)>>();
    }
}