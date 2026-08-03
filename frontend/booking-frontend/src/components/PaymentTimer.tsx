import { useEffect, useState } from 'react';

interface Props {
  expiresAt: string;   // ISO 8601 UTC — comes from booking.paymentExpiresAt
  onExpired?: () => void;
}

export default function PaymentTimer({ expiresAt, onExpired }: Readonly<Props>) {
  const deadline = new Date(expiresAt).getTime();

  const [remaining, setRemaining] = useState(() => Math.max(0, deadline - Date.now()));

  useEffect(() => {
    if (remaining === 0) { onExpired?.(); return; }

    const id = setInterval(() => {
      const left = Math.max(0, deadline - Date.now());
      setRemaining(left);
      if (left === 0) { clearInterval(id); onExpired?.(); }
    }, 1000);

    return () => clearInterval(id);
  }, [deadline, onExpired, remaining]);

  if (remaining === 0) {
    return (
      <p className="payment-timer expired">
        ⏰ Payment window expired — booking will be cancelled shortly.
      </p>
    );
  }

  const totalSec = Math.floor(remaining / 1000);
  const minutes  = Math.floor(totalSec / 60);
  const seconds  = totalSec % 60;
  const isUrgent = remaining < 3 * 60 * 1000;

  return (
    <p className={`payment-timer${isUrgent ? ' urgent' : ''}`}>
      ⏱ Pay within{' '}
      <strong>{String(minutes).padStart(2, '0')}:{String(seconds).padStart(2, '0')}</strong>
      {' '}— booking expires if unpaid.
    </p>
  );
}
