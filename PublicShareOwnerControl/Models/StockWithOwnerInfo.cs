using System;
using System.Collections.Generic;
using PublicShareOwnerControl.DB;

namespace PublicShareOwnerControl.Models
{
    public class StockWithOwnerInfo
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public double LastTradedValue { get; set; }
        public int TotalShares { get; set; }
        public static List<StockWithOwnerInfo> FromStockList(List<Stock> list)
        {
            var stockWithOwnerInfos = new List < StockWithOwnerInfo >();
            foreach (var stock in list)
            {
                var totalShares = 0;
                foreach (var stockShareHolder in stock.ShareHolders)
                {
                    totalShares += stockShareHolder.Amount;
                }

                stockWithOwnerInfos.Add(new StockWithOwnerInfo
                {
                    Id = stock.Id,
                    LastTradedValue = stock.LastTradedValue,
                    Name = stock.Name,
                    TotalShares = totalShares
                });
            }

            return stockWithOwnerInfos;
        }
    }
}