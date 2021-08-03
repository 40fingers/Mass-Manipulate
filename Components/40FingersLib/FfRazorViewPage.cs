using DotNetNuke.Web.Razor;

namespace FortyFingers.DnnMassManipulate.Components._40FingersLib
{
    /// <summary>
    /// Strong Typed WebViewPage inherited by razor scripts. Adds in several helpers: Ajax, Html, Url, Dnn, moduleContext
    /// </summary>
    public abstract class FfRazorViewPage : DotNetNukeWebPage
    {
        /// <summary>
        /// Provides access to current ModuleContext
        /// </summary>
        public ContextHelper ModuleContext { get; set; }

        ///// <summary>
        ///// Instanciates helpers
        ///// </summary>
        //public override void InitHelpers()
        //{
        //    Ajax = new AjaxHelper<TModel>(ViewContext, this);
        //    Html = new DnnHtmlHelper<TModel>(ViewContext, this);
        //    Url = new DnnUrlHelper(ViewContext);
        //    Dnn = new DnnHelper<TModel>(ViewContext, this);
        //    ModuleContext = new ContextHelper(ViewContext);
        //}

        /// <summary>
        /// Returns the current locale
        /// </summary>
        public string Locale
        {
            get
            {
                return System.Threading.Thread.CurrentThread.CurrentCulture.Name;
            }
        }

        /// <summary>
        /// Tries to get the request resource from the local resourcefile. If it fails, from the sharedresourcefile.
        /// Note that DNN itself also uses fallback to globalresources if the first one fails.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string GetString(string key, params object[] args)
        {
            var localResourceFile = Common.StripFileName(VirtualPath) + "App_LocalResources/Resources.resx";

            return Common.GetString(key, localResourceFile, args);
        }
    }
}