<?php
/**
 * Configuración de la demo de Rayo para IA Labs.
 *
 * 1. Copia este archivo como config.php en la MISMA carpeta (app/).
 * 2. La carpeta app/ debe quedar FUERA de public_html para que nadie la pueda abrir desde el navegador.
 * 3. La llave de Gemini NUNCA se escribe aquí. Va en una variable de entorno (GEMINI_API_KEY)
 *    o en un archivo aparte, fuera de public_html, con permisos 600.
 */
return [
    // 'mock' = respuestas desde la FAQ, sin IA y sin llamadas a Google (valor por defecto, cero costo).
    // 'live' = usa Gemini por la API de Google AI Studio. Solo se activa si también se cumple lo de abajo.
    'mode' => 'mock',

    // Ponlo en true SOLO después de comprobar en https://aistudio.google.com (Projects / Usage)
    // que el proyecto de la llave aparece en el plan gratuito (Free) y sin cuenta de facturación vinculada.
    // Si es false, la demo se queda en modo simulado aunque 'mode' diga 'live'.
    'free_tier_verified' => false,

    // Fecha en la que lo comprobaste (texto libre). Obligatoria para activar el modo en vivo.
    'free_tier_verified_on' => '',

    // Ruta a un archivo que contiene SOLO la llave (una línea). Fuera de public_html.
    // Si existe la variable de entorno GEMINI_API_KEY, se usa esa primero.
    'api_key_file' => '/home/USUARIO/secrets/gemini_api_key.txt',

    // Modelo. gemini-3.5-flash-lite aparece como "Free of charge" en el plan gratuito
    // de https://ai.google.dev/gemini-api/docs/pricing (revisado el 28/10/2026). Vuelve a revisarlo si lo cambias.
    'model' => 'gemini-3.5-flash-lite',

    // Endpoint de la Interactions API de Gemini.
    'endpoint' => 'https://generativelanguage.googleapis.com/v1beta/interactions',

    // Límites propios de la demo (además de los de Google). Pónlos por DEBAJO de lo que AI Studio
    // muestra para tu proyecto en la página de límites (RPM / RPD).
    'limits' => [
        'max_input_chars'      => 400,   // largo máximo de la pregunta
        'max_output_tokens'    => 350,   // largo máximo de la respuesta
        'per_ip_per_minute'    => 4,
        'per_ip_per_day'       => 25,
        'global_per_minute'    => 8,
        'global_per_day'       => 200,   // presupuesto diario total de la demo
        'timeout_seconds'      => 20,
    ],

    // Carpeta donde se guardan los contadores (fuera de public_html, con permiso de escritura).
    'storage_dir' => __DIR__ . '/storage',

    // Texto al azar para anonimizar las IP en los contadores. Cámbialo por cualquier frase larga.
    'ip_salt' => 'cambia-esta-frase-por-una-propia',

    // Dominios que pueden llamar a la API (CORS). Déjalo vacío si la demo y la API están en el mismo dominio.
    'allowed_origins' => ['https://www.raymandesign.com', 'https://raymandesign.com'],
];
