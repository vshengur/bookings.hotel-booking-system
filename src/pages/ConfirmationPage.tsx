import { useEffect, useRef, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { getBooking, cancelBooking } from '../api/gateway';
import PaymentTimer from '../components/PaymentTimer';
import type { Booking, BookingStatus } from '../types';

const STATUS_LABEL: Record<BookingStatus, string> = {
  Confirmed:      '✓ Booking confirmed',
  Reserved:       '⏳ Reserved — awaiting PMS confirmation',
  AwaitingPayment:'⏳ Awaiting payment',
  Cancelled:      '✗ Booking cancelled',
  Failed:         '✗ Payment failed',
  Expired:        '✗ Booking expired',
  Created:        '⏳ Processing…',
  Pending:        '⏳ Processing…',
};

const PENDING_STATUSES    = new Set<BookingStatus>(['Created', 'Pending', 'AwaitingPayment', 'Reserved']);
const CANCELLABLE_STATUSES = new Set<BookingStatus>(['Created', 'Pending', 'AwaitingPayment']);

export default function ConfirmationPage() {
  const { bookingId } = useParams<{ bookingId: string }>();
  const navigate = useNavigate();
  const [booking, setBooking]         = useState<Booking | null>(null);
  const [loading, setLoading]         = useState(true);
  const [error, setError]             = useState('');
  const [confirmCancel, setConfirmCancel] = useState(false);
  const [cancelling, setCancelling]   = useState(false);
  const [cancelError, setCancelError] = useState('');
  const pollRef = useRef<ReturnType<typeof setInterval> | null>(null);

  useEffect(() => {
    if (!bookingId) return;

    function stopPolling() {
      if (pollRef.current) { clearInterval(pollRef.current); pollRef.current = null; }
    }

    async function fetchStatus() {
      try {
        const b = await getBooking(bookingId!);
        setBooking(b);
        setLoading(false);
        if (!PENDING_STATUSES.has(b.status)) stopPolling();
      } catch (err: unknown) {
        setError(err instanceof Error ? err.message : 'Failed to load booking');
        setLoading(false);
        stopPolling();
      }
    }

    fetchStatus();
    pollRef.current = setInterval(fetchStatus, 2000);
    return () => stopPolling();
  }, [bookingId]);

  async function handleCancel() {
    if (!bookingId) return;
    setCancelling(true);
    setCancelError('');
    try {
      await cancelBooking(bookingId, 'Cancelled by user');
      // Refresh booking status
      const updated = await getBooking(bookingId);
      setBooking(updated);
      setConfirmCancel(false);
    } catch (err: unknown) {
      setCancelError(err instanceof Error ? err.message : 'Cancellation failed');
    } finally {
      setCancelling(false);
    }
  }

  const isSuccess   = booking?.status === 'Confirmed' || booking?.status === 'Reserved';
  const canCancel   = booking != null && CANCELLABLE_STATUSES.has(booking.status);
  const canPay      = booking?.status === 'AwaitingPayment';

  return (
    <div className="page confirmation">
      <h1>Booking summary</h1>

      {loading && <p className="hint">Loading…</p>}
      {error   && <p className="error">{error}</p>}

      {booking && (
        <>
          <div className={`booking-summary ${isSuccess ? 'success' : ''}`}>
            <p className="status-label">
              {STATUS_LABEL[booking.status] ?? booking.status}
            </p>
            <p><strong>Booking ID:</strong> {booking.id}</p>
            <p><strong>Check-in:</strong>  {booking.checkIn}</p>
            <p><strong>Check-out:</strong> {booking.checkOut}</p>
            <p><strong>Total:</strong>     {booking.totalPrice}</p>
            {!!booking.roomId && (
              <p>
                <strong>Room:</strong>{' '}
                <button className="room-link" onClick={() => navigate(`/rooms/${booking.roomId}`)}>
                  #{booking.roomId} — view details →
                </button>
              </p>
            )}
          </div>

          {/* ── Pay now + timer ── */}
          {canPay && (
            <>
              {booking.paymentExpiresAt && (
                <PaymentTimer expiresAt={booking.paymentExpiresAt} />
              )}
              <button className="primary-btn" onClick={() => navigate(`/payment/${bookingId}`)}>
                Pay now →
              </button>
            </>
          )}

          {/* ── Cancel section ── */}
          {canCancel && (
            <div className="cancel-section">
              {confirmCancel ? (
                <div className="cancel-confirm">
                  <p>Are you sure you want to cancel this booking?</p>
                  {cancelError && <p className="error">{cancelError}</p>}
                  <div className="cancel-confirm-actions">
                    <button className="cancel-btn-confirm" onClick={handleCancel} disabled={cancelling}>
                      {cancelling ? 'Cancelling…' : 'Yes, cancel booking'}
                    </button>
                    <button className="back-btn" onClick={() => { setConfirmCancel(false); setCancelError(''); }}>
                      No, keep it
                    </button>
                  </div>
                </div>
              ) : (
                <button className="cancel-btn" onClick={() => setConfirmCancel(true)}>
                  Cancel this booking
                </button>
              )}
            </div>
          )}
        </>
      )}

      <div className="confirmation-actions">
        <button className="back-btn" onClick={() => navigate('/')}>
          ← Search again
        </button>
        <button className="nav-btn" onClick={() => navigate('/my-bookings')}>
          My bookings
        </button>
      </div>
    </div>
  );
}
