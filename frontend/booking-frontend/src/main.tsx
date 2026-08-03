import ReactDOM from 'react-dom/client'
import App from './App.tsx'
import './index.css'
import { tryConsumeTokenFromUrl } from './auth.ts'

// Run synchronously before React renders so isAuthenticated() is correct on first paint
tryConsumeTokenFromUrl()

ReactDOM.createRoot(document.getElementById('root')!).render(<App />)
