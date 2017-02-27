using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Knowit.NotFound.Bvn.FileNotFound.Configuration
{
    public class Bvn404HandlerProvider : ConfigurationElement
    {
        [GeneratedCode("ConfigurationSectionDesigner.CsdFileGenerator", "2.0.0.0")]
        internal const string NamePropertyName = "name";
        [GeneratedCode("ConfigurationSectionDesigner.CsdFileGenerator", "2.0.0.0")]
        internal const string TypePropertyName = "type";
        [GeneratedCode("ConfigurationSectionDesigner.CsdFileGenerator", "2.0.0.0"), Description("The Name."), ConfigurationProperty("name", IsRequired = true, IsKey = true, IsDefaultCollection = false)]
        public string Name
        {
            get
            {
                return (string)base["name"];
            }
            set
            {
                base["name"] = value;
            }
        }
        [GeneratedCode("ConfigurationSectionDesigner.CsdFileGenerator", "2.0.0.0"), Description("The Type."), ConfigurationProperty("type", IsRequired = false, IsKey = false, IsDefaultCollection = false)]
        public string Type
        {
            get
            {
                return (string)base["type"];
            }
            set
            {
                base["type"] = value;
            }
        }
        [GeneratedCode("ConfigurationSectionDesigner.CsdFileGenerator", "2.0.0.0")]
        public override bool IsReadOnly()
        {
            return false;
        }
    }
}
