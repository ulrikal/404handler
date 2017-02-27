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
    public class Bvn404HandlerConfiguration : ConfigurationSection
    {
      
        internal const string Bvn404HandlerConfigurationSectionName = "bvn404Handler"; 
        internal const string XmlnsPropertyName = "xmlns";
        internal const string RedirectsXmlFilePropertyName = "redirectsXmlFile";
        internal const string HandlerModePropertyName = "handlerMode";
        internal const string FileNotFoundPagePropertyName = "fileNotFoundPage";
      
        internal const string ThresholdPropertyName = "threshold";
        
        internal const string BufferSizePropertyName = "bufferSize";
     
        internal const string LoggingPropertyName = "logging";
    
        internal const string Bvn404HandlerProvidersPropertyName = "providers";
      
        public static Bvn404HandlerConfiguration Instance
        {
            get
            {
                return (Bvn404HandlerConfiguration)ConfigurationManager.GetSection("bvn404Handler");
            }
        }
       
        public string Xmlns
        {
            get
            {
                return (string)base["xmlns"];
            }
        }
        
        public string RedirectsXmlFile
        {
            get
            {
                return (string)base["redirectsXmlFile"];
            }
            set
            {
                base["redirectsXmlFile"] = value;
            }
        }
        
        public string HandlerMode
        {
            get
            {
                return (string)base["handlerMode"];
            }
            set
            {
                base["handlerMode"] = value;
            }
        }
        
        public string FileNotFoundPage
        {
            get
            {
                return (string)base["fileNotFoundPage"];
            }
            set
            {
                base["fileNotFoundPage"] = value;
            }
        }
        
        public int Threshold
        {
            get
            {
                return (int)base["threshold"];
            }
            set
            {
                base["threshold"] = value;
            }
        }
  
        public int BufferSize
        {
            get
            {
                return (int)base["bufferSize"];
            }
            set
            {
                base["bufferSize"] = value;
            }
        }
        
        public string Logging
        {
            get
            {
                return (string)base["logging"];
            }
            set
            {
                base["logging"] = value;
            }
        }
       
        public Bvn404HandlerProviders Bvn404HandlerProviders
        {
            get
            {
                return (Bvn404HandlerProviders)base["providers"];
            }
            set
            {
                base["providers"] = value;
            }
        }
       
        public override bool IsReadOnly()
        {
            return false;
        }
    }
}
