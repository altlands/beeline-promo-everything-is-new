using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace Promo.EverythingIsNew.WebApp.Models
{
    public class TariffsConfiguration : ConfigurationSection
    {
        public static TariffsConfiguration GetConfig()
        {
            return (TariffsConfiguration)ConfigurationManager.GetSection("tariffsConfiguration");  // ?? new ShiConfiguration();
        }

        [ConfigurationProperty("codes")]
        public TariffIndexesCollection Codes
        {
            get
            {
                return (TariffIndexesCollection)this["codes"] ?? new TariffIndexesCollection();
            }
        }
    }

    public class TariffIndexesCollection : ConfigurationElementCollection
    {
        List<TariffIndexElement> _elements = new List<TariffIndexElement>();

        public TariffIndexElement this[int index]
        {
            get
            {
                return base.BaseGet(index) as TariffIndexElement;
            }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            TariffIndexElement newElement = new TariffIndexElement();
            _elements.Add(newElement);
            return newElement;
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return _elements.Find(e => e.Equals(element));
        }

        public new IEnumerator<TariffIndexElement> GetEnumerator()
        {
            return _elements.GetEnumerator();
        }
    }

    public class TariffIndexElement : ConfigurationElement
    {
        [ConfigurationProperty("Soc", IsRequired = true)]
        public string Soc
        {
            get
            {
                return this["Soc"] as string;
            }
        }

        [ConfigurationProperty("MarketCode", IsRequired = true)]
        public string MarketCode
        {
            get
            {
                return this["MarketCode"] as string;
            }
        }

        [ConfigurationProperty("City", IsRequired = true)]
        public string City
        {
            get
            {
                return this["City"] as string;
            }
        }
    }






   
}