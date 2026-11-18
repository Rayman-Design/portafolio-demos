<?php
/**
 * Punto de entrada público de la demo de Rayo.
 * GET  -> información pública (modo y preguntas sugeridas).
 * POST -> {"message": "..."} y devuelve {"ok", "mode", "answer"}.
 *
 * EDITA SOLO ESTA LÍNEA: ruta a la carpeta app/ (fuera de public_html).
 */
$APP_DIR = getenv('RAYO_APP_DIR') ?: dirname(__DIR__, 2) . '/app';

header('Content-Type: application/json; charset=utf-8');
header('X-Content-Type-Options: nosniff');
header('Cache-Control: no-store');
header('Referrer-Policy: no-referrer');

$configFile = $APP_DIR . '/config.php';
if (!is_readable($configFile)) {
    $configFile = $APP_DIR . '/config.example.php'; // sin config.php se queda en modo simulado
}
$config = require $configFile;
require_once $APP_DIR . '/lib/RayoChat.php';
$faq = json_decode((string)file_get_contents($APP_DIR . '/data/faq.json'), true) ?: [];

// CORS solo para los dominios permitidos.
$origin = $_SERVER['HTTP_ORIGIN'] ?? '';
$allowed = $config['allowed_origins'] ?? [];
if ($origin !== '' && in_array($origin, $allowed, true)) {
    header('Access-Control-Allow-Origin: ' . $origin);
    header('Vary: Origin');
    header('Access-Control-Allow-Methods: GET, POST, OPTIONS');
    header('Access-Control-Allow-Headers: Content-Type');
}
$method = $_SERVER['REQUEST_METHOD'] ?? 'GET';
if ($method === 'OPTIONS') {
    http_response_code(204);
    exit;
}

$chat = new RayoChat($config, $faq);

if ($method === 'GET') {
    echo json_encode(['ok' => true] + $chat->publicInfo(), JSON_UNESCAPED_UNICODE);
    exit;
}
if ($method !== 'POST') {
    http_response_code(405);
    echo json_encode(['ok' => false, 'answer' => 'Método no permitido.']);
    exit;
}
if ($origin !== '' && $allowed && !in_array($origin, $allowed, true)) {
    http_response_code(403);
    echo json_encode(['ok' => false, 'answer' => 'Origen no permitido.']);
    exit;
}

$raw = file_get_contents('php://input', false, null, 0, 4096);
$data = json_decode((string)$raw, true);
$message = is_array($data) ? ($data['message'] ?? null) : null;
$ip = $_SERVER['REMOTE_ADDR'] ?? '0.0.0.0';

[$status, $body] = $chat->handle($message, $ip);
http_response_code($status);
echo json_encode($body, JSON_UNESCAPED_UNICODE);
