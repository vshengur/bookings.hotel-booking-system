export interface Room {
  id: number;
  room_number: string;
  room_type: string;
  floor: number;
  capacity: number;
  bed_type: string;
  size: number;
  description: string;
  is_active: boolean;
  amenities: Amenity[];
  images: RoomImage[];
}

export interface Amenity {
  amenity_type: string;
  name: string;
  description: string;
}

export interface RoomImage {
  url: string;
  title: string;
  is_primary: boolean;
}

export interface SearchRoomsResponse {
  rooms: Room[];
  total_count: number;
  page: number;
  page_size: number;
  total_pages: number;
}

export interface AvailabilityResponse {
  room_id: number;
  is_available: boolean;
  check_in: string;
  check_out: string;
}

export type BookingStatus =
  | 'Created'
  | 'Pending'
  | 'AwaitingPayment'
  | 'Reserved'
  | 'Confirmed'
  | 'Cancelled'
  | 'Failed'
  | 'Expired';

export interface Booking {
  id: string;
  guestId: string;
  checkIn: string;
  checkOut: string;
  status: BookingStatus;
  totalPrice: string;
  roomId: number;
}

export interface PaymentIntentResponse {
  intentId: string;
  pspRedirectUrl: string;
}

export interface SearchParams {
  checkIn: string;
  checkOut: string;
  adults: number;
  children: number;
}
