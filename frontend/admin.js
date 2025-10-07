import { apiBase as rawApiBase, getToken } from './apiClient.js';

// Normalize apiBase to avoid accidental double /api/api when concatenating
const apiBase = rawApiBase.replace(/\/api\/api(\/|$)/,'/api$1');

// Toast notifications
function showToast(message, type='info', timeout=4000){
  let host = document.getElementById('toasts');
  if(!host){
    host = document.createElement('div');
    host.id = 'toasts';
    document.body.appendChild(host);
  }
  const div = document.createElement('div');
  div.className = 'toast '+type;
  div.innerHTML = `<span>${message}</span>`;
  const btn = document.createElement('button');
  btn.className='close-btn';
  btn.innerHTML='&times;';
  btn.addEventListener('click', () => removeToast(div));
  div.appendChild(btn);
  host.appendChild(div);
  if(timeout>0){
    setTimeout(()=> removeToast(div), timeout);
  }
  return div;
}

function removeToast(el){
  if(!el) return;
  el.classList.add('fading');
  el.addEventListener('animationend', () => el.remove());
}

function setStatus(el, msg, ok=false){
  el.textContent = msg;
  el.className = ok ? 'ok' : 'error';
  if(!msg) { el.className='muted'; }
}

function authHeader(){
  const t = getToken();
  return t ? { 'Authorization': 'Bearer ' + t } : {};
}

function decodeJwt(token){
  if(!token) return null;
  const parts = token.split('.');
  if(parts.length !== 3) return null;
  try {
    return JSON.parse(atob(parts[1]));
  } catch { return null; }
}

function ensureAdmin(){
  const div = document.getElementById('roleCheck');
  const token = getToken();
  if(!token){
    div.innerHTML = 'No token found. <a href="./login.html">Login</a>'; return false;
  }
  const payload = decodeJwt(token);
  if(!payload){ div.textContent = 'Invalid token payload'; return false; }
  const roles = payload['role'] || payload['roles'] || payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'];
  let roleList = Array.isArray(roles) ? roles : [roles];
  roleList = roleList.filter(r=>r);
  if(!roleList.includes('Admin')){
    div.innerHTML = 'You are not Admin. Roles: ' + roleList.join(', ') + ' <a href="./login.html">Switch account</a>';
    return false;
  }
  div.innerHTML = 'Logged as <span class="badge">Admin</span>'; div.className='ok';
  return true;
}

async function apiFetch(path, opts={}){
  const headers = { 'Content-Type': 'application/json', ...authHeader(), ...(opts.headers||{}) };
  const res = await fetch(apiBase + path, { ...opts, headers });
  let data = null;
  const text = await res.text();
  try { data = text ? JSON.parse(text) : null; } catch { data = text; }
  if(!res.ok){
    throw { status: res.status, statusText: res.statusText, data };
  }
  return data;
}

function encodeQuery(params){
  const q = Object.entries(params)
    .filter(([,v]) => v !== undefined && v !== null && v !== '')
    .map(([k,v]) => encodeURIComponent(k)+'='+encodeURIComponent(v))
    .join('&');
  return q ? ('?'+q) : '';
}

async function addHotel(e){
  e.preventDefault();
  const f = e.target;
  const msg = document.getElementById('hotelMsg');
  setStatus(msg,'');
  const body = {
    name: f.name.value.trim(),
    address: f.address.value.trim(),
    description: f.description.value.trim()
  };
  try {
  const data = await apiFetch('/Admin/AddHotel'+encodeQuery(body), { method:'POST' });
    const createdId = data?.hotelId || 'OK';
    setStatus(msg,'Hotel created id '+ createdId, true);
    showToast('Отель создан (ID '+createdId+')','success');
  } catch(err){
    const errMsg = 'Ошибка создания отеля: '+(err.data?.message || err.status);
    setStatus(msg, errMsg, false);
    showToast(errMsg,'error');
  }
}

async function updateHotel(e){
  e.preventDefault();
  const f = e.target;
  const msg = document.getElementById('hotelUpdateMsg');
  setStatus(msg,'');
  const hotelId = f.hotelId.value;
  const q = encodeQuery({ name:f.name.value.trim(), address:f.address.value.trim(), description:f.description.value.trim() });
  try {
  await apiFetch('/Admin/UpdateHotel/'+hotelId + q, { method:'POST' });
    setStatus(msg,'Updated', true);
    showToast('Отель обновлён ('+hotelId+')','success');
  } catch(err){ setStatus(msg,'Error '+(err.data?.message || err.status), false); }
}

async function removeHotel(e){
  e.preventDefault();
  const f = e.target;
  const msg = document.getElementById('hotelRemoveMsg');
  setStatus(msg,'');
  try {
  await apiFetch('/Admin/RemoveHotel/'+f.hotelId.value, { method:'DELETE' });
    setStatus(msg,'Removed', true);
    showToast('Отель удалён ('+f.hotelId.value+')','success');
  } catch(err){ setStatus(msg,'Error '+(err.data?.message || err.status), false); }
}

async function addRoom(e){
  e.preventDefault();
  const f = e.target; const msg = document.getElementById('roomMsg'); setStatus(msg,'');
  const q = encodeQuery({
    hotelId: f.hotelId.value,
    name: f.name.value.trim(),
    price: f.price.value,
    capacity: f.capacity.value,
    startDate: f.startDate.value,
    endDate: f.endDate.value
  });
  try { await apiFetch('/Admin/AddRoom'+q, { method:'POST' }); setStatus(msg,'Room added', true); }
  catch(err){
    const m = 'Ошибка добавления комнаты: '+(err.data?.message || err.status);
    setStatus(msg,m,false); showToast(m,'error');
  }
  if(msg.className==='ok') showToast('Комната добавлена','success');
}

async function updateRoom(e){
  e.preventDefault();
  const f = e.target; const msg = document.getElementById('roomUpdateMsg'); setStatus(msg,'');
  const q = encodeQuery({ name:f.name.value.trim(), price:f.price.value, capacity:f.capacity.value });
  try { await apiFetch('/Admin/UpdateRoom/'+f.roomId.value + q, { method:'POST' }); setStatus(msg,'Updated', true); }
  catch(err){
    const m='Ошибка обновления комнаты: '+(err.data?.message || err.status);
    setStatus(msg,m,false); showToast(m,'error'); return;
  }
  showToast('Комната обновлена ('+f.roomId.value+')','success');
}

async function removeRoom(e){
  e.preventDefault();
  const f = e.target; const msg = document.getElementById('roomRemoveMsg'); setStatus(msg,'');
  try { await apiFetch('/Admin/RemoveRoom/'+f.roomId.value, { method:'DELETE' }); setStatus(msg,'Removed', true); }
  catch(err){
    const m='Ошибка удаления комнаты: '+(err.data?.message || err.status);
    setStatus(msg,m,false); showToast(m,'error'); return;
  }
  showToast('Комната удалена ('+f.roomId.value+')','success');
}

async function loadBookings(){
  const out = document.getElementById('bookingsOutput');
  out.style.display='block'; out.textContent='Loading...';
  try { const data = await apiFetch('/Admin/bookings'); out.textContent = JSON.stringify(data,null,2); }
  catch(err){ out.textContent='Error '+(err.data?.message || err.status); }
}

async function loadStats(rangeDays=30){
  const statsDiv = document.getElementById('statsResult');
  statsDiv.textContent='Loading stats...';
  const end = new Date();
  const start = new Date(Date.now() - rangeDays*86400000);
  const q = encodeQuery({ startDate:start.toISOString(), endDate:end.toISOString() });
  try {
    const data = await apiFetch('/Admin/stats/bookings'+q);
    renderStats(data, statsDiv);
  }
  catch(err){ statsDiv.textContent='Error '+(err.data?.message || err.status); }
}

function renderStats(data, host){
  if(!data){ host.textContent='No stats'; return; }
  host.innerHTML='';
  const prettyMap = [
    { key:'totalBookings', label:'Bookings', accent:'#1f4d8f' },
    { key:'distinctRooms', label:'Rooms Used', accent:'#235fae' },
    { key:'distinctUsers', label:'Users', accent:'#2c7a3f' },
    { key:'totalRoomNights', label:'Room Nights', accent:'#8555c9' }
  ];
  const period = document.createElement('div');
  period.style.flexBasis='100%';
  period.style.fontSize='12px';
  period.style.color='#555';
  period.textContent = `Period: ${formatDate(data.startDate)} – ${formatDate(data.endDate)}`;
  host.appendChild(period);
  prettyMap.forEach(m => {
    const v = data[m.key];
    const card = document.createElement('div');
    card.style.background='#ffffff';
    card.style.border='1px solid #e3e9ef';
    card.style.borderRadius='8px';
    card.style.padding='10px 14px';
    card.style.minWidth='140px';
    card.style.flex='1 1 140px';
    card.style.display='flex';
    card.style.flexDirection='column';
    card.style.gap='4px';
    card.innerHTML = `<span style="font-size:11px;letter-spacing:.5px;text-transform:uppercase;color:#6a7785;">${m.label}</span>`+
      `<span style="font-size:22px;font-weight:600;color:${m.accent}">${v ?? 0}</span>`;
    host.appendChild(card);
  });
}

function formatDate(dt){
  try { const d = new Date(dt); return d.toISOString().substring(0,10); } catch { return dt; }
}

function decodeTokenBtn(){
  const out = document.getElementById('bookingsOutput');
  out.style.display='block';
  const payload = decodeJwt(getToken());
  out.textContent = 'JWT payload:\n'+JSON.stringify(payload,null,2);
}

function wire(){
  if(!ensureAdmin()) return;
  document.getElementById('formAddHotel').addEventListener('submit', addHotel);
  document.getElementById('formUpdateHotel').addEventListener('submit', updateHotel);
  document.getElementById('formRemoveHotel').addEventListener('submit', removeHotel);
  document.getElementById('formAddRoom').addEventListener('submit', addRoom);
  document.getElementById('formUpdateRoom').addEventListener('submit', updateRoom);
  document.getElementById('formRemoveRoom').addEventListener('submit', removeRoom);
  document.getElementById('btnLoadBookings').addEventListener('click', loadBookings);
  document.getElementById('btnStats').addEventListener('click', () => loadStats(30));
  document.getElementById('btnDecodeToken').addEventListener('click', decodeTokenBtn);
}

document.addEventListener('DOMContentLoaded', wire);
