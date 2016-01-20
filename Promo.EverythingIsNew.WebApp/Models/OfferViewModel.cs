using System.Collections.Generic;

namespace Promo.EverythingIsNew.WebApp.Models
{
    public class OfferViewModel
    {
        public string UserName { get; set; }
        public string TariffName { get; set; }
        public List<TariffGroupViewModel> Groups { get; set; }

        public OfferViewModel()
        {
            Groups = new List<TariffGroupViewModel>();
        }
    }

    public class TariffGroupViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<TariffLineViewModel> Lines { get; set; }
        public int? SortOrder { get; set; }

        public TariffGroupViewModel()
        {
            Lines = new List<TariffLineViewModel>();
        }
    }

    public class TariffLineViewModel
    {
        public string Title { get; set; }
        public string NumValue { get; set; }
        public string UnitDisplay { get; set; }
        public string Value { get; set; }
        public int? SortOrder { get; set; }
    }


}