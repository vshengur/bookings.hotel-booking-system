import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { searchRooms } from '../api/gateway';
import type { Room, SearchParams } from '../types';

const today = () => new Date().toISOString().slice(0, 10);
const tomorrow = () => {
  const d = new Date();
  d.setDate(d.getDate() + 1);
  return d.toISOString().slice(0, 10);
};

export default function SearchPage() {
  const navigate = useNavigate();
  const [params, setParams] = useState<SearchParams>({
    checkIn: today(),
    checkOut: tomorrow(),
    adults: 2,
    children: 0,
  });
  const [rooms, setRooms] = useState<Room[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

  async function handleSearch(e: React.FormEvent) {
    e.preventDefault();
    setError('');
    setLoading(true);
    try {
      const res = await searchRooms(params.checkIn, params.checkOut, params.adults, params.children);
      setRooms(res.rooms ?? []);
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : 'Search failed');
    } finally {
      setLoading(false);
    }
  }

  function set(field: keyof SearchParams, value: string | number) {
    setParams(p => ({ ...p, [field]: value }));
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

      {rooms.length === 0 && !loading && !error && (
        <p className="hint">Enter dates and press Search to see available rooms.</p>
      )}

      <div className="room-list">
        {rooms.map(room => (
          <div key={room.id} className="room-card"
            onClick={() => navigate(`/rooms/${room.id}`, { state: { params } })}>
            {room.images.find(i => i.is_primary)?.url && (
              <img src={room.images.find(i => i.is_primary)!.url} alt={room.room_type} />
            )}
            <div className="room-card-body">
              <h3>{room.room_type} — Room {room.room_number}</h3>
              <p>{room.description}</p>
              <p>Floor {room.floor} · {room.capacity} guests · {room.bed_type} · {room.size} m²</p>
              <button>View &amp; Book →</button>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}
