// =====================================================================
// IPeliculaRepositorio.cs: la INTERFAZ del repositorio.
// Aquí escribimos el "contrato": la lista de cosas que se pueden hacer con
// las películas, pero sin decir CÓMO se hacen.
//
// ¿Para qué sirve separar el contrato de la implementación?
// Para que el controlador no dependa de dónde se guardan los datos.
// Hoy los guardamos en memoria; mañana puedes crear otra clase que use
// SQL Server o PostgreSQL, y el controlador no cambia ni una línea.
// =====================================================================

using PeliculasApi.Modelos;

namespace PeliculasApi.Repositorios;

public interface IPeliculaRepositorio
{
    // Devuelve todas las películas. Si mandan un género, filtra por ese género.
    IReadOnlyList<Pelicula> ObtenerTodas(string? genero = null);

    // Busca una película por su Id. Devuelve null si no existe.
    Pelicula? ObtenerPorId(int id);

    // Guarda una película nueva, le asigna un Id y la devuelve ya con ese Id.
    Pelicula Crear(Pelicula pelicula);

    // Reemplaza los datos de una película que ya existe.
    // Devuelve la película actualizada, o null si ese Id no existe.
    Pelicula? Actualizar(int id, Pelicula datosNuevos);

    // Borra una película. Devuelve true si la encontró y la borró, false si no existía.
    bool Eliminar(int id);
}
