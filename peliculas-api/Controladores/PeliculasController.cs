// =====================================================================
// PeliculasController.cs: el CONTROLADOR.
// Aquí viven los endpoints, o sea, las "puertas" de la API.
// Cada método recibe una petición HTTP, le pide al repositorio lo que haga falta
// y devuelve una respuesta con el código HTTP correcto.
//
// Si vienes de Java, esto es muy parecido a un @RestController de Spring Boot.
// =====================================================================

using Microsoft.AspNetCore.Mvc;
using PeliculasApi.Dtos;
using PeliculasApi.Modelos;
using PeliculasApi.Repositorios;

namespace PeliculasApi.Controladores;

// [ApiController] nos regala varias cosas automáticas. La más importante:
// si el JSON que llega no cumple las reglas del DTO, .NET responde 400 solito
// con la lista de errores, sin que tengamos que escribir esa validación a mano.
[ApiController]

// [Route] define la ruta base de todos los endpoints de esta clase.
// Todo lo de aquí empieza con /api/peliculas.
[Route("api/peliculas")]

// Aquí le decimos que siempre responde en JSON.
[Produces("application/json")]
public class PeliculasController : ControllerBase
{
    // Aquí guardamos el repositorio que nos entrega .NET.
    private readonly IPeliculaRepositorio _repositorio;

    // El constructor recibe el repositorio por "inyección de dependencias".
    // Nosotros no hacemos "new PeliculaRepositorioEnMemoria()": .NET nos lo pasa
    // porque lo registramos en Program.cs. Así el controlador solo conoce la interfaz.
    public PeliculasController(IPeliculaRepositorio repositorio)
    {
        _repositorio = repositorio;
    }

    // -----------------------------------------------------------------
    // GET /api/peliculas            -> lista todas
    // GET /api/peliculas?genero=Comedia -> lista solo las de ese género
    // -----------------------------------------------------------------
    // [HttpGet] sin nada más quiere decir que responde en la ruta base.
    // [FromQuery] toma el valor de la URL, de lo que va después del "?".
    [HttpGet]
    public ActionResult<IEnumerable<Pelicula>> ObtenerTodas([FromQuery] string? genero)
    {
        var peliculas = _repositorio.ObtenerTodas(genero);

        // Ok(...) responde 200 con la lista en JSON.
        // Si no hay ninguna, igual es 200 con una lista vacía []: no es un error.
        return Ok(peliculas);
    }

    // -----------------------------------------------------------------
    // GET /api/peliculas/3 -> trae la película con Id 3
    // -----------------------------------------------------------------
    // "{id:int}" toma el número de la URL y lo pasa al parámetro id.
    // El ":int" hace que solo acepte números; /api/peliculas/hola no entra aquí.
    // Name = "ObtenerPeliculaPorId" le pone un nombre a la ruta para usarlo en Crear.
    [HttpGet("{id:int}", Name = "ObtenerPeliculaPorId")]
    public ActionResult<Pelicula> ObtenerPorId(int id)
    {
        var pelicula = _repositorio.ObtenerPorId(id);

        // Si no existe, respondemos 404 (No encontrado) con un mensaje claro.
        if (pelicula is null)
        {
            return NotFound(new { mensaje = $"No existe una película con id {id}." });
        }

        // Si existe, 200 con la película.
        return Ok(pelicula);
    }

    // -----------------------------------------------------------------
    // POST /api/peliculas -> crea una película nueva
    // -----------------------------------------------------------------
    // [FromBody] quiere decir que los datos vienen en el cuerpo de la petición, en JSON.
    // Para cuando llegamos a la primera línea, [ApiController] ya validó el DTO.
    // Si algo estaba mal, ni siquiera entramos aquí: ya respondió 400.
    [HttpPost]
    public ActionResult<Pelicula> Crear([FromBody] PeliculaEntradaDto entrada)
    {
        // Pasamos los datos del DTO a un modelo Pelicula.
        var nueva = _repositorio.Crear(ConvertirAModelo(entrada));

        // CreatedAtRoute responde 201 (Creado). Además:
        //  - agrega el encabezado Location con la URL de la nueva película (/api/peliculas/5),
        //  - y devuelve la película creada, ya con su Id.
        return CreatedAtRoute("ObtenerPeliculaPorId", new { id = nueva.Id }, nueva);
    }

    // -----------------------------------------------------------------
    // PUT /api/peliculas/3 -> reemplaza todos los datos de la película 3
    // -----------------------------------------------------------------
    // Combina las dos cosas: el id viene en la URL y los datos nuevos en el cuerpo.
    [HttpPut("{id:int}")]
    public ActionResult<Pelicula> Actualizar(int id, [FromBody] PeliculaEntradaDto entrada)
    {
        var actualizada = _repositorio.Actualizar(id, ConvertirAModelo(entrada));

        // Si el repositorio devolvió null, es que esa película no existe: 404.
        if (actualizada is null)
        {
            return NotFound(new { mensaje = $"No existe una película con id {id}." });
        }

        // Si salió bien, 200 con la película ya cambiada, para que se vea el resultado.
        return Ok(actualizada);
    }

    // -----------------------------------------------------------------
    // DELETE /api/peliculas/3 -> borra la película 3
    // -----------------------------------------------------------------
    [HttpDelete("{id:int}")]
    public IActionResult Eliminar(int id)
    {
        // Si no se pudo borrar es porque no existía: 404.
        if (!_repositorio.Eliminar(id))
        {
            return NotFound(new { mensaje = $"No existe una película con id {id}." });
        }

        // NoContent responde 204: "listo, se borró, y no hay nada más que mostrar".
        return NoContent();
    }

    // Un ayudante privado para no repetir el mismo código en Crear y en Actualizar.
    // Toma lo que mandó el cliente (DTO) y lo pasa a nuestro modelo.
    // Trim() quita espacios sobrantes al inicio y al final de los textos.
    private static Pelicula ConvertirAModelo(PeliculaEntradaDto entrada) => new()
    {
        Titulo = entrada.Titulo.Trim(),
        Director = entrada.Director.Trim(),
        Anio = entrada.Anio,
        Genero = entrada.Genero.Trim(),
        DuracionMinutos = entrada.DuracionMinutos,
        Calificacion = entrada.Calificacion
    };
}
