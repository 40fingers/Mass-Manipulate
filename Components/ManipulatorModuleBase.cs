using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FortyFingers.DnnMassManipulate.Components
{
    public abstract class ManipulatorModuleBase
    {
        public ContextHelper Context { get; set; }
        public string JsName => this.GetType().Name;
        public string Assembly => this.GetType().AssemblyQualifiedName;
        public abstract string TabName();
        public abstract bool AllowAdministrator();

        public virtual bool AllowHost()
        {
            return AllowAdministrator();
        }
        public abstract string GetHtml();
    }
}