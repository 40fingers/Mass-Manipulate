using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FortyFingers.Library;

namespace FortyFingers.DnnMassManipulate.Components
{
    public class MassManipulateConfig : ModuleConfigBase
    {
        public MassManipulateConfig(Hashtable settings, int moduleId, int tabModuleId) : base(settings, moduleId, tabModuleId)
        {
        }

        protected override string ConfigDefaultsResourceFile()
        {
            return "";
        }
    }
}