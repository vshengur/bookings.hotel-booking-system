import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { getMyBookings } from '../api/gateway';
import { getGuestId } from '../auth';
import type { Booking, BookingStatus } from '../types';

const STATUS_LABEL: Record<BookingStatus, string> = {
  Created:        'Processing…',
  Pending:        'Processing…',
  AwaitingPayment:'Awaiting payment',
  Reserved:       'Reserved',
  Confirmed:      'Confirmed',
  Cancelled:      'Cancelled',
  Failed:         'Payment failed',
  Expired:        'Expired',
};

const STATUS_CLASS: Record<BookingStatus, string> = {
  Created:        '',
  Pending:        '',
  AwaitingPayment:'',
  Reserved:       'status-ok',
  Confirmed:      'status-ok',
  Cancelled:      'status-bad',
  Failed:         'status-bad',
  Expired:        'status-bad',
};

export default function MyBookingsPage() {
  const navigate = useNavigate();
  const [bookings, setBookings] = useState<Booking[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    const guestId = getGuestId();
    getMyBookings(guestId)
      .then(setBookings)
      .catch(err => setError(err instanceof Error ? err.message : 'Failed to load bookings'))
      .finally(() => setLoading(false));
  }, []);

  return (
    <div className="page">
      <h1>My bookings</h1>

      {loading && <p className="hint">Loading…</p>}
      {error   && <p className="error">{error}</p>}

      {!loading && !error && bookings.length === 0 && (
        <p className="hint">
          No bookings yet.{' '}
          <button className="back-btn" onClick={() => navigate('/')}>Search rooms →</button>
        </p>
      )}

      <div className="booking-list">
        {bookings.map(b => (
          <button
            key={b.id}
            className="booking-card"
            onClick={() => navigate(`/confirmation/${b.id}`)}
          >
            <div className="booking-card-body">
              <div className="booking-card-dates">
                <span>{b.checkIn}</span>
                <span className="arrow">→</span>
                <span>{b.checkOut}</span>
              </div>
              <div className="booking-card-meta">
                <p className="booking-card-total">{b.totalPrice}</p>
                {!!b.roomId && (
                  <button className="room-link" onClick={e => { e.stopPropagation(); navigate(`/rooms/${b.roomId}`); }}>
                    Room {b.roomId} →
                  </button>
                )}
              </div>
            </div>
            <span className={`booking-status ${STATUS_CLASS[b.status] ?? ''}`}>
              {STATUS_LABEL[b.status] ?? b.status}
            </span>
          </button>
        ))}
      </div>
    </div>
  );
}
