import { BrowserRouter, Routes, Route } from 'react-router-dom';
import SearchPage       from './pages/SearchPage';
import RoomDetailsPage  from './pages/RoomDetailsPage';
import BookingFormPage  from './pages/BookingFormPage';
import PaymentPage      from './pages/PaymentPage';
import ConfirmationPage from './pages/ConfirmationPage';

export default function App() {
  return (
    <BrowserRouter>
      <header className="app-header">
        <span onClick={() => location.assign('/')} style={{ cursor: 'pointer' }}>
          Hotel Booking — PoC
        </span>
      </header>
      <main>
        <Routes>
          <Route path="/"                        element={<SearchPage />} />
          <Route path="/rooms/:id"               element={<RoomDetailsPage />} />
          <Route path="/booking"                 element={<BookingFormPage />} />
          <Route path="/payment/:bookingId"      element={<PaymentPage />} />
          <Route path="/confirmation/:bookingId" element={<ConfirmationPage />} />
        </Routes>
      </main>
    </BrowserRouter>
  );
}
