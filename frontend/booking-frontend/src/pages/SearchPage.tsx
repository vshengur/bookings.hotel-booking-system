import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { searchRooms, getMyBookings } from '../api/gateway';
import { useSearch } from '../SearchContext';
import { getGuestId, isAuthenticated } from '../auth';
import type { Booking, BookingStatus, Room } from '../types';

const today = () => new Date().toISOString().slice(0, 10);

const ACTIVE_STATUSES = new Set<BookingStatus>(['Created', 'Pending', 'AwaitingPayment', 'Reserved', 'Confirmed']);

function datesOverlap(
  aIn: string, aOut: string,
  bIn: string, bOut: string,
): boolean {
  return aIn < bOut && aOut > bIn;
}

export default function SearchPage() {
  const navigate = useNavigate();
  const { params, setParams } = useSearch();
  const [rooms, setRooms]                     = useState<Room[]>([]);
  const [conflictBookings, setConflictBookings] = useState<Booking[]>([]);
  const [loading, setLoading]                 = useState(false);
  const [error, setError]                     = useState('');

  async function handleSearch(e: React.FormEvent) {
    e.preventDefault();
    setError('');
    setConflictBookings([]);
    setLoading(true);
    try {
      const [res, myBookings] = await Promise.all([
        searchRooms(params.checkIn, params.checkOut, params.adults, params.children),
        isAuthenticated()
          ? getMyBookings(getGuestId()).catch(() => [] as Booking[])
          : Promise.resolve([] as Booking[]),
      ]);
      setRooms(res.rooms ?? []);

      // Find active bookings overlapping the searched period
      const conflicts = myBookings.filter(
        b => ACTIVE_STATUSES.has(b.status) &&
             datesOverlap(params.checkIn, params.checkOut, b.checkIn, b.checkOut),
      );
      setConflictBookings(conflicts);
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : 'Search failed');
    } finally {
      setLoading(false);
    }
  }

  function set<K extends keyof typeof params>(field: K, value: typeof params[K]) {
    setParams({ ...params, [field]: value });
  }

  return (
    <div className="page">
      <h1>Find a room</h1>

      <form className="search-form" onSubmit={handleSearch}>
        <label>
          Check-in
          <input type="date" value={params.checkIn} min={today()}
            onChange={e => set('checkIn', e.target.value)} required />
        </label>
        <label>
          Check-out
          <input type="date" value={params.checkOut} min={params.checkIn}
            onChange={e => set('checkOut', e.target.value)} required />
        </label>
        <label>
          Adults
          <input type="number" value={params.adults} min={1} max={10}
            onChange={e => set('adults', +e.target.value)} required />
        </label>
        <label>
          Children
          <input type="number" value={params.children} min={0} max={10}
            onChange={e => set('children', +e.target.value)} />
        </label>
        <button type="submit" disabled={loading}>
          {loading ? 'Searching…' : 'Search'}
        </button>
      </form>

      {error && <p className="error">{error}</p>}

      {/* ── Existing booking warning ── */}
      {conflictBookings.length > 0 && (
        <div className="conflict-banner">
          <strong>⚠ You already have {conflictBookings.length === 1 ? 'a booking' : 'bookings'} for these dates:</strong>
          <ul>
            {conflictBookings.map(b => (
              <li key={b.id}>
                {b.checkIn} → {b.checkOut} — {b.status}{' '}
                <button className="room-link" onClick={() => navigate(`/confirmation/${b.id}`)}>
                  View booking →
                </button>
              </li>
            ))}
          </ul>
        </div>
      )}

      {rooms.length === 0 && !loading && !error && (
        <p className="hint">Enter dates and press Search to see available rooms.</p>
      )}

      <div className="room-list">
        {rooms.map(room => (
          <button key={room.id} className="room-card"
            onClick={() => navigate(`/rooms/${room.id}`)}>
            {(room.images ?? []).find(i => i.is_primary)?.url && (
              <img src={(room.images ?? []).find(i => i.is_primary)!.url} alt={room.room_type} />
            )}
            <div className="room-card-body">
              <h3>{room.room_type} — Room {room.room_number}</h3>
              <p>{room.description}</p>
              <p>Floor {room.floor} · {room.capacity} guests · {room.bed_type} · {room.size} m²</p>
              <span className="room-card-cta">View &amp; Book →</span>
            </div>
          </button>
        ))}
      </div>
    </div>
  );
}
