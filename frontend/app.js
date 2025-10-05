// Shared utility & auth helpers for HotelBooking frontend
import { getToken, logout } from './apiClient.js';

export function requireAuth(redirect = 'login.html') {
  if (!getToken()) {
    window.location.replace(redirect);
    return false;
  }
  return true;
}

export function fmtDateInput(date) {
  // Returns yyyy-MM-dd from Date or string
  const d = (date instanceof Date) ? date : new Date(date);
  if (isNaN(d.getTime())) return '';
  return d.toISOString().slice(0,10);
}

export function fmtDateTimeHuman(date) {
  const d = new Date(date);
  if (isNaN(d.getTime())) return '';
  return d.toLocaleString();
}

export function attachLogout(buttonId = 'logoutBtn') {
  const btn = document.getElementById(buttonId);
  if (!btn) return;
  btn.addEventListener('click', (e) => {
    e.preventDefault();
    logout();
    window.location.href = 'login.html';
  });
}

export function showAlert(container, message, type = 'danger') {
  container.innerHTML = `<div class="alert alert-${type} py-2 mb-2">${message}</div>`;
}
