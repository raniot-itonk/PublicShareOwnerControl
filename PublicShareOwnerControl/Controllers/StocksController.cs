using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PublicShareOwnerControl.DB;
using PublicShareOwnerControl.Models;

namespace PublicShareOwnerControl.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public partial class StocksController : ControllerBase
    {
        private readonly PublicShareOwnerContext _context;
        private readonly ILogger<StocksController> _logger;

        public StocksController(PublicShareOwnerContext context, ILogger<StocksController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Get single Stock information
        //[Authorize("BankingService.UserActions")]
        [HttpGet("{id}")]
        public async Task<ActionResult<Stock>> GetStock(long id)
        {
            
            var stock = await _context.Stocks.Include(s => s.ShareHolders).FirstOrDefaultAsync(s2 => s2.Id == id);

            if (stock == null)
            {
                return NotFound();
            }
            _logger.LogInformation("Got {@Stock}", stock);
            return stock;
        }

        // Get all Stock, get all stock where OwnerId equal ownerId or
        // get all stock where owner is equal to userIdGuid
        //[Authorize("BankingService.UserActions")]
        [HttpGet]
        public async Task<ActionResult> GetStocks([FromQuery] string userIdGuid = null, string ownerId = null)
        {
            //TODO validate that ShareholderId is same as in Header

            List<Stock> stocks;
            if (userIdGuid == null && ownerId == null)
            {
                stocks = await _context.Stocks.ToListAsync();
                _logger.LogInformation("Got list of all stocks");
            }
            else if (ownerId != null)
            {
                var stockOwnerId = Guid.Parse(ownerId);
                stocks = await _context.Stocks
                    .Include(stock => stock.ShareHolders)
                    .Where(s => s.StockOwner == stockOwnerId).ToListAsync();
                _logger.LogInformation("Got stock with StockOwner {ownerId}", ownerId);

                return Ok(StockWithOwnerInfo.FromStockList(stocks));
            }
            else
            {
                stocks = await _context.Stocks
                    .Include(stock => stock.ShareHolders)
                    .Where(s => s.ShareHolders.Any(q => q.ShareholderId == Guid.Parse(userIdGuid))).ToListAsync();
                _logger.LogInformation("Got list of all stocks with owner {UserId}", userIdGuid);
            }
            return Ok(stocks);
        }

        // Add Stock
        //[Authorize("BankingService.UserActions")]
        [HttpPost]
        public async Task<ActionResult<Stock>> PostStock(StockObject stockObject)
        {
            var stock = new Stock
            {
                LastTradedValue = 0,
                Name = stockObject.Name,
                ShareHolders = stockObject.Shares,
                StockOwner = stockObject.StockOwner
            };
            await _context.Stocks.AddAsync(stock);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Added Stock {@Stock}", stock);
            return stock;
        }

        // Issue more Shares to existing stock
        //[Authorize("BankingService.UserActions")]
        [HttpPut("{id}/issue")]
        public async Task<ActionResult> IssueShares([FromRoute] long id,[FromBody] IssueObject issueObject)
        {
            var stock = await _context.Stocks.Where(x => x.Id == id)
                .Include(s => s.ShareHolders)
                .Where(s => s.ShareHolders.Any(q => q.ShareholderId == issueObject.Owner)).FirstOrDefaultAsync();
            stock = await AddShareholder(id, issueObject, stock);

            await _context.SaveChangesAsync();

            return Ok();
        }

        private async Task<Stock> AddShareholder(long id, IssueObject issueObject, Stock stock)
        {
            if (stock == null)
            {
                stock = await _context.Stocks.Include(x => x.ShareHolders).Where(x => x.Id == id).FirstOrDefaultAsync();
                stock.ShareHolders.Add(new Shareholder { ShareholderId = issueObject.Owner, Amount = issueObject.Amount });
            }
            else
            {
                var shareHolder = stock.ShareHolders.FirstOrDefault(sh => sh.ShareholderId == issueObject.Owner);
                if (shareHolder != null) shareHolder.Amount += issueObject.Amount;
            }

            return stock;
        }

        // Change ownership of existing shares
        //[Authorize("BankingService.UserActions")]
        [HttpPut("{id}/ownership")]
        public async Task<ActionResult> ChangeOwnership([FromRoute] long id, [FromBody] OwnershipObject ownershipObject)
        {
            var stock = await _context.Stocks.Where(x => x.Id == id)
                .Include(s => s.ShareHolders)
                .Where(s => s.ShareHolders.Any(q => q.ShareholderId == ownershipObject.Seller)).FirstOrDefaultAsync();
            if (stock == null)
            {
                _logger.LogError("Failed to find the Seller");
                return NotFound("Failed to find the Seller");
            }

            var actionResult = SetSellerAmount(ownershipObject, stock);
            if (actionResult != null) return actionResult;

            SetBuyerAmount(ownershipObject, stock);

            _logger.LogInformation("Changed ownership of {Amount} shares from {Seller} to {Buyer}", ownershipObject.Amount, ownershipObject.Seller, ownershipObject.Buyer);

            await _context.SaveChangesAsync();

            return Ok();
        }

        // Change ownership of existing shares
        //[Authorize("BankingService.UserActions")]
        [HttpPut("{id}/LastTradedValue")]
        public async Task<ActionResult> UpdateLastTradedValue([FromRoute] long id, [FromBody] LastTradedValueResponse response)
        {
            if (id != response.Id) return BadRequest("Ids are not equal");
            var stock = await _context.Stocks.Where(x => x.Id == id).FirstOrDefaultAsync();
            if (stock == null)
            {
                _logger.LogError("Failed to find the Stock");
                return NotFound("Failed to find the Stock");
            }

            var oldLastTradedValue = stock.LastTradedValue;
            stock.LastTradedValue = response.Value;

            _logger.LogInformation("Updated the last traded value of stock {StockName} from {oldValue} to {NewValue}", stock.Name, oldLastTradedValue, stock.LastTradedValue);

            await _context.SaveChangesAsync();

            return Ok();
        }

        private static void SetBuyerAmount(OwnershipObject ownershipObject, Stock stock)
        {
            var shareHolderBuyer = stock.ShareHolders.FirstOrDefault(sh => sh.ShareholderId == ownershipObject.Buyer);
            if (shareHolderBuyer == null)
            {
                shareHolderBuyer = new Shareholder { ShareholderId = ownershipObject.Buyer, Amount = ownershipObject.Amount };
                stock.ShareHolders.Add(shareHolderBuyer);
            }
            else
            {
                shareHolderBuyer.Amount += ownershipObject.Amount;
            }
        }

        private ActionResult SetSellerAmount(OwnershipObject ownershipObject, Stock stock)
        {
            var shareholderSeller = stock.ShareHolders.FirstOrDefault(sh => sh.ShareholderId == ownershipObject.Seller);
            if (shareholderSeller == null)
            {
                _logger.LogError("Failed to find the Seller");
                {
                    return NotFound("Failed to find the Seller");
                }
            }

            _logger.LogInformation(@"shareholder Seller Amount {shareholderSellerAmount} request amount {requestAmount}", shareholderSeller.Amount, ownershipObject.Amount);
            shareholderSeller.Amount -= ownershipObject.Amount;
            if (shareholderSeller.Amount >= 0) return null;
            _logger.LogError("Seller cannot go below 0 shares");
            return BadRequest("Seller cannot go below 0 shares");

        }
    }
}
