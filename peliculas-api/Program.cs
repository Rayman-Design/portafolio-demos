// =====================================================================
// Program.cs: el punto de arranque de la API.
// Aquí armamos la aplicación en dos partes:
//   1. Registramos los servicios (lo que la app necesita para funcionar).
//   2. Configuramos cómo se atienden las peticiones y la ponemos a correr.
// =====================================================================

using PeliculasApi.Repositorios;

// Aquí creamos el "constructor" de la app. Lee la configuración (appsettings.json)
// y prepara todo lo básico para que podamos registrar nuestras piezas.
var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------------------
// 1. SERVICIOS
// ---------------------------------------------------------------------

// Aquí activamos los controladores, que son las clases donde viven los endpoints.
// Sin esta línea, .NET no buscaría nuestro PeliculasController.
builder.Services.AddControllers();

// Aquí registramos el repositorio de películas en el contenedor de dependencias.
// "AddSingleton" quiere decir que se crea UNA sola instancia para toda la app,
// y eso es justo lo que queremos: si se creara una nueva por cada petición,
// la lista en memoria se reiniciaría y perderíamos las películas que agregamos.
// Le decimos: "cuando alguien pida IPeliculaRepositorio, entrégale PeliculaRepositorioEnMemoria".
builder.Services.AddSingleton<IPeliculaRepositorio, PeliculaRepositorioEnMemoria>();

// Aquí construimos la app ya con todos los servicios registrados.
var app = builder.Build();

// ---------------------------------------------------------------------
// 2. CÓMO SE ATIENDEN LAS PETICIONES
// ---------------------------------------------------------------------

// Un endpoint de bienvenida en la raíz ("/") para comprobar rápido que la API está viva.
// Abres http://localhost:5080 en el navegador y te responde este mensaje.
app.MapGet("/", () => Results.Ok(new
{
    mensaje = "API de películas de Rayman Design funcionando. Prueba GET /api/peliculas",
    nota = "Proyecto demostrativo con datos ficticios."
}));

// Aquí conectamos las rutas de los controladores (por ejemplo /api/peliculas)
// para que cada petición llegue al método correcto.
app.MapControllers();

// Y aquí arrancamos el servidor. Desde este punto la API se queda escuchando peticiones.
app.Run();

// Esta línea vacía hace que la clase Program sea pública.
// Hoy no la necesitamos, pero sirve si más adelante agregas pruebas automáticas.
public partial class Program { }
