using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DotNetNuke.Entities.Tabs;
using FortyFingers.DnnMassManipulate.Components;

namespace FortyFingers.DnnMassManipulate.ManipulatorModules.Users
{
    public class UsersModel
    {
        public UsersModel()
        {
        }
        public ContextHelper Context { get; set; }
    }

    public class UsersPostModel
    {
        public string TemplateType { get; set; }
        public string UsersInput { get; set; }
        public bool UsersFolders { get; set; }
        public bool HardDelete { get; set; }
    }
}