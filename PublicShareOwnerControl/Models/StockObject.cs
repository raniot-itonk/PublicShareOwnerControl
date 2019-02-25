using System.Collections.ObjectModel;
using PublicShareOwnerControl.DB;

namespace PublicShareOwnerControl.Models
{
    public class StockObject
    {
        public string Name { get; set; }
        public Collection<Shareholder> Shares { get; set; }
    }
}