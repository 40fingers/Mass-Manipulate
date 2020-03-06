using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DotNetNuke.Entities.Tabs;
using FortyFingers.DnnMassManipulate.Components;

namespace FortyFingers.DnnMassManipulate.ManipulatorModules.GenerateRoles
{
    public class GenerateRolesModel
    {
        public GenerateRolesModel()
        {
        }
        public ContextHelper Context { get; set; }
    }

    public class GenerateRolesPostModel
    {
        public string RolesTemplate { get; set; }
    }
}