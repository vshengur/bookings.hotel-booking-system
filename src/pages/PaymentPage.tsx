import { useEffect, useState } from 'react';
import { useNavigate, useParams, useLocation } from 'react-router-dom';
import { createPaymentIntent, simulatePaymentSuccess, getBooking } from '../api/gateway';

export default function PaymentPage() {
  const { bookingId } = useParams<{ bookingId: string }>();
  const navigate = useNavigate();
  const location = useLocation();
  const total: string = location.state?.total ?? '—';

  const [intentId, setIntentId] = useState('');
  const [loadingIntent, setLoadingIntent] = useState(true);
  const [loadingPay, setLoadingPay] = useState(false);
  const [error, setError] = useState('');

  useEffect(() => {
    if (!bookingId) return;
    createPaymentIntent(bookingId)
      .then(res => setIntentId(res.intentId))
      .catch(err => setError(err.message))
      .finally(() => setLoadingIntent(false));
  }, [bookingId]);

  async function handlePay() {
    if (!bookingId) return;
    setError('');
    setLoadingPay(true);
    try {
      await simulatePaymentSuccess(bookingId);
      // Poll for booking status change (up to 10s)
      for (let i = 0; i < 10; i++) {
        await new Promise(r => setTimeout(r, 1000));
        const booking = await getBooking(bookingId);
        if (['Reserved', 'Confirmed', 'Failed', 'Cancelled'].includes(booking.status)) {
          navigate(`/confirmation/${bookingId}`);
          return;
        }
      }
      // Fallback: navigate anyway
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

      <div className="booking-summary">
        <p><strong>Booking ID:</strong> {bookingId}</p>
        <p><strong>Amount due:</strong> {total} EUR</p>
        {intentId && <p><strong>Payment intent:</strong> {intentId}</p>}
      </div>

      {loadingIntent && <p>Creating payment intent…</p>}
      {error && <p className="error">{error}</p>}

      {!loadingIntent && !error && (
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
