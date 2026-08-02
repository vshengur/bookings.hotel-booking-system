import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../AuthContext';
import { getMyBookings } from '../api/gateway';
import { getGuestId } from '../auth';
import type { Booking, BookingStatus } from '../types';

const STATUS_LABEL: Record<BookingStatus, string> = {
  Confirmed:      'Confirmed',
  Reserved:       'Reserved',
  AwaitingPayment:'Awaiting payment',
  Cancelled:      'Cancelled',
  Failed:         'Failed',
  Expired:        'Expired',
  Created:        'Processing…',
  Pending:        'Processing…',
};

const STATUS_CLASS: Record<BookingStatus, string> = {
  Confirmed:      'status-ok',
  Reserved:       'status-ok',
  AwaitingPayment:'',
  Cancelled:      'status-bad',
  Failed:         'status-bad',
  Expired:        'status-bad',
  Created:        '',
  Pending:        '',
};

export default function ProfilePage() {
  const navigate = useNavigate();
  const { authed, user, logout } = useAuth();
  const [bookings, setBookings] = useState<Booking[]>([]);
  const [loading, setLoading]   = useState(true);
  const [error, setError]       = useState('');

  useEffect(() => {
    if (!authed) { navigate('/'); return; }
    getMyBookings(getGuestId())
      .then(setBookings)
      .catch(err => setError(err instanceof Error ? err.message : 'Failed to load bookings'))
      .finally(() => setLoading(false));
  }, [authed, navigate]);

  if (!authed) return null;

  return (
    <div className="page">
      {/* ── User card ── */}
      <div className="profile-card">
        {user?.picture
          ? <img src={user.picture} alt={user.name ?? ''} className="profile-avatar" />
          : <div className="profile-avatar-placeholder">
              {(user?.name ?? user?.email ?? '?')[0].toUpperCase()}
            </div>
        }
        <div className="profile-info">
          {user?.name  && <p className="profile-name">{user.name}</p>}
          {user?.email && <p className="profile-email">{user.email}</p>}
          <p className="profile-id">Guest ID: <code>{getGuestId()}</code></p>
        </div>
        <button className="nav-btn-outline profile-logout" onClick={logout}>Sign out</button>
      </div>

      {/* ── Bookings ── */}
      <h2>My bookings</h2>

      {loading && <p className="hint">Loading…</p>}
      {error   && <p className="error">{error}</p>}
      {!loading && !error && bookings.length === 0 && (
        <p className="hint">No bookings yet. <button className="back-btn" onClick={() => navigate('/')}>Search rooms →</button></p>
      )}

      <div className="booking-list">
        {bookings.map(b => (
          <div key={b.id} className="booking-card"
            role="button" tabIndex={0}
            onClick={() => navigate(`/confirmation/${b.id}`)}
            onKeyDown={e => e.key === 'Enter' && navigate(`/confirmation/${b.id}`)}>
            <div className="booking-card-body">
              <div className="booking-card-dates">
                <span>{b.checkIn}</span><span className="arrow">→</span><span>{b.checkOut}</span>
              </div>
              <div className="booking-card-meta">
                <p className="booking-card-total">{b.totalPrice}</p>
                {b.roomId && (
                  <button className="room-link" onClick={e => { e.stopPropagation(); navigate(`/rooms/${b.roomId}`); }}>
                    Room {b.roomId} →
                  </button>
                )}
              </div>
            </div>
            <span className={`booking-status ${STATUS_CLASS[b.status] ?? ''}`}>
              {STATUS_LABEL[b.status] ?? b.status}
            </span>
          </div>
        ))}
      </div>
    </div>
  );
}
