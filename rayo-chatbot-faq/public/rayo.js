/* Interfaz de la demo de Rayo. No contiene llaves ni secretos: todo pasa por api/chat.php. */
(function () {
  'use strict';
  var API = 'api/chat.php';
  var log = document.getElementById('rayo-log');
  var form = document.getElementById('rayo-form');
  var input = document.getElementById('rayo-input');
  var send = document.getElementById('rayo-send');
  var chips = document.getElementById('rayo-chips');
  var badge = document.getElementById('rayo-mode');
  var busy = false;

  function add(text, cls) {
    var div = document.createElement('div');
    div.className = 'msg ' + cls;
    div.textContent = text; // siempre texto plano, nunca HTML
    log.appendChild(div);
    log.scrollTop = log.scrollHeight;
    return div;
  }

  function setMode(mode) {
    if (mode === 'live') {
      badge.textContent = 'IA en vivo · Gemini';
      badge.className = 'rayo-badge live';
    } else {
      badge.textContent = 'Modo demo · FAQ';
      badge.className = 'rayo-badge';
      badge.title = 'Respuestas tomadas de la FAQ pública, sin IA en vivo.';
    }
  }

  function setBusy(b) {
    busy = b;
    send.disabled = b;
    Array.prototype.forEach.call(chips.querySelectorAll('button'), function (x) { x.disabled = b; });
  }

  function ask(text) {
    text = (text || '').trim();
    if (!text || busy) return;
    add(text, 'user');
    input.value = '';
    setBusy(true);
    var typing = add('Rayo está escribiendo…', 'bot typing');
    fetch(API, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ message: text })
    })
      .then(function (r) { return r.json().catch(function () { return { ok: false, answer: 'Respuesta no válida del servidor.' }; }); })
      .then(function (data) {
        typing.remove();
        if (data.mode) setMode(data.mode);
        add(data.answer || 'No hubo respuesta.', data.ok ? 'bot' : 'bot error');
      })
      .catch(function () {
        typing.remove();
        add('No me pude conectar. Revisa tu internet e intenta otra vez.', 'bot error');
      })
      .then(function () { setBusy(false); input.focus(); });
  }

  form.addEventListener('submit', function (e) { e.preventDefault(); ask(input.value); });

  fetch(API, { method: 'GET' })
    .then(function (r) { return r.json(); })
    .then(function (info) {
      setMode(info.mode);
      if (info.max_input_chars) input.maxLength = info.max_input_chars;
      (info.suggested || []).forEach(function (q) {
        var b = document.createElement('button');
        b.type = 'button';
        b.textContent = q;
        b.addEventListener('click', function () { ask(q); });
        chips.appendChild(b);
      });
    })
    .catch(function () { setMode('mock'); });
})();
