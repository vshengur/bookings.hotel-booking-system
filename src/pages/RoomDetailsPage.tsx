import { useEffect, useState } from 'react';
import { useNavigate, useParams, useLocation } from 'react-router-dom';
import { getRoom } from '../api/gateway';
import type { Room, SearchParams } from '../types';

function nightsBetween(checkIn: string, checkOut: string): number {
  return Math.max(
    1,
    Math.round((new Date(checkOut).getTime() - new Date(checkIn).getTime()) / 86_400_000),
  );
}

export default function RoomDetailsPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const location = useLocation();
  const params: SearchParams | undefined = location.state?.params;

  const [room, setRoom] = useState<Room | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    if (!id) return;
    getRoom(Number(id))
      .then(setRoom)
      .catch(err => setError(err.message))
      .finally(() => setLoading(false));
  }, [id]);

  if (loading) return <div className="page"><p>Loading…</p></div>;
  if (error)   return <div className="page"><p className="error">{error}</p></div>;
  if (!room)   return <div className="page"><p>Room not found.</p></div>;

  const nights = params ? nightsBetween(params.checkIn, params.checkOut) : 1;
  const primaryImage = room.images.find(i => i.is_primary)?.url ?? room.images[0]?.url;

  return (
    <div className="page">
      <button className="back-btn" onClick={() => navigate(-1)}>← Back</button>

      {primaryImage && <img src={primaryImage} alt={room.room_type} className="room-hero" />}

      <h1>{room.room_type} — Room {room.room_number}</h1>
      <p className="room-meta">
        Floor {room.floor} · {room.capacity} guests · {room.bed_type} · {room.size} m²
      </p>
      <p>{room.description}</p>

      {room.amenities.length > 0 && (
        <>
          <h3>Amenities</h3>
          <ul>
            {room.amenities.map((a, i) => <li key={i}>{a.name}</li>)}
          </ul>
        </>
      )}

      {params && (
        <div className="booking-summary">
          <p>
            <strong>{params.checkIn}</strong> → <strong>{params.checkOut}</strong>
            &nbsp;({nights} night{nights !== 1 ? 's' : ''})
            &nbsp;· {params.adults} adult{params.adults !== 1 ? 's' : ''}
            {params.children > 0 && `, ${params.children} child${params.children !== 1 ? 'ren' : ''}`}
          </p>
        </div>
      )}

      <button
        className="primary-btn"
        onClick={() => navigate('/booking', { state: { room, params } })}
      >
        Book this room
      </button>
    </div>
  );
}
