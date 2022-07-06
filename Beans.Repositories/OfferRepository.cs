using Beans.Common;
using Beans.Common.Enumerations;
using Beans.Repositories.Entities;
using Beans.Repositories.Interfaces;
using Beans.Repositories.Models;

using Dapper;
using Dapper.Contrib.Extensions;

using System.Data.Common;
using System.Data.SqlClient;
using System.Text;

namespace Beans.Repositories;
public class OfferRepository : RepositoryBase<OfferEntity>, IOfferRepository
{
    private readonly IBeanRepository _beanRepository;
    private readonly IUserRepository _userRepository;
    private readonly IHoldingRepository _holdingRepository;
    private readonly INoticeRepository _noticeRepository;

    public OfferRepository(IDatabase database, IBeanRepository beanRepository, IUserRepository userRepository, IHoldingRepository holdingRepository,
      INoticeRepository noticeRepository) : base(database)
    {
        _beanRepository = beanRepository;
        _userRepository = userRepository;
        _holdingRepository = holdingRepository;
        _noticeRepository = noticeRepository;
    }

    public override async Task<IEnumerable<OfferEntity>> GetAsync(string sql, params QueryParameter[] parameters)
    {
        using var conn = new SqlConnection(ConnectionString);
        try
        {
            var offers = await conn.QueryAsync<OfferEntity>(sql, BuildParameters(parameters));
            if (offers is not null && offers.Any())
            {
                foreach (var offer in offers)
                {
                    offer.Bean = await _beanRepository.ReadAsync(offer.BeanId);
                    offer.User = await _userRepository.ReadAsync(offer.UserId);
                }
            }
            return offers!;
        }
        finally
        {
            await conn.CloseAsync();
        }
    }

    public override async Task<OfferEntity?> ReadAsync(string sql, params QueryParameter[] parameters)
    {
        using var conn = new SqlConnection(ConnectionString);
        try
        {
            var ret = await conn.QueryFirstOrDefaultAsync<OfferEntity>(sql, BuildParameters(parameters));
            if (ret is not null)
            {
                ret.Bean = await _beanRepository.ReadAsync(ret.BeanId);
                ret.User = await _userRepository.ReadAsync(ret.UserId);
            }
            return ret;
        }
        finally
        {
            await conn.CloseAsync();
        }
    }

    public async Task<IEnumerable<OfferEntity>> GetForUserAsync(int userid) => await GetAsync($"select * from Offers where UserId={userid};");

    public async Task<IEnumerable<OfferEntity>> GetOtherOffersAsync(int userid) => await GetAsync($"select * from Offers where UserId != {userid};");

    public async Task<IEnumerable<OfferEntity>> GetForBeanAsync(int beanid) => await GetAsync($"select * from Offers where BeanId={beanid};");

    public async Task<IEnumerable<OfferEntity>> GetForHoldingAsync(int holdingid) => await GetAsync($"select * from Offers where HoldingId={holdingid};");

    public async Task<IEnumerable<OfferEntity>> GetOffersAsync(int sellerid, int beanid, bool includeExpired)
    {
        var sb = new StringBuilder("select * from offers");
        var needsAnd = false;
        if (sellerid > 0)
        {
            needsAnd = true;
            sb.Append($" where SellerId={sellerid}");
        }
        if (beanid > 0)
        {
            if (needsAnd)
            {
                sb.Append(" and");
            }
            else
            {
                sb.Append(" where");
            }
            needsAnd = true;
            sb.Append($" BeanId={beanid}");
        }
        if (!includeExpired)
        {
            if (needsAnd)
            {
                sb.Append(" and");
            }
            else
            {
                sb.Append(" where");
            }
            sb.Append($" ExpirationDate > '{DateTime.UtcNow:yyyy-MM-dd}'");
        }
        return await GetAsync(sb.ToString());
    }

    private async Task<int> GetCountAsync(string sql)
    {
        using var conn = new SqlConnection(ConnectionString);
        try
        {
            await conn.OpenAsync();
            return await conn.QueryFirstOrDefaultAsync<int>(sql);
        }
        finally
        {
            await conn.CloseAsync();
        }
    }

    public async Task<bool> UserHasOffersAsync(int userid) => await GetCountAsync($"select count(*) from Offers where UserId={userid};") > 0;

    public async Task<bool> BeanHasOffersAsync(int beanid) => await GetCountAsync($"select count(*) from Offers where BeanId={beanid};") > 0;

    public async Task<bool> HoldingHasOffersAsync(int holdingid) => await GetCountAsync($"select count(*) from Offers where HoldingId={holdingid};") > 0;

    public async Task<DalResult> BuyFromOfferAsync(int buyerid, long quantity, int offerid, bool oldestFirst)
    {
        if (buyerid <= 0)
        {
            return new(DalErrorCode.Invalid, new("Buyer id is invalid"));
        }
        if (quantity == 0)
        {
            return new(DalErrorCode.Invalid, new("Quantity must be greater than zero"));
        }
        if (offerid <= 0)
        {
            return new(DalErrorCode.Invalid, new Exception("Offer id is invalid"));
        }
        var offer = await ReadAsync(offerid);
        if (offer is null)
        {
            return new(DalErrorCode.Invalid, new Exception("Offer not found"));
        }
        if (offer.UserId <= 0)
        {
            return new(DalErrorCode.Invalid, new Exception("Seller id in offer is invalid"));
        }
        var bean = await _beanRepository.ReadAsync(offer.BeanId);
        if (bean is null)
        {
            return new(DalErrorCode.NotFound, new Exception($"No bean with the id {offer.BeanId} was found"));
        }
        var buyer = await _userRepository.ReadAsync(buyerid);
        if (buyer is null)
        {
            return new(DalErrorCode.NotFound, new($"No user with the id {buyerid} was found"));
        }
        if (buyer.Balance < quantity * offer.Price)
        {
            return new(DalErrorCode.NSF, new("Buyer does not have sufficient funds"));
        }
        var seller = await _userRepository.ReadAsync(offer.UserId);
        if (seller is null)
        {
            return new(DalErrorCode.NotFound, new($"No user with the id {offer.User} was found"));
        }
        var beansHeld = await _holdingRepository.BeansHeldByUserAndBeanAsync(offer.UserId, offer.BeanId);
        if (quantity > beansHeld)
        {
            return new(DalErrorCode.NSF, new("Seller has insufficient beans"));
        }
        var holdings = await _holdingRepository.GetHoldingsAsync(oldestFirst, offer.UserId, offer.BeanId, quantity);
        if (holdings.Length == 0)
        {
            return new(DalErrorCode.NotFound, new("Unable to locate holdings to satisfy sale"));
        }
        using var conn = new SqlConnection(ConnectionString);
        await conn.OpenAsync();
        using var transaction = await conn.BeginTransactionAsync();
        try
        {
            var profit = 0M;
            long satisfied = 0;
            var ix = 0;
            while (ix < holdings.Length - 1)
            {
                var holding = await _holdingRepository.ReadAsync(holdings[ix]);
                if (holding is null)
                {
                    await transaction.RollbackAsync();
                    return new(DalErrorCode.NotFound, new($"No holding with the id {holdings[ix]} was found"));
                }
                var sale = new SaleEntity
                {
                    Id = 0,
                    UserId = seller.Id,
                    BeanId = bean.Id,
                    PurchaseDate = holding.PurchaseDate,
                    SaleDate = DateTime.UtcNow,
                    CostBasis = holding.Price,
                    Quantity = holding.Quantity,
                    SalePrice = offer.Price,
                    Bean = null
                };
                var insresult = await conn.InsertAsync(sale, transaction: transaction);
                profit += holding.Quantity * offer.Price;
                satisfied += holding.Quantity;
                await conn.DeleteAsync(holding, transaction: transaction);
                ix++;
            }
            var lastHolding = await _holdingRepository.ReadAsync(holdings[^1]);
            if (lastHolding is null)
            {
                await transaction.RollbackAsync();
                return new(DalErrorCode.NotFound, new($"No holding with the id {holdings[^1]} was found"));
            }
            var remaining = quantity - satisfied;
            var lastSale = new SaleEntity
            {
                Id = 0,
                UserId = seller.Id,
                BeanId = bean.Id,
                PurchaseDate = lastHolding.PurchaseDate,
                SaleDate = DateTime.UtcNow,
                CostBasis = lastHolding.Price,
                Quantity = remaining,
                SalePrice = offer.Price,
                Bean = null
            };
            await conn.InsertAsync(lastSale, transaction: transaction);
            profit += remaining * offer.Price;
            lastHolding.Quantity -= remaining;
            if (lastHolding.Quantity == 0)
            {
                await conn.DeleteAsync(lastHolding, transaction: transaction);
            }
            else
            {
                await conn.UpdateAsync(lastHolding, transaction: transaction);
            }
            seller.Balance += profit;
            buyer.Balance -= profit;
            await conn.UpdateAsync(seller, transaction: transaction);
            await conn.UpdateAsync(buyer, transaction: transaction);
            var newHolding = new HoldingEntity
            {
                Id = 0,
                UserId = buyer.Id,
                BeanId = bean.Id,
                PurchaseDate = DateTime.UtcNow,
                Quantity = quantity,
                Price = offer.Price,
                Bean = null
            };
            await conn.InsertAsync(newHolding, transaction: transaction);
            if (offer.Quantity == quantity)
            {
                await conn.DeleteAsync(offer, transaction: transaction);
            }
            else
            {
                offer.Quantity -= quantity;
                await conn.UpdateAsync(offer, transaction: transaction);
            }
            await transaction.CommitAsync();
            await _noticeRepository.SendNoticeAsync(buyerid, -1, "Bean Purchase Successful",
              $"Your purchase of {quantity} {bean.Name} bean(s) from {seller} for {offer.Price.ToCurrency(2)} is complete");
            await _noticeRepository.SendNoticeAsync(seller.Id, -1, "Bean(s) Sold",
              $"You sold {quantity} {bean.Name} bean(s) to {buyer} for {offer.Price.ToCurrency(2)} each");
            return DalResult.Success;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return DalResult.FromException(ex);
        }
        finally
        {
            await conn.CloseAsync();
        }
    }

    private static async Task MakeSale(SqlConnection conn, int seller, int beanid, DateTime purchasedate, long quantity, decimal costbasis, decimal saleprice,
        DbTransaction transaction)
    {
        var sale = new SaleEntity
        {
            Id = 0,
            UserId = seller,
            BeanId = beanid,
            PurchaseDate = purchasedate,
            SaleDate = DateTime.UtcNow,
            Quantity = quantity,
            CostBasis = costbasis,
            SalePrice = saleprice,
            Bean = null
        };
        await conn.InsertAsync(sale, transaction: transaction);
    }

    public async Task<DalResult> SellToOfferAsync(int offerid, int sellerid, (int holdingid, long quantity)[] holdings)
    {
        // validation
        var offer = await ReadAsync(offerid);
        if (offer is null)
        {
            return new(DalErrorCode.NotFound, new Exception("Offer not found"));
        }
        var seller = await _userRepository.ReadAsync(sellerid);
        if (seller is null)
        {
            return new(DalErrorCode.NotFound, new Exception("Seller not found"));
        }
        var buyer = await _userRepository.ReadAsync(offer.UserId);
        if (buyer is null)
        {
            return new(DalErrorCode.NotFound, new Exception("Buyer not found"));
        }
        var sellingquantity = holdings.Sum(x => x.quantity);
        if (sellingquantity * offer.Price > buyer.Balance)
        {
            return new(DalErrorCode.NSF, new Exception("Buyer has insufficient funds"));
        }
        if (holdings is null || !holdings.Any())
        {
            return new(DalErrorCode.Invalid, new Exception("Holdings collection is required"));
        }
        Dictionary<int, HoldingEntity> hdict = new();
        foreach (var (holdingid, quantity) in holdings)
        {
            if (quantity <= 0)
            {
                return new(DalErrorCode.Invalid, new Exception("Invalid quantity in holding"));
            }
            var entity = await _holdingRepository.ReadAsync(holdingid);
            if (entity is null)
            {
                return new(DalErrorCode.NotFound, new Exception("Holding not found"));
            }
            if (entity.UserId != sellerid)
            {
                return new(DalErrorCode.Invalid, new Exception("Holding does not belong to seller"));
            }
            hdict.Add(holdingid, entity);
        }
        // begin updating
        using var conn = new SqlConnection(ConnectionString);
        await conn.OpenAsync();
        using var transaction = await conn.BeginTransactionAsync();
        var fulfilled = 0L;
        try
        {
            foreach (var kvp in hdict)
            {
                // process holdings in order until we've fulfilled the request, or we run out of holdings
                var q = holdings.Single(x => x.holdingid == kvp.Key).quantity;
                var h = kvp.Value;
                if (fulfilled + q >= offer.Quantity)
                {
                    kvp.Value.Quantity -= offer.Quantity - fulfilled;
                    fulfilled = offer.Quantity;
                    seller.Balance += q * offer.Price;
                    buyer.Balance -= q * offer.Price;
                    await MakeSale(conn, seller.Id, h.BeanId, h.PurchaseDate, q, h.Price, offer.Price, transaction);
                    break; // we're done
                }
                else
                {
                    fulfilled += q;
                    kvp.Value.Quantity = 0;
                    seller.Balance += q * offer.Price;
                    buyer.Balance -= q * offer.Price;
                    await MakeSale(conn, seller.Id, h.BeanId, h.PurchaseDate, q, h.Price, offer.Price, transaction);
                }
            }
            // all should be prepped now, first update offer or delete if fulfilled
            if (fulfilled == offer.Quantity)
            {
                await conn.DeleteAsync(offer, transaction: transaction);
            }
            else
            {
                offer.Quantity -= fulfilled;
                await conn.UpdateAsync(offer, transaction: transaction);
            }
            // now update or delete each holding
            foreach (var kvp in hdict)
            {
                var holding = kvp.Value;
                if (holding.Quantity == 0)
                {
                    await conn.DeleteAsync(holding, transaction: transaction);
                }
                else
                {
                    await conn.UpdateAsync(holding, transaction: transaction);
                }
            }
            // commit
            await transaction.CommitAsync();
            // create notices
            await _noticeRepository.SendNoticeAsync(offer.UserId, -1, "Bean Purchase Successful",
              $"Your purchase of {fulfilled} {offer.Bean!.Name} bean(s) from {seller.DisplayName} for {offer.Price.ToCurrency(2)} is complete");
            await _noticeRepository.SendNoticeAsync(sellerid, -1, "Bean(s) Sold",
              $"You sold {fulfilled} {offer.Bean!.Name} bean(s) to {buyer.DisplayName} for {offer.Price.ToCurrency(2)} each");
            return DalResult.Success;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return DalResult.FromException(ex);
        }
        finally
        {
            await conn.CloseAsync();
        }
    }
}
