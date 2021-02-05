using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DotNetNuke.Entities.Tabs;
using FortyFingers.DnnMassManipulate.Components;

namespace FortyFingers.DnnMassManipulate.ManipulatorModules.DbReplace
{
    public class DbReplaceModel
    {
        public DbReplaceModel()
        {
        }
        public ContextHelper Context { get; set; }
        public List<KeyValuePair<string, string>> ConnectionStrings { get; set; } = new List<KeyValuePair<string, string>>();
    }

    public class DbReplacePostModel
    {
        public string ConnectionString { get; set; }
        public string SqlSelect { get; set; }
        public string SqlUpdate { get; set; }
        public string AllPortals { get; set; }
        public string CaseSensitive { get; set; }
        public List<SearchReplacePairModel> SearchReplace { get; set; } = new List<SearchReplacePairModel>();
    }

    public class SearchReplacePairModel
    {
        public string Search { get; set; }
        public string Replace { get; set; }
    }
}