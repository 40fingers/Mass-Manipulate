using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using DotNetNuke.Entities.Host;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using FortyFingers.DnnMassManipulate.Components;
using FortyFingers.DnnMassManipulate.Components._40FingersLib;

namespace FortyFingers.DnnMassManipulate.ManipulatorModules.RemoveSkins
{
    public class RemoveSkinsModule : ManipulatorModuleBase
    {
        private readonly string ScriptsPath = $"~/DesktopModules/40Fingers/DnnMassManipulate/ManipulatorModules/RemoveSkins/";

        public override string TabName()
        {
            return "Remove Skins";
        }

        public override bool AllowAdministrator()
        {
            return false;
        }

        public override string GetHtml()
        {
            string msg;
            string retval;
            var model = new RemoveSkinsModel();
            model.Context = Context;
            model.Skins = Skins();

            if (RazorUtils.Render(model, "RemoveSkins.cshtml", ScriptsPath, null, out retval, out msg))
            {
                return retval;
            }
            else if (UserController.Instance.GetCurrentUserInfo().IsSuperUser)
            {
                return msg;
            }
            else
            {
                return "Something went wrong";
            }
        }

        private List<SkinModel> Skins()
        {
            var retval = new List<SkinModel>();
            var ctx = HttpContext.Current;
            // find skins on filesystem
            var portalsPath = ctx.Server.MapPath("~/Portals");
            foreach (var skinsFolder in Directory.GetDirectories(portalsPath, "*skins", SearchOption.AllDirectories))
            {
                var containersPath = Regex.Replace(skinsFolder, "skins", "Containers", RegexOptions.IgnoreCase);
                var portalFolder = skinsFolder.Replace(portalsPath + "\\", "");
                portalFolder = portalFolder.Substring(0, portalFolder.IndexOf("\\"));

                foreach (var skinFolder in Directory.GetDirectories(skinsFolder))
                {
                    var skinName = skinFolder.Substring(skinFolder.LastIndexOf("\\") + 1);

                    retval.Add(new SkinModel()
                    {
                        Name = skinName,
                        SkinPath = skinFolder,
                        PortalHomeFolder = portalFolder,
                        PortalSkin = portalFolder.ToLower() == "_default",
                        HasContainers = Directory.Exists($"{containersPath}\\{skinName}")
                    });
                }
            }

            return retval;
        }

    }
}