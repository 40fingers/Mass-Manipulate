using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FortyFingers.DnnMassManipulate.Components;

namespace FortyFingers.DnnMassManipulate.ManipulatorModules.RemoveSkins
{
    public class RemoveSkinsModel
    {
        public ContextHelper Context { get; set; }
        public List<SkinModel> Skins { get; set; }
    }

    public class SkinModel
    {
        public string Name { get; set; }
        public string SkinPath { get; set; }
        public string PortalHomeFolder { get; set; }
        public bool PortalSkin { get; set; }
        public bool SkinUsed { get; set; }
        public bool HasContainers { get; set; }
        public bool ContainersUsed { get; set; }
    }
}