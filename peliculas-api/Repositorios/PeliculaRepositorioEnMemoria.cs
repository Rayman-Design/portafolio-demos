// =====================================================================
// PeliculaRepositorioEnMemoria.cs: la IMPLEMENTACIÓN del repositorio.
// Aquí sí decimos CÓMO se guardan las películas: en una lista en memoria.
//
// Ojo: como todo vive en memoria, los cambios se pierden cuando apagas la API.
// Para una demo está perfecto; para un proyecto real usarías una base de datos.
// =====================================================================

using PeliculasApi.Modelos;

namespace PeliculasApi.Repositorios;

// ": IPeliculaRepositorio" quiere decir que esta clase cumple el contrato de la interfaz.
// En Java sería "implements IPeliculaRepositorio".
public class PeliculaRepositorioEnMemoria : IPeliculaRepositorio
{
    // Aquí guardamos las películas. Es "readonly" porque la lista en sí no se reemplaza,
    // solo le agregamos, cambiamos o quitamos elementos.
    private readonly List<Pelicula> _peliculas;

    // El siguiente Id que vamos a asignar. Va subiendo de uno en uno.
    private int _siguienteId;

    // Este objeto sirve de "candado". Como la API puede recibir varias peticiones
    // al mismo tiempo, usamos lock(_candado) para que dos peticiones no modifiquen
    // la lista a la vez y la dejen en mal estado.
    private readonly object _candado = new();

    // El constructor: se ejecuta una sola vez, cuando la app crea el repositorio.
    // Aquí cargamos unas películas de ejemplo para que la API no arranque vacía.
    // Todas son INVENTADAS: títulos, directores y datos ficticios, sin licencias de por medio.
    public PeliculaRepositorioEnMemoria()
    {
        _peliculas = new List<Pelicula>
        {
            new() { Id = 1, Titulo = "El último píxel", Director = "Lucía Andrade", Anio = 2019, Genero = "Ciencia ficción", DuracionMinutos = 112, Calificacion = 8.1m },
            new() { Id = 2, Titulo = "Café para tres", Director = "Martín Ocampo", Anio = 2021, Genero = "Comedia", DuracionMinutos = 95, Calificacion = 7.4m },
            new() { Id = 3, Titulo = "La ruta del colibrí", Director = "Sofía Herrera", Anio = 2017, Genero = "Aventura", DuracionMinutos = 128, Calificacion = 8.6m },
            new() { Id = 4, Titulo = "Código en la niebla", Director = "Andrés Quintero", Anio = 2023, Genero = "Suspenso", DuracionMinutos = 104, Calificacion = null }
        };

        // El próximo Id será el 5, porque ya usamos del 1 al 4.
        _siguienteId = _peliculas.Max(p => p.Id) + 1;
    }

    public IReadOnlyList<Pelicula> ObtenerTodas(string? genero = null)
    {
        lock (_candado)
        {
            // Arrancamos con todas las películas.
            IEnumerable<Pelicula> resultado = _peliculas;

            // Si nos mandaron un género, filtramos. Comparamos sin importar mayúsculas,
            // para que "comedia" y "Comedia" den lo mismo.
            if (!string.IsNullOrWhiteSpace(genero))
            {
                resultado = resultado.Where(p =>
                    p.Genero.Equals(genero.Trim(), StringComparison.OrdinalIgnoreCase));
            }

            // Devolvemos una COPIA ordenada por Id. Así nadie de afuera puede
            // modificar nuestra lista interna sin pasar por el repositorio.
            return resultado.OrderBy(p => p.Id).Select(Copiar).ToList();
        }
    }

    public Pelicula? ObtenerPorId(int id)
    {
        lock (_candado)
        {
            // FirstOrDefault busca la primera que cumpla la condición.
            // Si no encuentra ninguna, devuelve null.
            var pelicula = _peliculas.FirstOrDefault(p => p.Id == id);

            // Si la encontramos, devolvemos una copia; si no, null.
            return pelicula is null ? null : Copiar(pelicula);
        }
    }

    public Pelicula Crear(Pelicula pelicula)
    {
        lock (_candado)
        {
            // Aquí le ponemos el Id nosotros y luego subimos el contador,
            // para que la siguiente película reciba un número nuevo.
            var nueva = Copiar(pelicula);
            nueva.Id = _siguienteId++;

            _peliculas.Add(nueva);
            return Copiar(nueva);
        }
    }

    public Pelicula? Actualizar(int id, Pelicula datosNuevos)
    {
        lock (_candado)
        {
            // Primero buscamos la película. Si no existe, avisamos con null
            // y el controlador se encarga de responder 404.
            var existente = _peliculas.FirstOrDefault(p => p.Id == id);
            if (existente is null)
            {
                return null;
            }

            // Reemplazamos todos los datos (por eso usamos PUT y no PATCH).
            // El Id NO se toca: la película sigue siendo la misma.
            existente.Titulo = datosNuevos.Titulo;
            existente.Director = datosNuevos.Director;
            existente.Anio = datosNuevos.Anio;
            existente.Genero = datosNuevos.Genero;
            existente.DuracionMinutos = datosNuevos.DuracionMinutos;
            existente.Calificacion = datosNuevos.Calificacion;

            return Copiar(existente);
        }
    }

    public bool Eliminar(int id)
    {
        lock (_candado)
        {
            // RemoveAll borra todas las que cumplan la condición y dice cuántas borró.
            // Como el Id es único, borrará 1 (si existía) o 0 (si no existía).
            return _peliculas.RemoveAll(p => p.Id == id) > 0;
        }
    }

    // Un ayudante privado que hace una copia de una película.
    // Lo usamos para que los objetos que salen del repositorio no estén "conectados"
    // a la lista interna. Si alguien cambia la copia, nuestra lista no se altera.
    private static Pelicula Copiar(Pelicula p) => new()
    {
        Id = p.Id,
        Titulo = p.Titulo,
        Director = p.Director,
        Anio = p.Anio,
        Genero = p.Genero,
        DuracionMinutos = p.DuracionMinutos,
        Calificacion = p.Calificacion
    };
}
