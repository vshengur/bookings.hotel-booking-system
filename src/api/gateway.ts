import type {
  SearchRoomsResponse,
  Room,
  Booking,
  PaymentIntentResponse,
} from '../types';

const BASE = import.meta.env.VITE_GATEWAY_URL ?? 'http://localhost:8080';

async function request<T>(path: string, init?: RequestInit): Promise<T> {
  const res = await fetch(`${BASE}${path}`, {
    headers: { 'Content-Type': 'application/json', ...init?.headers },
    ...init,
  });
  if (!res.ok) {
    const text = await res.text().catch(() => res.statusText);
    throw new Error(`${res.status} ${path}: ${text}`);
  }
  if (res.status === 204) return undefined as T;
  return res.json();
}

// ── Rooms ──────────────────────────────────────────────────────────────────

export function searchRooms(
  checkIn: string,
  checkOut: string,
  adults: number,
  children: number,
): Promise<SearchRoomsResponse> {
  const q = new URLSearchParams({
    checkIn,
    checkOut,
    adults: String(adults),
    children: String(children),
  });
  return request(`/api/rooms/search?${q}`);
}

export function getRoom(id: number): Promise<Room> {
  return request(`/api/rooms/${id}`);
}

// ── Bookings ───────────────────────────────────────────────────────────────

export interface CreateBookingPayload {
  bookingId: string;
  guestId: string;
  checkIn: string;   // "YYYY-MM-DD"
  checkOut: string;  // "YYYY-MM-DD"
  items: {
    roomId: number;
    adults: number;
    children: number;
    nights: number;
    pricePerNight: { amount: number; currency: string };
  }[];
  promoCode?: string;
}

export function createBooking(payload: CreateBookingPayload): Promise<{ bookingId: string }> {
  return request('/api/booking', { method: 'POST', body: JSON.stringify(payload) });
}

export function getBooking(id: string): Promise<Booking> {
  return request(`/api/booking/${id}`);
}

// ── Payment ────────────────────────────────────────────────────────────────

export function createPaymentIntent(bookingId: string): Promise<PaymentIntentResponse> {
  return request('/payment/intent', {
    method: 'POST',
    body: JSON.stringify({ bookingId }),
  });
}

// PoC only: simulate PSP webhook so payment moves booking forward
export function simulatePaymentSuccess(bookingId: string): Promise<void> {
  return request('/payment/webhook', {
    method: 'POST',
    body: JSON.stringify({ bookingId, status: 'Succeeded' }),
  });
}
