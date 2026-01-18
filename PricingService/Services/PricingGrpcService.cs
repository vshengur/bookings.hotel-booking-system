using Grpc.Core;
using PricingService.Application.DTOs;
using PricingService.Application.Services;
using PricingService.Grpc;

namespace PricingService.Services;

/// <summary>
/// gRPC service implementation for pricing operations
/// </summary>
public class PricingGrpcService : PricingGrpc.PricingGrpcBase
{
    private readonly IPricingService _pricingService;
    private readonly ILogger<PricingGrpcService> _logger;

    public PricingGrpcService(IPricingService pricingService, ILogger<PricingGrpcService> logger)
    {
        _pricingService = pricingService;
        _logger = logger;
    }

    public override async Task<CalculatePriceResponse> CalculatePrice(CalculatePriceRequest request, ServerCallContext context)
    {
        try
        {
            var calculationRequest = new PriceCalculationRequest
            {
                RoomId = request.RoomId,
                RoomType = request.RoomType,
                CheckIn = DateTime.Parse(request.CheckIn),
                CheckOut = DateTime.Parse(request.CheckOut),
                Occupancy = request.Occupancy,
                PromoCode = request.PromoCode
            };

            var result = await _pricingService.CalculatePriceAsync(calculationRequest, context.CancellationToken);

            return new CalculatePriceResponse
            {
                RoomId = result.RoomId,
                RoomType = result.RoomType,
                BasePrice = (double)result.BasePrice,
                FinalPrice = (double)result.FinalPrice,
                TotalPrice = (double)result.TotalPrice,
                Currency = result.Currency,
                Nights = result.Nights,
                CheckIn = result.CheckIn.ToString("O"),
                CheckOut = result.CheckOut.ToString("O"),
                CalculatedAt = result.CalculatedAt.ToString("O"),
                AppliedRules = { result.AppliedRules.Select(r => new AppliedRule
                {
                    RuleName = r.RuleName,
                    RuleType = r.RuleType,
                    PriceImpact = (double)r.PriceImpact,
                    Multiplier = (double)r.Multiplier
                })}
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating price via gRPC");
            throw new RpcException(new Status(StatusCode.Internal, ex.Message));
        }
    }

    public override async Task<RoomPriceResponse> GetRoomPrice(GetRoomPriceRequest request, ServerCallContext context)
    {
        try
        {
            var result = await _pricingService.GetRoomPriceAsync(request.RoomId, context.CancellationToken);
            if (result == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, $"Room price not found for RoomId: {request.RoomId}"));
            }

            return new RoomPriceResponse
            {
                Id = result.Id,
                RoomId = result.RoomId,
                RoomType = result.RoomType,
                BasePrice = (double)result.BasePrice,
                Currency = result.Currency,
                ValidFrom = result.ValidFrom.ToString("O"),
                ValidTo = result.ValidTo?.ToString("O") ?? "",
                IsActive = result.IsActive
            };
        }
        catch (RpcException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting room price via gRPC");
            throw new RpcException(new Status(StatusCode.Internal, ex.Message));
        }
    }

    public override async Task<RoomPriceListResponse> GetAllActivePrices(GetAllActivePricesRequest request, ServerCallContext context)
    {
        try
        {
            var result = await _pricingService.GetAllActivePricesAsync(context.CancellationToken);

            var response = new RoomPriceListResponse();
            response.Prices.AddRange(result.Select(p => new RoomPriceResponse
            {
                Id = p.Id,
                RoomId = p.RoomId,
                RoomType = p.RoomType,
                BasePrice = (double)p.BasePrice,
                Currency = p.Currency,
                ValidFrom = p.ValidFrom.ToString("O"),
                ValidTo = p.ValidTo?.ToString("O") ?? "",
                IsActive = p.IsActive
            }));

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all active prices via gRPC");
            throw new RpcException(new Status(StatusCode.Internal, ex.Message));
        }
    }
}
