import { useEffect, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { getBooking } from '../api/gateway';
import type { Booking } from '../types';

const STATUS_LABEL: Record<string, string> = {
  Confirmed:     '✓ Booking confirmed',
  Reserved:      '⏳ Reserved — waiting for PMS confirmation',
  AwaitingPayment: '⏳ Awaiting payment',
  Cancelled:     '✗ Booking cancelled',
  Failed:        '✗ Payment failed',
  Expired:       '✗ Booking expired',
  Created:       '⏳ Processing…',
};

export default function ConfirmationPage() {
  const { bookingId } = useParams<{ bookingId: string }>();
  const navigate = useNavigate();
  const [booking, setBooking] = useState<Booking | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    if (!bookingId) return;
    getBooking(bookingId)
      .then(setBooking)
      .catch(err => setError(err.message))
      .finally(() => setLoading(false));
  }, [bookingId]);

  const isSuccess = booking?.status === 'Confirmed' || booking?.status === 'Reserved';

  return (
    <div className="page confirmation">
      <h1>Booking summary</h1>

      {loading && <p>Loading…</p>}
      {error   && <p className="error">{error}</p>}

      {booking && (
        <div className={`booking-summary ${isSuccess ? 'success' : ''}`}>
          <p className="status-label">
            {STATUS_LABEL[booking.status] ?? booking.status}
          </p>
          <p><strong>Booking ID:</strong> {booking.id}</p>
          <p><strong>Check-in:</strong> {booking.checkIn}</p>
          <p><strong>Check-out:</strong> {booking.checkOut}</p>
          <p><strong>Total:</strong> {booking.totalPrice}</p>
          <p><strong>Status:</strong> {booking.status}</p>
        </div>
      )}

      <button className="back-btn" onClick={() => navigate('/')}>
        ← Back to search
      </button>
    </div>
  );
}
