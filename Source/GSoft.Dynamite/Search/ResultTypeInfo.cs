using System.Collections.Generic;
using GSoft.Dynamite.Branding;
using Microsoft.Office.Server.Search.Administration;

namespace GSoft.Dynamite.Search
{
    public class ResultTypeInfo
    {
        public ResultTypeInfo(string name, DisplayTemplateInfo displaytemplate, ResultSourceInfo resultSource)
        {
            this.Name = name;
            this.DisplayProperties = new List<ManagedPropertyInfo>();
            this.Rules = new List<ResultTypeRuleInfo>();
            this.DisplayTemplate = displaytemplate;
            this.ResultSource = resultSource;
        }

        public string Name { get; private set; }

        public bool OptimizeForFrequenUse { get; set; }

        public int Priority { get; set; }

        public DisplayTemplateInfo DisplayTemplate { get; private set; }

        public ResultSourceInfo ResultSource { get; private set; }

        public IList<ResultTypeRuleInfo> Rules { get; set; }

        public IList<ManagedPropertyInfo> DisplayProperties { get; set; }
    }
}
