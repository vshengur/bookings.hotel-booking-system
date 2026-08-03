import { useEffect, useState } from 'react';
import { useNavigate, useParams, useLocation } from 'react-router-dom';
import { createPaymentIntent, simulatePaymentSuccess, getBooking } from '../api/gateway';
import PaymentTimer from '../components/PaymentTimer';
import type { Booking } from '../types';

async function fetchIntentWithRetry(bookingId: string, signal: AbortSignal): Promise<string> {
  for (let attempt = 0; attempt < 8; attempt++) {
    if (signal.aborted) throw new DOMException('Aborted', 'AbortError');
    try {
      const res = await createPaymentIntent(bookingId);
      return res.intentId;
    } catch (err) {
      if (attempt === 7) throw err;
      await new Promise(r => setTimeout(r, 1000));
    }
  }
  throw new Error('unreachable');
}

export default function PaymentPage() {
  const { bookingId } = useParams<{ bookingId: string }>();
  const navigate = useNavigate();
  const location = useLocation();
  const total: string = location.state?.total ?? '—';

  const [booking, setBooking]           = useState<Booking | null>(null);
  const [intentId, setIntentId]         = useState('');
  const [loadingIntent, setLoadingIntent] = useState(true);
  const [loadingPay, setLoadingPay]     = useState(false);
  const [error, setError]               = useState('');
  const [expired, setExpired]           = useState(false);

  // Load booking (for createdAt → timer) and intent in parallel
  useEffect(() => {
    if (!bookingId) return;
    const controller = new AbortController();

    getBooking(bookingId)
      .then(setBooking)
      .catch(() => { /* non-critical, timer just won't show */ });

    fetchIntentWithRetry(bookingId, controller.signal)
      .then(id => setIntentId(id))
      .catch(err => {
        if ((err as DOMException).name !== 'AbortError')
          setError(err instanceof Error ? err.message : 'Failed to load payment intent');
      })
      .finally(() => setLoadingIntent(false));

    return () => controller.abort();
  }, [bookingId]);

  async function handlePay() {
    if (!bookingId) return;
    setError('');
    setLoadingPay(true);
    try {
      await simulatePaymentSuccess(bookingId);
      for (let i = 0; i < 10; i++) {
        await new Promise(r => setTimeout(r, 1000));
        const b = await getBooking(bookingId);
        if (['Reserved', 'Confirmed', 'Failed', 'Cancelled'].includes(b.status)) {
          navigate(`/confirmation/${bookingId}`);
          return;
        }
      }
      navigate(`/confirmation/${bookingId}`);
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : 'Payment failed');
    } finally {
      setLoadingPay(false);
    }
  }

  return (
    <div className="page">
      <h1>Payment</h1>

      {/* ── Countdown timer ── */}
      {booking?.paymentExpiresAt && (
        <PaymentTimer
          expiresAt={booking.paymentExpiresAt}
          onExpired={() => setExpired(true)}
        />
      )}

      {expired && (
        <div className="conflict-banner">
          Time is up — your booking has been cancelled. You can search for another room.
        </div>
      )}

      <div className="booking-summary">
        <p><strong>Booking ID:</strong> {bookingId}</p>
        <p><strong>Amount due:</strong> {total} EUR</p>
        {intentId && <p><strong>Payment intent:</strong> {intentId}</p>}
      </div>

      {loadingIntent && <p className="hint">Creating payment intent…</p>}
      {error && <p className="error">{error}</p>}

      {!loadingIntent && !error && !expired && (
        <div className="test-card">
          <h3>Test mode</h3>
          <p>Card: <code>4242 4242 4242 4242</code></p>
          <p>Expiry: any future date &nbsp; CVC: any 3 digits</p>
          <button className="primary-btn" onClick={handlePay} disabled={loadingPay}>
            {loadingPay ? 'Processing…' : 'Pay now (test mode)'}
          </button>
        </div>
      )}
    </div>
  );
}
