using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DotNetNuke.Entities.Tabs;
using FortyFingers.DnnMassManipulate.Components;

namespace FortyFingers.DnnMassManipulate.ManipulatorModules.Sample
{
    public class SampleModel
    {
        public SampleModel()
        {
        }
        public ContextHelper Context { get; set; }
    }

    public class SamplePostModel
    {
        public string SampleInput { get; set; }
    }
}