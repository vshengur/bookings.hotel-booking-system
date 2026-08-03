namespace PaymentService.API.Services;

public class PaymentProviderUnavailableException : Exception
{
    public PaymentProviderUnavailableException(string message, int retryAfterSeconds, Exception innerException)
        : base(message, innerException)
    {
        RetryAfterSeconds = retryAfterSeconds;
    }

    public int RetryAfterSeconds { get; }
}
