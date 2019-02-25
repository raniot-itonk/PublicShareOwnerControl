using System.Collections.ObjectModel;

namespace PublicShareOwnerControl.DB
{
    public class Stock
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public double LastTradedValue { get; set; }
        public Collection<Shareholder> ShareHolders { get; set; }
    }
}