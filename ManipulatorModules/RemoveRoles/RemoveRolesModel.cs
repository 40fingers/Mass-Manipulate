using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DotNetNuke.Entities.Tabs;
using FortyFingers.DnnMassManipulate.Components;

namespace FortyFingers.DnnMassManipulate.ManipulatorModules.RemoveRoles
{
    public class RemoveRolesModel
    {
        public RemoveRolesModel()
        {
        }
        public ContextHelper Context { get; set; }
    }

    public class RemoveRolesPostModel
    {
        public string RolesRegex { get; set; }
        public bool DoRemove { get; set; }
    }
}