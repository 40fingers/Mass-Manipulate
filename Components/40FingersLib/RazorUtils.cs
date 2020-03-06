using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Hosting;
using DotNetNuke.UI.Modules;
using DotNetNuke.Web.Razor;

namespace FortyFingers.FortyFingersLib
{
    public static class RazorUtils
    {
        internal static bool Render<T>(T model, string scriptfile, string scriptsPath, ModuleInstanceContext moduleContext, out string result, out string message)
        {
            bool retval = false;
            try
            {
                var razorScriptFile = Path.Combine(scriptsPath, scriptfile);

                if (File.Exists(razorScriptFile) || File.Exists(HostingEnvironment.MapPath(razorScriptFile)))
                {
                    //Common.Logger.DebugFormat("Rendering Scriptfile [{0}]", HostingEnvironment.MapPath(razorScriptFile));
                    var engine = new RazorEngine(razorScriptFile, moduleContext, null);
                    var writer = new StringWriter();
                    engine.Render<T>(writer, model);

                    result = writer.ToString();
                    message = "";
                    retval = true;
                }
                else
                {
                    //Common.Logger.ErrorFormat("Script not found: {0}", razorScriptFile);
                    message = "Script not found";
                    result = "";
                }
            }
            catch (Exception e)
            {
                message = String.Format("Rendering razor file {0} failed for object of type {1}", scriptfile, typeof(T).Name);
                //Common.Logger.Error(message, e);
                result = "";
            }
            return retval;
        }
        internal static bool Render(string scriptfile, string scriptsPath, ModuleInstanceContext moduleContext, out string result, out string message)
        {
            bool retval = false;
            try
            {
                var razorScriptFile = Path.Combine(scriptsPath, scriptfile);

                if (File.Exists(razorScriptFile) || File.Exists(HostingEnvironment.MapPath(razorScriptFile)))
                {
                    //Common.Logger.DebugFormat("Rendering Scriptfile [{0}]", HostingEnvironment.MapPath(razorScriptFile));
                    var engine = new RazorEngine(razorScriptFile, moduleContext, null);
                    var writer = new StringWriter();
                    engine.Render(writer);

                    result = writer.ToString();
                    message = "";
                    retval = true;
                }
                else
                {
                    //Common.Logger.ErrorFormat("Script not found: {0}", razorScriptFile);
                    message = "Script not found";
                    result = "";
                }
            }
            catch (Exception e)
            {
                message = String.Format("Rendering razor file {0} failed", scriptfile);
                //Common.Logger.Error(message, e);
                result = "";
            }
            return retval;
        }
    }
}
