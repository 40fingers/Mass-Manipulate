using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DotNetNuke.Entities.Tabs;
using FortyFingers.DnnMassManipulate.Components;

namespace FortyFingers.DnnMassManipulate.ManipulatorModules.GenerateTabs
{
    public class GenerateTabsModel
    {
        public GenerateTabsModel()
        {
            Tabs = new List<TabInfo>();
        }
        public ContextHelper Context { get; set; }
        public List<TabInfo> Tabs { get; set; }
    }

    public class GenerateTabsPostModel
    {
        public string IndentCharacter { get; set; }
        public int ParentTabId { get; set; }
        public string PageTree { get; set; }
    }
    public class GenerateOneTabPostModel
    {
        public string IndentCharacter { get; set; }
        public int ParentTabId { get; set; }
        public int PreviousTabId { get; set; }
        public string PreviousTabName { get; set; }
        public string TabName { get; set; }
    }
}