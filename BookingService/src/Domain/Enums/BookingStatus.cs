namespace BookingService.Domain.Enums;

public enum BookingStatus
{
    Pending,    // Ожидает подтверждения
    Confirmed,  // Подтверждено
    Cancelled,  // Отменено
    Completed,  // Завершено
    NoShow      // Гость не явился
} 