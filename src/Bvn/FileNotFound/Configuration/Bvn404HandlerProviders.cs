using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Knowit.NotFound.Bvn.FileNotFound.Configuration
{
    [ConfigurationCollection(typeof(Bvn404HandlerProvider), CollectionType = ConfigurationElementCollectionType.BasicMapAlternate, AddItemName = "add")]
    public class Bvn404HandlerProviders : ConfigurationElementCollection
    {
        [GeneratedCode("ConfigurationSectionDesigner.CsdFileGenerator", "2.0.0.0")]
        internal const string Bvn404HandlerProviderPropertyName = "add";
        [GeneratedCode("ConfigurationSectionDesigner.CsdFileGenerator", "2.0.0.0")]
        public override ConfigurationElementCollectionType CollectionType
        {
            get
            {
                return ConfigurationElementCollectionType.BasicMapAlternate;
            }
        }
        [GeneratedCode("ConfigurationSectionDesigner.CsdFileGenerator", "2.0.0.0")]
        protected override string ElementName
        {
            get
            {
                return "add";
            }
        }
        [GeneratedCode("ConfigurationSectionDesigner.CsdFileGenerator", "2.0.0.0")]
        public Bvn404HandlerProvider this[int index]
        {
            get
            {
                return (Bvn404HandlerProvider)base.BaseGet(index);
            }
        }
        [GeneratedCode("ConfigurationSectionDesigner.CsdFileGenerator", "2.0.0.0")]
        public Bvn404HandlerProvider this[object name]
        {
            get
            {
                return (Bvn404HandlerProvider)base.BaseGet(name);
            }
        }
        [GeneratedCode("ConfigurationSectionDesigner.CsdFileGenerator", "2.0.0.0")]
        protected override bool IsElementName(string elementName)
        {
            return elementName == "add";
        }
        [GeneratedCode("ConfigurationSectionDesigner.CsdFileGenerator", "2.0.0.0")]
        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((Bvn404HandlerProvider)element).Name;
        }
        [GeneratedCode("ConfigurationSectionDesigner.CsdFileGenerator", "2.0.0.0")]
        protected override ConfigurationElement CreateNewElement()
        {
            return new Bvn404HandlerProvider();
        }
        [GeneratedCode("ConfigurationSectionDesigner.CsdFileGenerator", "2.0.0.0")]
        public void Add(Bvn404HandlerProvider add)
        {
            base.BaseAdd(add);
        }
        [GeneratedCode("ConfigurationSectionDesigner.CsdFileGenerator", "2.0.0.0")]
        public void Remove(Bvn404HandlerProvider add)
        {
            base.BaseRemove(this.GetElementKey(add));
        }
       
        public Bvn404HandlerProvider GetItemAt(int index)
        {
            return (Bvn404HandlerProvider)base.BaseGet(index);
        }
      
        public Bvn404HandlerProvider GetItemByKey(string name)
        {
            return (Bvn404HandlerProvider)base.BaseGet(name);
        }

        public override bool IsReadOnly()
        {
            return false;
        }
    }
}
