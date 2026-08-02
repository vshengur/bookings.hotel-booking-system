import { useState } from 'react';
import { useNavigate, useLocation } from 'react-router-dom';
import { createBooking } from '../api/gateway';
import { getGuestId } from '../auth';
import { useSearch } from '../SearchContext';
import type { Room } from '../types';

// PoC: hardcoded price; replace with pricing-service call
const NIGHTLY_PRICE = 120;
const CURRENCY = 'EUR';

function nightsBetween(checkIn: string, checkOut: string) {
  return Math.max(
    1,
    Math.round((new Date(checkOut).getTime() - new Date(checkIn).getTime()) / 86_400_000),
  );
}

export default function BookingFormPage() {
  const navigate = useNavigate();
  const location = useLocation();
  const { params } = useSearch();

  const room: Room | undefined = location.state?.room;

  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

  if (!room) {
    return (
      <div className="page">
        <p className="error">Missing booking context. Please start from the search page.</p>
        <button onClick={() => navigate('/')}>Go to search</button>
      </div>
    );
  }

  const checkedRoom = room; // narrowed: Room (not undefined) — TypeScript can't narrow through closures
  const nights = nightsBetween(params.checkIn, params.checkOut);
  const total = (NIGHTLY_PRICE * nights).toFixed(2);

  async function handleConfirm(e: React.FormEvent) {
    e.preventDefault();
    setError('');
    setLoading(true);
    const bookingId = crypto.randomUUID();
    try {
      await createBooking({
        bookingId,
        guestId: getGuestId(),
        checkIn: params.checkIn,
        checkOut: params.checkOut,
        items: [{
          roomId: checkedRoom.id,
          adults: params.adults,
          children: params.children,
          nights,
          pricePerNight: { amount: NIGHTLY_PRICE, currency: CURRENCY },
        }],
      });
      navigate(`/payment/${bookingId}`, { state: { total } });
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : 'Booking failed');
    } finally {
      setLoading(false);
    }
  }

  return (
    <div className="page">
      <button className="back-btn" onClick={() => navigate(-1)}>← Back</button>
      <h1>Confirm your booking</h1>

      <div className="booking-summary">
        <p><strong>Room:</strong> {room.room_type} — {room.room_number}</p>
        <p><strong>Check-in:</strong>  {params.checkIn}</p>
        <p><strong>Check-out:</strong> {params.checkOut}</p>
        <p><strong>Nights:</strong>    {nights}</p>
        <p><strong>Guests:</strong> {params.adults} adult{params.adults === 1 ? '' : 's'}{params.children > 0 ? `, ${params.children} child${params.children === 1 ? '' : 'ren'}` : ''}</p>
        <p><strong>Price per night:</strong> {NIGHTLY_PRICE} {CURRENCY}</p>
        <p><strong>Total:</strong> {total} {CURRENCY}</p>
      </div>

      {error && <p className="error">{error}</p>}

      <form onSubmit={handleConfirm}>
        <button type="submit" className="primary-btn" disabled={loading}>
          {loading ? 'Creating booking…' : 'Proceed to payment →'}
        </button>
      </form>
    </div>
  );
}
