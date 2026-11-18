<?php
// Servidor FALSO que imita la respuesta de la Interactions API. Solo para pruebas locales.
// php -S 127.0.0.1:8099 tests/fake_gemini.php
$key = $_SERVER['HTTP_X_GOOG_API_KEY'] ?? '';
header('Content-Type: application/json');
if ($key === '') { http_response_code(401); echo '{"error":{"code":401}}'; exit; }
$req = json_decode(file_get_contents('php://input'), true) ?: [];
if (strpos($req['input'] ?? '', 'cupo') !== false) { http_response_code(429); echo '{"error":{"code":429,"status":"RESOURCE_EXHAUSTED"}}'; exit; }
echo json_encode(['status' => 'completed', 'steps' => [['type' => 'model_output', 'content' => [['type' => 'text', 'text' => '[respuesta simulada del servidor falso] Modelo: ' . ($req['model'] ?? '?')]]]]]);
