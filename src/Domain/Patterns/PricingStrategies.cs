namespace BookingService.Domain.Patterns
{
    public interface IPricingStrategy
    {
        decimal Calculate(decimal basePrice, int nights);
    }

    public class StandardPricingStrategy : IPricingStrategy
    {
        public decimal Calculate(decimal basePrice, int nights) => basePrice * nights;
    }

    public class SeasonalPricingStrategy : IPricingStrategy
    {
        public decimal Calculate(decimal basePrice, int nights) => basePrice * nights * 1.25m;
    }

    public class OccupancyPricingStrategy : IPricingStrategy
    {
        private readonly int _occupancyPercent;

        public OccupancyPricingStrategy(int occupancyPercent) =>
            _occupancyPercent = occupancyPercent;

        public decimal Calculate(decimal basePrice, int nights)
        {
            var factor = _occupancyPercent switch
            {
                >= 85 => 1.50m,
                >= 70 => 1.25m,
                >= 50 => 1.10m,
                _ => 1.00m
            };

            return basePrice * nights * factor;
        }
    }
}
