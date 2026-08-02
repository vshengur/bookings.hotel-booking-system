import { BrowserRouter, Routes, Route, useNavigate } from 'react-router-dom';
import SearchPage       from './pages/SearchPage';
import RoomDetailsPage  from './pages/RoomDetailsPage';
import BookingFormPage  from './pages/BookingFormPage';
import PaymentPage      from './pages/PaymentPage';
import ConfirmationPage from './pages/ConfirmationPage';
import MyBookingsPage   from './pages/MyBookingsPage';
import ProfilePage      from './pages/ProfilePage';
import { SearchProvider } from './SearchContext';
import { AuthProvider, useAuth } from './AuthContext';

const GATEWAY = import.meta.env.VITE_GATEWAY_URL ?? 'http://localhost:8080';
const GOOGLE_AUTH_ENABLED = import.meta.env.VITE_GOOGLE_AUTH_ENABLED === 'true';

async function handleGoogleLogin() {
  try {
    const res = await fetch(`${GATEWAY}/api/auth/login`, { redirect: 'manual' });
    if (res.type === 'opaqueredirect' || res.status === 302) {
      globalThis.location.href = `${GATEWAY}/api/auth/login`;
      return;
    }
    const data = await res.json().catch(() => null);
    globalThis.location.href = data?.auth_url ?? `${GATEWAY}/api/auth/login`;
  } catch {
    globalThis.location.href = `${GATEWAY}/api/auth/login`;
  }
}

function Header() {
  const navigate = useNavigate();
  const { authed, user, logout, loginAsDemo } = useAuth();

  return (
    <header className="app-header">
      <button className="app-logo nav-btn-plain" onClick={() => navigate('/')}>
        Hotel Booking — PoC
      </button>
      <nav className="app-nav">
        {authed && (
          <>
            <button className="nav-btn" onClick={() => navigate('/my-bookings')}>My bookings</button>
            <button className="nav-btn user-chip" onClick={() => navigate('/profile')}>
              {user?.picture
                ? <img src={user.picture} alt={user.name ?? 'user'} className="user-avatar" />
                : <span className="user-avatar-placeholder">{(user?.name ?? user?.email ?? '?')[0].toUpperCase()}</span>
              }
              <span className="user-name">{user?.name ?? user?.email ?? 'User'}</span>
            </button>
            <button className="nav-btn nav-btn-outline" onClick={logout}>Sign out</button>
          </>
        )}
        {!authed && (
          <>
            {GOOGLE_AUTH_ENABLED && (
              <button className="nav-btn nav-btn-outline" onClick={() => void handleGoogleLogin()}>Sign in</button>
            )}
            <button className="nav-btn nav-btn-demo" onClick={loginAsDemo}>Demo mode</button>
          </>
        )}
      </nav>
    </header>
  );
}

export default function App() {
  return (
    <BrowserRouter>
      <AuthProvider>
        <SearchProvider>
          <Header />
          <main>
            <Routes>
              <Route path="/"                        element={<SearchPage />} />
              <Route path="/rooms/:id"               element={<RoomDetailsPage />} />
              <Route path="/booking"                 element={<BookingFormPage />} />
              <Route path="/payment/:bookingId"      element={<PaymentPage />} />
              <Route path="/confirmation/:bookingId" element={<ConfirmationPage />} />
              <Route path="/my-bookings"             element={<MyBookingsPage />} />
              <Route path="/profile"                 element={<ProfilePage />} />
            </Routes>
          </main>
        </SearchProvider>
      </AuthProvider>
    </BrowserRouter>
  );
}
