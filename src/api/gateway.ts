import type {
  SearchRoomsResponse,
  Room,
  Booking,
  PaymentIntentResponse,
} from '../types';
import { getToken } from '../auth';

const BASE = import.meta.env.VITE_GATEWAY_URL ?? 'http://localhost:8080';

function authHeaders(): Record<string, string> {
  const token = getToken();
  return token ? { Authorization: `Bearer ${token}` } : {};
}

async function request<T>(path: string, init?: RequestInit): Promise<T> {
  const res = await fetch(`${BASE}${path}`, {
    headers: { 'Content-Type': 'application/json', ...authHeaders(), ...init?.headers },
    ...init,
  });
  if (!res.ok) {
    const text = await res.text().catch(() => res.statusText);
    throw new Error(`${res.status} ${path}: ${text}`);
  }
  if (res.status === 204) return undefined as T;
  const text = await res.text();
  return text ? JSON.parse(text) as T : undefined as T;
}

// ── Rooms ──────────────────────────────────────────────────────────────────

function toRfc3339(date: string): string {
  return date.includes('T') ? date : `${date}T00:00:00Z`;
}

export function searchRooms(
  checkIn: string,
  checkOut: string,
  adults: number,
  children: number,
): Promise<SearchRoomsResponse> {
  const q = new URLSearchParams({
    checkIn:  toRfc3339(checkIn),
    checkOut: toRfc3339(checkOut),
    adults: String(adults),
    children: String(children),
  });
  return request(`/api/rooms/search?${q}`);
}

export function getRoom(id: number): Promise<Room> {
  return request(`/api/rooms/${id}`);
}

export function checkRoomAvailability(
  id: number,
  checkIn: string,
  checkOut: string,
): Promise<{ is_available: boolean }> {
  const q = new URLSearchParams({ checkIn: toRfc3339(checkIn), checkOut: toRfc3339(checkOut) });
  return request(`/api/rooms/${id}/availability?${q}`);
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

export function cancelBooking(id: string, reason?: string): Promise<void> {
  return request(`/api/booking/${id}/cancel`, {
    method: 'POST',
    body: JSON.stringify({ reason: reason ?? 'Cancelled by user' }),
  });
}

export function getMyBookings(guestId: string, page = 1): Promise<Booking[]> {
  const q = new URLSearchParams({ guestId, page: String(page), pageSize: '20' });
  return request(`/api/bookings?${q}`);
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
