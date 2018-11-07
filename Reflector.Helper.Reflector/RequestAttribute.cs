using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reflector.Helper.Reflector
{
    public class RequestAttribute : Attribute
    {

        public enum ValueProvider
        {
            None,
            Category, 
            OriginSupplier,
            Status,
            Company,
            DeliveryCalendar,
            OrderType
        }

        public string name { get; set; }
        public string alias { get; set; }
        public string indexProp { get; set; }
        public ValueProvider valueProvider { get; set; }

        public RequestAttribute(string pName)
        {
            name = pName;
        }

        public RequestAttribute(string pName, string pAlias)
        {
            name = pName;
            alias = pAlias;
        }

        public RequestAttribute(string pName, string pAlias, string pIndexProp)
        {
            name = pName;
            alias = pAlias;
            indexProp = pIndexProp;
        }
        
        public RequestAttribute(string pName, string pAlias, ValueProvider pValueProvider)
        {
            name = pName;
            alias = pAlias;
            valueProvider = pValueProvider;
        }
        

        public string GetValue(object value)
        {
            if (this.valueProvider != ValueProvider.None && !string.IsNullOrEmpty(value.ToString()))
            {
                return Helper.Reflector.Translator.Providers[this.valueProvider].GetValue(value.ToString());
            }
            else
            {
                return value.ToString();
            }
        }

    }

}
