using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DotNetNuke.Entities.Tabs;
using FortyFingers.DnnMassManipulate.Components;

namespace FortyFingers.DnnMassManipulate.ManipulatorModules.CopyPagePermissions
{
    public class CopyPagePermissionsModel
    {
        public CopyPagePermissionsModel()
        {
        }
        public ContextHelper Context { get; set; }
    }

    public class CopyPagePermissionsPostModel
    {
        public string FromRole { get; set; }
        public string ToRole { get; set; }
        public bool DoCopy { get; set; }
    }
}