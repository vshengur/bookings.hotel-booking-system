import { useCallback, useEffect, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { getRoom, checkRoomAvailability, getMyBookings } from '../api/gateway';
import { useSearch } from '../SearchContext';
import { getGuestId, isAuthenticated } from '../auth';
import type { Booking, BookingStatus, Room } from '../types';

const FINAL_STATUSES = new Set<BookingStatus>(['Cancelled', 'Failed', 'Expired']);

function nightsBetween(checkIn: string, checkOut: string): number {
  return Math.max(
    1,
    Math.round((new Date(checkOut).getTime() - new Date(checkIn).getTime()) / 86_400_000),
  );
}

export default function RoomDetailsPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { params } = useSearch();

  const [room, setRoom]                   = useState<Room | null>(null);
  const [available, setAvailable]         = useState<boolean | null>(null);
  const [userBookings, setUserBookings]   = useState<Booking[]>([]);
  const [activeImg, setActiveImg]         = useState(0);
  const [lightbox, setLightbox]           = useState<number | null>(null);
  const [loading, setLoading]             = useState(true);
  const [error, setError]                 = useState('');

  const closeLightbox = useCallback(() => setLightbox(null), []);

  const lightboxPrev = useCallback((images: Room['images']) => {
    setLightbox(i => (i === null ? null : (i - 1 + images.length) % images.length));
  }, []);

  const lightboxNext = useCallback((images: Room['images']) => {
    setLightbox(i => (i === null ? null : (i + 1) % images.length));
  }, []);

  useEffect(() => {
    if (lightbox === null) return;
    const onKey = (e: KeyboardEvent) => {
      if (e.key === 'Escape') closeLightbox();
    };
    document.addEventListener('keydown', onKey);
    return () => document.removeEventListener('keydown', onKey);
  }, [lightbox, closeLightbox]);

  useEffect(() => {
    if (!id) return;
    const roomId = Number(id);

    Promise.all([
      getRoom(roomId),
      checkRoomAvailability(roomId, params.checkIn, params.checkOut),
      isAuthenticated()
        ? getMyBookings(getGuestId()).catch(() => [] as Booking[])
        : Promise.resolve([] as Booking[]),
    ])
      .then(([r, avail, bookings]) => {
        setRoom(r);
        setAvailable(avail.is_available);
        // Show only active (non-final) bookings for this room
        setUserBookings(
          bookings.filter(b => b.roomId === roomId && !FINAL_STATUSES.has(b.status)),
        );
      })
      .catch(err => setError(err instanceof Error ? err.message : 'Failed to load room'))
      .finally(() => setLoading(false));
  }, [id, params.checkIn, params.checkOut]);

  if (loading) return <div className="page"><p className="hint">Loading…</p></div>;
  if (error)   return <div className="page"><p className="error">{error}</p></div>;
  if (!room)   return <div className="page"><p>Room not found.</p></div>;

  const images   = room.images ?? [];
  const amenities = room.amenities ?? [];
  const nights   = nightsBetween(params.checkIn, params.checkOut);

  return (
    <div className="page">
      <button className="back-btn" onClick={() => navigate(-1)}>← Back</button>

      {/* ── Image gallery ── */}
      {images.length > 0 && (
        <div className="room-gallery">
          <button className="room-hero-btn" onClick={() => setLightbox(activeImg)}>
            <img
              className="room-hero"
              src={images[activeImg]?.url}
              alt={images[activeImg]?.title ?? room.room_type}
            />
            <span className="room-hero-zoom">🔍 Click to enlarge</span>
          </button>
          {images.length > 1 && (
            <div className="room-thumbnails">
              {images.map((img, i) => (
                <button
                  key={img.url}
                  className={`room-thumb${i === activeImg ? ' active' : ''}`}
                  onClick={() => setActiveImg(i)}
                >
                  <img src={img.url} alt={img.title} />
                </button>
              ))}
            </div>
          )}
        </div>
      )}

      {/* ── Lightbox ── */}
      {lightbox !== null && images.length > 0 && (
        <dialog className="lightbox-overlay" open aria-label="Image viewer">
          {/* Transparent full-screen button — closes lightbox when clicking the backdrop */}
          <button className="lightbox-backdrop" onClick={closeLightbox} aria-label="Close lightbox" />
          <button className="lightbox-close" onClick={closeLightbox} aria-label="Close">✕</button>
          {images.length > 1 && (
            <button className="lightbox-nav lightbox-prev" aria-label="Previous image"
              onClick={() => lightboxPrev(images)}>‹</button>
          )}
          <figure className="lightbox-content">
            <img src={images[lightbox]?.url} alt={images[lightbox]?.title ?? ''} />
            {images[lightbox]?.title && (
              <figcaption className="lightbox-caption">{images[lightbox].title}</figcaption>
            )}
          </figure>
          {images.length > 1 && (
            <button className="lightbox-nav lightbox-next" aria-label="Next image"
              onClick={() => lightboxNext(images)}>›</button>
          )}
        </dialog>
      )}

      <h1>{room.room_type} — Room {room.room_number}</h1>
      <p className="room-meta">
        Floor {room.floor} · {room.capacity} guests · {room.bed_type} · {room.size} m²
      </p>
      <p>{room.description}</p>

      {amenities.length > 0 && (
        <>
          <h3>Amenities</h3>
          <ul className="amenity-list">
            {amenities.map(a => <li key={`${a.amenity_type}-${a.name}`}>{a.name}</li>)}
          </ul>
        </>
      )}

      {/* ── Existing user bookings for this room ── */}
      {userBookings.length > 0 && (
        <div className="room-existing-bookings">
          <h3>Your active bookings for this room</h3>
          {userBookings.map(b => (
            <div key={b.id} className="room-booking-item">
              <span>{b.checkIn} → {b.checkOut}</span>
              <span className={`booking-status ${b.status === 'Confirmed' || b.status === 'Reserved' ? 'status-ok' : ''}`}>
                {b.status}
              </span>
              <button className="room-link" onClick={() => navigate(`/confirmation/${b.id}`)}>
                View →
              </button>
            </div>
          ))}
        </div>
      )}

      {/* ── Dates + Book button ── */}
      <div className="booking-summary">
        <p>
          <strong>{params.checkIn}</strong> → <strong>{params.checkOut}</strong>
          &nbsp;({nights} night{nights === 1 ? '' : 's'})
          &nbsp;· {params.adults} adult{params.adults === 1 ? '' : 's'}
          {params.children > 0 && `, ${params.children} child${params.children === 1 ? '' : 'ren'}`}
        </p>
        {available === false && (
          <p className="error">This room is not available for the selected dates.</p>
        )}
      </div>

      <button
        className="primary-btn"
        disabled={available === false}
        onClick={() => navigate('/booking', { state: { room } })}
      >
        {available === false ? 'Not available' : 'Book this room'}
      </button>
    </div>
  );
}
