using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using DotNetNuke.Entities.Modules;

namespace FortyFingers.DnnMassManipulate
{
    public partial class Settings : ModuleSettingsBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }
        public override void LoadSettings()
        {

        }
        public override void UpdateSettings()
        {
            var ctl = new ModuleController();
        }
    }
}