using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Http;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Web.Api;
using FortyFingers.DnnMassManipulate.ManipulatorModules.GenerateTabs;
using FortyFingers.DnnMassManipulate.ManipulatorModules.DbReplace;
using Newtonsoft.Json;

// Leave the ApiController in this namespace to avoid the need for a custom routemapper
namespace FortyFingers.DnnMassManipulate.Services
{
    [DnnModuleAuthorize]
    [SupportedModules("40Fingers.DnnMassManipulate")] // can be comma separated list of supported module
    public class DbReplaceController : DnnApiController
    {
        #region API Methods

        [HttpPost]
        public HttpResponseMessage DoReplace(DbReplacePostModel model)
        {
            var retval = SqlModulesReplace(model, ModuleReplaceMode.Replace);
            return Request.CreateResponse(HttpStatusCode.OK, retval);
        }
        [HttpPost]
        public HttpResponseMessage DoTestReplace(DbReplacePostModel model)
        {
            var retval = SqlModulesReplace(model, ModuleReplaceMode.ReplaceTest);
            return Request.CreateResponse(HttpStatusCode.OK, retval);
        }
        [HttpPost]
        public HttpResponseMessage DoListModules(DbReplacePostModel model)
        {
            var retval = SqlModulesReplace(model, ModuleReplaceMode.Find);
            return Request.CreateResponse(HttpStatusCode.OK, retval);
        }
        [HttpPost]
        public HttpResponseMessage DoGetScript(DbReplacePostModel model)
        {
            var retval = SqlModulesReplace(model, ModuleReplaceMode.GetScript);
            return Request.CreateResponse(HttpStatusCode.OK, retval);
        }

        #endregion


        private Hashtable GetSqlModules(DbReplacePostModel model)
        {
            Hashtable htModules = new Hashtable();

            var connstr = GetConnectionString(model);

            SqlConnection con = new SqlConnection(connstr);
            SqlCommand cmd = new SqlCommand();
            cmd.CommandText = model.SqlSelect;
            cmd.Connection = con;

            try
            {
                con.Open();
                var reader = cmd.ExecuteReader(); // (CommandBehavior.SingleRow)

                while (reader.Read())
                {
                    int portalid = CheckDbValueInteger(reader, "PortalId", PortalSettings.PortalId);

                    if (model.AllPortals == bool.TrueString || portalid == PortalSettings.PortalId)
                    {
                        // Check if the item was already added
                        string id = reader["ContentId"].ToString();
                        if (!htModules.ContainsKey(id))
                        {
                            var oMod = new ModuleContent
                            {
                                PortalId = CheckDbValueInteger(reader, "PortalId", PortalSettings.PortalId),
                                TabId = CheckDbValueInteger(reader, "TabId", -1),
                                ModuleId = CheckDbValueInteger(reader, "ModuleId", -1),
                                ContentSubId = CheckDbValueInteger(reader, "ContentSubId", -1),
                                ContentId = (int)reader["ContentId"],
                                Text = reader["ContentText"].ToString(),
                                Title = reader["ContentTitle"].ToString()
                            };

                            htModules.Add(id, oMod);
                        }
                    }
                }

                reader.Close();
            }
            finally
            {
                con.Close();
            }


            return htModules;
        }

        private static string GetConnectionString(DbReplacePostModel model)
        {
            var connstr = "";
            foreach (ConnectionStringSettings cs in ConfigurationManager.ConnectionStrings)
            {
                if (cs.Name == model.ConnectionString)
                {
                    connstr = cs.ConnectionString;
                    break;
                }
            }

            return connstr;
        }


        private string SqlModulesReplace(DbReplacePostModel model, ModuleReplaceMode mode)
        {
            // Get all modules
            Hashtable htModules = GetSqlModules(model);
            // Output
            StringBuilder sOut = new StringBuilder();

            var rxList = new List<KeyValuePair<string, string>>();
            foreach (var searchReplacePairModel in model.SearchReplace)
            {
                if (!string.IsNullOrEmpty(searchReplacePairModel.Search))
                    rxList.Add(new KeyValuePair<string, string>(searchReplacePairModel.Search, searchReplacePairModel.Replace));
            }

            int iModules = 0;
            int iReplaced = 0;

            foreach (DictionaryEntry Item in htModules)
            {

                // Count number of found modules
                iModules += 1;
                var oMod = (ModuleContent)Item.Value;
                string sText = oMod.Text;

                RegexOptions rxOptions = RegexOptions.None;
                if (model.CaseSensitive != bool.TrueString)
                    rxOptions = RegexOptions.IgnoreCase;

                // Store the Original text
                string sOldText = oMod.Text;

                // Do the replacement according to the regex replacements
                string sNewText = RegexReplace(sText, mode, rxList, rxOptions);

                


                if (sNewText != sText)
                {
                    string sPortal = GetPortalString(model.AllPortals == bool.TrueString, oMod.PortalId);

                    // These are for reporting, not for parsing / Sql
                    string sNewTextEncoded = HttpUtility.HtmlEncode(sNewText);
                    string sOldTextEncoded = HttpUtility.HtmlEncode(sOldText);


                    // Depending on the mode, handle text in a different way
                    switch (mode)
                    {
                        case ModuleReplaceMode.Find:
                            {
                                if (sNewTextEncoded != string.Empty)
                                {
                                    sNewTextEncoded = sNewTextEncoded.Replace("[*]", "<br />");

                                    sOut.AppendLine(string.Format("<h4>Module: <a href=\"{3}/Default.aspx?Tabid={0}#{1}\" target=\"_blank\">{2}</a></h4><pre>{4}</pre>", oMod.TabId, oMod.ModuleId, oMod.Title, sPortal, sNewTextEncoded));

                                    iReplaced += 1;
                                }

                                break;
                            }

                        case ModuleReplaceMode.ReplaceTest:
                            {
                                sOut.AppendLine(string.Format("<div><h4>Module: <a href=\"{3}/Default.aspx?Tabid={0}#{1}\" target=\"_blank\">{2}</a></h4><pre class=\"text-old\">{4}</pre><pre class=\"text-new\">{5}</pre></div><hr />", oMod.TabId, oMod.ModuleId, oMod.Title, sPortal, sOldTextEncoded, sNewTextEncoded));

                                iReplaced += 1;
                                break;
                            }

                        case ModuleReplaceMode.Replace:
                            {
                                var sUpdateSql = model.SqlUpdate;

                                if (model.SqlUpdate.Trim().ToLower().StartsWith("select"))
                                    sOut.AppendLine("Invalid Update SQL <br />");
                                else
                                {
                                    // Get the update statement
                                    sUpdateSql = GetUpdateSql(sUpdateSql, sNewText, oMod.ContentId);

                                    // Update
                                    if (ModuleSqlReplaceSave(GetConnectionString(model), sUpdateSql))
                                    {
                                        sOut.AppendLine(string.Format("<div><a href=\"/Default.aspx?Tabid={0}#{1}\" target=\"_blank\">{2}</a></div>", oMod.TabId, oMod.ModuleId, oMod.Title));

                                        iReplaced += 1;
                                    }
                                    else
                                        sOut.AppendLine("Errors SQL update <br />");
                                }
                                break;
                            }
                        case ModuleReplaceMode.GetScript:
                            {
                                var sUpdateSql = model.SqlUpdate;

                                if (model.SqlUpdate.ToLower().Contains("update"))
                                {

                                    // Get the update statement
                                    sUpdateSql = GetUpdateSql(sUpdateSql, sNewText, oMod.ContentId);

                                    // To show copyable SQL on page
                                    string sPrint = sUpdateSql.Replace("&#39;", "&#39;&#39;");

                                    sOut.AppendLine(string.Format("<pre>{0}</pre><hr />", sPrint));

                                    iReplaced += 1;
                                }
                                else
                                    sOut.AppendLine("Invalid Update SQL <br />");
                                break;
                            }
                    }
                }
            }

            var retval = "";
            // Output
            switch (mode)
            {
                case ModuleReplaceMode.Find:
                    {
                        retval = string.Format("<div class=\"Report\"><h2>Checked Items: {0} - found {1}</h2>{2}</div>", iModules, iReplaced, sOut.ToString());
                        break;
                    }

                case ModuleReplaceMode.ReplaceTest:
                    {
                        retval = string.Format("<div class=\"Report\"><h2>Checked Items: {0} - replaced {1}</h2>{2}</div>", iModules, iReplaced, sOut.ToString());
                        break;
                    }

                case ModuleReplaceMode.GetScript:
                    {
                        retval = string.Format("<div class=\"Report\"><h2>Generated SQL script for {0}/{1} Items</h2>{2}</div>", iReplaced, iModules, sOut.ToString());
                        break;
                    }

                case ModuleReplaceMode.Replace:
                    {
                        retval = string.Format("<div class=\"Report\"><h2>Checked Items: {0} - replaced {1}</h2>{2}</div>", iModules, iReplaced, sOut.ToString());
                        break;
                    }
            }

            return retval;
        }
        private bool ModuleSqlReplaceSave(string connectionString, string UpdateSql)
        {
            try
            {
                SqlConnection con = new SqlConnection(connectionString);
                SqlCommand cmd = new SqlCommand();

                cmd.CommandText = UpdateSql;
                cmd.Connection = con;

                cmd.Connection.Open();
                cmd.ExecuteNonQuery();
                cmd.Connection.Close();

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private string GetUpdateSql(string Input, string NewText, int id)
        {
            string sOut = Input;

            NewText = Regex.Replace(NewText, "'", "''");

            sOut = ReplaceToken(sOut, "newtext", NewText);
            sOut = ReplaceToken(sOut, "contentid", id.ToString());


            return (sOut);
        }
        private string ReplaceToken(string Input, string Token, string Replacement)
        {
            var sRegex = Regex.Escape(string.Format("[{0}]", Token));

            return Regex.Replace(Input, sRegex, Replacement, RegexOptions.IgnoreCase);
        }

        private string GetPortalString(bool allPortals, int PortalId)
        {
            if (allPortals)
            {
                if (PortalId != PortalSettings.PortalId)
                    return string.Format("//{0}", GetPortalAlias(PortalId));
                else
                    return string.Format("//{0}", PortalSettings.PortalAlias.HTTPAlias);
            }
            else
                return "";
        }

        private string GetPortalAlias(int PortalId)
        {
            PortalAliasController paController = new PortalAliasController();
            PortalAliasCollection paCollection = paController.GetPortalAliasByPortalID(PortalId);



            foreach (DictionaryEntry pa in paCollection)
            {
                PortalAliasInfo paI = (PortalAliasInfo)pa.Value;

                return (paI.HTTPAlias);
            }

            return "PortalAlias not found";
        }

        private string RegexReplace(string sIn, ModuleReplaceMode Mode, List<KeyValuePair<string, string>> RegexList, RegexOptions CompareCase = RegexOptions.IgnoreCase)
        {
            // Loop over the regex replacement and return the result.
            string sProcess; // Store string to process

            sProcess = sIn;

            foreach (KeyValuePair<string, string> Pair in RegexList)
            {
                switch (Mode)
                {
                    case ModuleReplaceMode.ReplaceTest:
                    case ModuleReplaceMode.GetScript:
                    case ModuleReplaceMode.Replace:
                        {
                            sProcess = Regex.Replace(sProcess, Pair.Key, Pair.Value, CompareCase | RegexOptions.CultureInvariant | RegexOptions.Singleline);
                            break;
                        }

                    case ModuleReplaceMode.Find:
                        {
                            var sOut = string.Empty;

                            // Loop over Matches
                            foreach (Match m in Regex.Matches(sProcess, Pair.Key, CompareCase | RegexOptions.CultureInvariant | RegexOptions.Singleline))

                                sOut += string.Format("{0}[*]", m.Value);


                            sProcess = sOut;
                            break;
                        }
                }
            }

            return sProcess;
        }

        private int CheckDbValueInteger(SqlDataReader reader, string colName, int defaultValue)
        {
            string selector = string.Format("ColumnName='{0}'", colName);

            if (reader.GetSchemaTable().Select(selector).Length > 0)
                return (int)reader[colName];
            else
                return defaultValue;
        }
        private string CheckDbValueString(SqlDataReader reader, string colName, string defaultValue)
        {
            string selector = string.Format("ColumnName='{0}'", colName);

            if (reader.GetSchemaTable().Select(selector).Length > 0)
                return reader[colName].ToString();
            else
                return defaultValue;
        }

        enum ModuleReplaceMode
        {
            Find = 0,
            ReplaceTest = 1,
            GetScript = 2,
            Replace = 3
        }

        private class ModuleContent
        {
            // PortalId of the module
            public int PortalId { get; set; }
            // Tabid where the mopdule was found
            public int TabId { get; set; }
            // Module id
            public int ModuleId { get; set; }
            // Detail id if the module contains articles
            public int ContentSubId { get; set; }
            // Unique ID for the content
            public int ContentId { get; set; }
            // Found text
            public string Text { get; set; }
            // Found text
            public string Title { get; set; }
            public void Update(int id, int Portalid, int TabId, int Moduleid, string Text)
            {
                this.PortalId = Portalid;
                this.TabId = TabId;
                this.ModuleId = Moduleid;
                this.ContentId = id;
                this.Text = Text;
            }
        }

    }
}