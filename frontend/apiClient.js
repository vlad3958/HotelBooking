// trigger pages deploy
// TEMP DEBUG: base URL determination for troubleshooting registration
/**
 * Simple API client for HotelBooking backend.
 * BASE_URL auto-detects depending on hosting:
 *  - Localhost (dev)
 *  - If running on GitHub Pages (hostname ends with github.io) -> use HEROKU_API_URL override you can inject via <script> before bundle OR fallback to hardcoded Heroku backend
 *  - Fallback: localhost
 */
const DEFAULT_LOCAL = 'http://localhost:5127/api';
// Corrected deployed API URL (Heroku backend):
const HEROKU_BACKEND = 'https://hotelbooking-api-dotnet-f4ed83e25d9f.herokuapp.com/api';
function resolveBaseUrl() {
  if (typeof window !== 'undefined' && window.HOTEL_API_BASE) {
    return window.HOTEL_API_BASE.replace(/\/$/, '');
  }
  if (typeof window !== 'undefined') {
    const host = window.location.host.toLowerCase();
    if (host.includes('localhost')) return DEFAULT_LOCAL;
    if (host.endsWith('github.io')) return HEROKU_BACKEND; // GitHub Pages domain
  }
  return DEFAULT_LOCAL;
}

const BASE_URL = resolveBaseUrl();
// TEMP DEBUG
if (typeof window !== 'undefined') { console.log('[HB] Using BASE_URL:', BASE_URL); }

// In-memory token store (replace with better storage if needed)
let authToken = null;

const TOKEN_KEY = 'hb_jwt';

function loadToken() {
  if (authToken) return authToken;
  try {
    const stored = localStorage.getItem(TOKEN_KEY);
    if (stored) authToken = stored;
  } catch {}
  return authToken;
}

export function setToken(token) {
  authToken = token;
  try { localStorage.setItem(TOKEN_KEY, token); } catch {}
}

export function getToken() { return loadToken(); }
export function logout() { authToken = null; try { localStorage.removeItem(TOKEN_KEY); } catch {} }

function getHeaders(isJson = true) {
  const headers = {};
  if (isJson) headers['Content-Type'] = 'application/json';
  const token = loadToken();
  if (token) headers['Authorization'] = `Bearer ${token}`;
  return headers;
}

async function handleResponse(resp) {
  if (!resp.ok) {
    let bodyText = '';
    try { bodyText = await resp.text(); } catch {}
    let parsed;
    try { parsed = bodyText ? JSON.parse(bodyText) : null; } catch { parsed = { raw: bodyText }; }
    const error = new Error(`HTTP ${resp.status}`);
    error.status = resp.status;
    error.body = parsed;
    throw error;
  }
  const ct = resp.headers.get('content-type');
  if (ct && ct.includes('application/json')) return resp.json();
  return resp.text();
}

// AUTH -------------------------------------------------
export async function login(email, password) {
  const resp = await fetch(`${BASE_URL}/Auth/login`, {
    method: 'POST',
    headers: getHeaders(),
    body: JSON.stringify({ email, password })
  });
  const data = await handleResponse(resp);
  // Accept either Token (backend) or token (fallback)
  const rawToken = data?.token || data?.Token;
  if (rawToken) setToken(rawToken);
  return data;
}

export async function register(email, password) {
  const url = `${BASE_URL}/Auth/register`;
  console.log('[HB] Register call ->', url, { email }); // TEMP DEBUG
  const resp = await fetch(url, {
    method: 'POST',
    headers: getHeaders(),
    body: JSON.stringify({ email, password })
  });
  return handleResponse(resp);
}

// HOTELS / ROOMS ---------------------------------------
export async function getHotels() {
  const resp = await fetch(`${BASE_URL}/Client/hotels`, { headers: getHeaders(false) });
  return handleResponse(resp);
}

export async function getRooms() {
  const resp = await fetch(`${BASE_URL}/Client/rooms`, { headers: getHeaders(false) });
  return handleResponse(resp);
}

export async function getRoomsByDateRange(startDate, endDate) {
  const url = new URL(`${BASE_URL}/Client/rooms/daterange`);
  url.searchParams.set('startDate', startDate);
  url.searchParams.set('endDate', endDate);
  const resp = await fetch(url, { headers: getHeaders(false) });
  return handleResponse(resp);
}

export async function getRoomByCity(city) {
  const resp = await fetch(`${BASE_URL}/Client/rooms/city/${encodeURIComponent(city)}`, { headers: getHeaders(false) });
  return handleResponse(resp);
}

// BOOKINGS ---------------------------------------------
export async function bookRoom(roomId, startDate, endDate) {
  const resp = await fetch(`${BASE_URL}/Client/book`, {
    method: 'POST',
    headers: getHeaders(),
    body: JSON.stringify({ roomId, startDate, endDate })
  });
  return handleResponse(resp);
}

export async function myBookings() {
  const resp = await fetch(`${BASE_URL}/Client/bookings/me`, { headers: getHeaders(false) });
  return handleResponse(resp);
}

// ADMIN ------------------------------------------------
export async function getAllBookingsAdmin() {
  const resp = await fetch(`${BASE_URL}/Admin/bookings`, { headers: getHeaders(false) });
  return handleResponse(resp);
}

export async function getBookingStats(startDate, endDate) {
  const url = new URL(`${BASE_URL}/Admin/stats/bookings`);
  url.searchParams.set('startDate', startDate);
  url.searchParams.set('endDate', endDate);
  const resp = await fetch(url, { headers: getHeaders(false) });
  return handleResponse(resp);
}

export async function addHotel(name, address, description) {
  const url = new URL(`${BASE_URL}/Admin/AddHotel`);
  url.searchParams.set('name', name);
  url.searchParams.set('address', address);
  url.searchParams.set('description', description);
  const resp = await fetch(url, { method: 'POST', headers: getHeaders(false) });
  return handleResponse(resp);
}

export async function addRoom(hotelId, name, price, capacity, startDate = null, endDate = null) {
  const url = new URL(`${BASE_URL}/Admin/AddRoom`);
  url.searchParams.set('hotelId', hotelId);
  url.searchParams.set('name', name);
  url.searchParams.set('price', price);
  url.searchParams.set('capacity', capacity);
  if (startDate) url.searchParams.set('startDate', startDate);
  if (endDate) url.searchParams.set('endDate', endDate);
  const resp = await fetch(url, { method: 'POST', headers: getHeaders(false) });
  return handleResponse(resp);
}

// Utility ------------------------------------------------
export function setBaseUrl(newBase) { /* runtime override */ if (newBase) { /* no-op placeholder for future dynamic switching */ } }

// Example usage (uncomment to test in browser environment)
// login('admin@example.com', 'Admin123!').then(() => getHotels()).then(console.log).catch(console.error);
