using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace FortyFingers.DnnMassManipulate.Components
{
    public class Range
    {
        public int FromValue { get; set; } = -1;
        public int ToValue { get; set; } = -1;


        /// <summary>
        ///         ''' Number of Characters
        ///         ''' </summary>
        ///         ''' <value></value>
        ///         ''' <returns></returns>
        ///         ''' <remarks></remarks>
        public int Characters
        {
            get
            {
                return ToValue.ToString().Length;
            }
        }


        public string RestString { get; set; } = string.Empty;


        public Range(string sIn, string sReplace)
        {

            // Get the range string
            Match rxmRange = Regex.Match(sIn, @"\[([\d-]*)\]", RegexOptions.IgnoreCase);
            if (rxmRange != null)
            {
                string strRange = rxmRange.Groups[1].Value;
                if (!string.IsNullOrEmpty(strRange))
                {
                    RestString = Regex.Replace(sIn, Regex.Escape(rxmRange.Value), sReplace);
                    string[] Values = Regex.Split(strRange, "-");

                    if (Values.Length > 0 && Values[0].Length > 0)
                    {
                        FromValue = System.Convert.ToInt32(Values[0]);
                        if (Values.Length > 1 && Values[1].Length > 0)
                            ToValue = System.Convert.ToInt32(Values[1]);
                        else
                            ToValue = FromValue;
                    }
                }
            }
        }

        /// <summary>
        ///         ''' Returns a String.format string to format a number with leading zero's
        ///         ''' </summary>
        ///         ''' <param name="MaxValue">Max value in th erange, pass 0 to get the maxvalue from this object</param>
        ///         ''' <param name="Minimum">Minimum number of characters</param>
        ///         ''' <returns></returns>
        ///         ''' <remarks></remarks>

        public string GetRangeFormat(int MaxValue, int Minimum)
        {
            if (MaxValue == 0)
                MaxValue = this.ToValue;

            int iChars = MaxValue.ToString().Length;
            if (Minimum > iChars)
                iChars = Minimum;


            string strTrailing = new string('0', iChars);

            string strFormat = string.Format("{{0:{0}}}", strTrailing);

            return strFormat;
        }
    }

}