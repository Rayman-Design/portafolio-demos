#!/usr/bin/env python3
"""Explorador de prueba de PeliculasApi (solo para demostración local).

Sirve una pagina de prueba en /explorador y reenvia todo lo demas, sin cambios,
a la API .NET que corre en 127.0.0.1:5080. No toca el codigo de la API.
"""
import http.client
import os
from http.server import BaseHTTPRequestHandler, ThreadingHTTPServer

API_HOST, API_PORT = "127.0.0.1", 5080
HERE = os.path.dirname(os.path.abspath(__file__))
PAGE = os.path.join(HERE, "explorador.html")
POSTERS = os.path.join(HERE, "posters")
MAX_BODY = 8 * 1024


class Handler(BaseHTTPRequestHandler):
    server_version = "PeliculasPreview"
    sys_version = ""

    def log_message(self, fmt, *args):
        pass

    def _send(self, code, body=b"", ctype="text/plain; charset=utf-8", extra=None):
        self.send_response(code)
        self.send_header("Content-Type", ctype)
        self.send_header("Content-Length", str(len(body)))
        self.send_header("X-Content-Type-Options", "nosniff")
        self.send_header("Cache-Control", "no-store")
        for k, v in (extra or {}).items():
            self.send_header(k, v)
        self.end_headers()
        if self.command != "HEAD":
            self.wfile.write(body)

    def _page(self):
        with open(PAGE, "rb") as f:
            self._send(200, f.read(), "text/html; charset=utf-8")

    def _poster(self, name):
        # Solo sirve archivos .jpg de la carpeta posters/, sin subcarpetas.
        if "/" in name or not name.endswith(".jpg") or name.startswith("."):
            return self._send(404)
        path = os.path.join(POSTERS, name)
        if not os.path.isfile(path):
            return self._send(404)
        with open(path, "rb") as f:
            self._send(200, f.read(), "image/jpeg")

    def _forward(self):
        length = int(self.headers.get("Content-Length") or 0)
        if length > MAX_BODY:
            return self._send(413, b'{"mensaje":"Cuerpo demasiado grande para la demo."}', "application/json")
        body = self.rfile.read(length) if length else None
        headers = {"Accept": "application/json"}
        if self.headers.get("Content-Type"):
            headers["Content-Type"] = self.headers["Content-Type"]
        try:
            conn = http.client.HTTPConnection(API_HOST, API_PORT, timeout=15)
            conn.request(self.command, self.path, body=body, headers=headers)
            resp = conn.getresponse()
            data = resp.read()
        except OSError:
            return self._send(502, b'{"mensaje":"La API no responde en este momento."}', "application/json")
        extra = {}
        loc = resp.getheader("Location")
        if loc:
            # La API arma la URL con su host interno; dejamos solo la ruta.
            if "://" in loc:
                loc = "/" + loc.split("://", 1)[1].split("/", 1)[-1]
            extra["Location"] = loc
        ctype = resp.getheader("Content-Type") or "application/json"
        self._send(resp.status, data, ctype, extra)

    def _route(self):
        path = self.path.split("?", 1)[0]
        if path in ("/explorador", "/explorador/") and self.command in ("GET", "HEAD"):
            return self._page()
        if path.startswith("/posters/") and self.command in ("GET", "HEAD"):
            return self._poster(path[len("/posters/"):])
        if path == "/favicon.ico":
            return self._send(204)
        return self._forward()

    do_GET = do_HEAD = do_POST = do_PUT = do_DELETE = _route


if __name__ == "__main__":
    ThreadingHTTPServer(("127.0.0.1", 8080), Handler).serve_forever()
