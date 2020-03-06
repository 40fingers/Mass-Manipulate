using System;
using System.Text.RegularExpressions;
using System.Web;
using DotNetNuke.Instrumentation;
using DotNetNuke.Services.Localization;

namespace FortyFingers.DnnMassManipulate.Components
{
    public static class Common
    {

        public static ILog Logger
        {
            get
            {
                return LoggerSource.Instance.GetLogger(Constants.MODULE_LOGGER_NAME);
            }
        }
        [Obsolete("Use Common.GetString instead")]
        public static string GetFromSharedResx(string key)
        {
            return GetString(key, Constants.SharedResourceFile);
        }

        /// <summary>
        /// Tries to get the request resource from the resourcefile. If it fails, from the sharedresourcefile.
        /// Note that DNN itself also uses fallback to globalresources if the first one fails.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="resourcefile"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static string GetString(string key, string resourcefile, params object[] args)
        {
            var retval = "";

            retval = Localization.GetString(key, resourcefile);
            if (string.IsNullOrEmpty(retval))
            {
                retval = Localization.GetString(key, Constants.SharedResourceFile);
            }

            if (args != null && args.Length > 0)
            {
                retval = String.Format(retval, args);
            }

            return retval;
        }

        public static string ToCleanJSON(this string s)
        {
            //if (s == null || s.Length == 0)
            //{
            //    return "";
            //}

            //char c = '\0';
            //int i;
            //int len = s.Length;
            //StringBuilder sb = new StringBuilder(len + 4);
            //String t;

            //for (i = 0; i < len; i += 1)
            //{
            //    c = s[i];
            //    switch (c)
            //    {
            //        case '\\':
            //        case '"':
            //            sb.Append('\\');
            //            sb.Append(c);
            //            break;
            //        case '/':
            //            sb.Append('\\');
            //            sb.Append(c);
            //            break;
            //        case '\b':
            //            sb.Append("\\b");
            //            break;
            //        case '\t':
            //            sb.Append("\\t");
            //            break;
            //        case '\n':
            //            sb.Append("\\n");
            //            break;
            //        case '\f':
            //            sb.Append("\\f");
            //            break;
            //        case '\r':
            //            sb.Append("\\r");
            //            break;
            //        default:
            //            if (c < ' ')
            //            {
            //                t = "000" + String.Format("X", c);
            //                sb.Append("\\u" + t.Substring(t.Length - 4));
            //            }
            //            else
            //            {
            //                sb.Append(c);
            //            }
            //            break;
            //    }
            //}

            //var retval = sb.ToString();

            // we also need to escape encoded quotes
            // json serialization will decode them and give invalid json

            //var retval = s.Replace("&quot;", "\"");
            var retval = HttpUtility.HtmlDecode(s);
            return retval;
        }


        /// <summary>
        /// Gets the path from a full path
        /// </summary>
        /// <param name="fullFilePath"></param>
        /// <returns></returns>
        public static string StripFileName(string fullFilePath)
        {
            var i = fullFilePath.LastIndexOfAny(@"\/".ToCharArray());
            if (i >= 0)
                return fullFilePath.Substring(0, i + 1);
            else
                return fullFilePath;
        }

        /// <summary>
        /// Gets the filename from a full path
        /// </summary>
        /// <param name="fullFilePath"></param>
        /// <returns></returns>
        public static string StripPath(string fullFilePath)
        {
            var i = fullFilePath.LastIndexOfAny(@"\/".ToCharArray());
            if (i >= 0)
                return fullFilePath.Substring(i + 1);
            else
                return fullFilePath;
        }

        /// <summary>
        /// Cleans a filename from forbidden characters on Windows Filesystems
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static string CleanFilename(string filename)
        {
            //stuk vanaf de laatste forward of backslash is de bestandsnaam
            string retval = "";
            var i = filename.LastIndexOfAny(@"\/".ToCharArray());
            if (i >= 0)
                retval = filename.Substring(i + 1);
            else
                retval = filename;

            // forbidden characters are: \/:*?"<>|
            var regex = new Regex("[:\\\\/\\*\\?\"<>\\|]", RegexOptions.CultureInvariant | RegexOptions.Compiled);
            // Replace the matched text in the InputText using the replacement pattern
            retval = regex.Replace(retval, "-");

            if (retval.Length > 200)
                retval = retval.Substring(0, 200);

            return retval;
        }


    }
}