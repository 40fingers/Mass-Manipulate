using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Web;
using System.Web.UI;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Framework;
using DotNetNuke.Instrumentation;

namespace FortyFingers.DnnMassManipulate.Components._40FingersLib
{
    /// <summary>
    /// Utility class containing several commonly used procedures by 40FINGERS
    /// </summary>
    public static class Utils
    {
        /// <summary>
        /// Concatenates the values in a List Of Strings, sepratated by the supplied separator.
        /// </summary>
        /// <param name="list"></param>
        /// <param name="separator">Any string</param>
        /// <returns></returns>
        public static string ToString(this List<string> list, string separator)
        {
            var retval = "";
            if (list != null && list.Count > 0)
            {
                foreach (var value in list)
                {
                    if (retval.Length > 0) retval += separator;
                    retval += value;
                }
            }

            return retval;
        }

        /// <summary>
        /// Safely converts a string to Integer, returning Null.NullInteger in case of failure.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static int ToInt(this string text)
        {
            int i = Null.NullInteger;
            int.TryParse(text.Trim(), out i);
            return i;
        }

        /// <summary>
        /// Safely converts a string to Integer, returning defaultValue in case of failure.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static int ToInt(this string text, int defaultValue)
        {
            int i = text.ToInt();

            // set defaultValue if conversion failed.
            // note that text could have been Null.NullValue and thus valid
            if (i == Null.NullInteger &&
                text.ToString(CultureInfo.InvariantCulture) != Null.NullInteger.ToString(CultureInfo.InvariantCulture))
            {
                i = defaultValue;
            }

            return i;
        }

        public static ILog Logger
        {
            get
            {
                return LoggerSource.Instance.GetLogger(Constants.MODULE_LOGGER_NAME);
            }
        }

        public static void AddStyleSheetCached(Page Page, string ID, string href)
        {
            CDefault DefaultPage = (CDefault)Page;
            Hashtable objCSSCache = null;
            if (DotNetNuke.Common.Globals.PerformanceSetting != DotNetNuke.Common.Globals.PerformanceSettings.NoCaching)
            {
                objCSSCache = (Hashtable)DataCache.GetCache("40FINGERS_CSS");
            }
            if (objCSSCache == null)
            {
                objCSSCache = new Hashtable();
            }

            if (objCSSCache.ContainsKey(ID) == false)
            {
                if (File.Exists(HttpContext.Current.Server.MapPath(href)))
                {
                    objCSSCache[ID] = href;
                }
                else
                {
                    objCSSCache[ID] = "";
                }
                if (DotNetNuke.Common.Globals.PerformanceSetting != DotNetNuke.Common.Globals.PerformanceSettings.NoCaching)
                {
                    DataCache.SetCache("40FINGERS_CSS", objCSSCache);
                }
            }
            if (!string.IsNullOrEmpty(objCSSCache[ID].ToString()))
            {
                DefaultPage.AddStyleSheet(ID, objCSSCache[ID].ToString());
            }

        }

        public static List<T> ToList<T>(this ArrayList arrayList)
        {
            var retval = new List<T>();

            if (arrayList == null) return retval;

            try
            {
                foreach (var item in arrayList)
                {
                    retval.Add((T)item);
                }
            }
            catch (Exception exception)
            {
                Common.Logger.FatalFormat("Casting arraylist to List of type {0} failed: {1}. {2}", typeof(T).FullName, exception.Message, exception.StackTrace);
                throw;
            }

            return retval;
        }

    }
}