// =====================================================================
// Pelicula.cs: el MODELO.
// Aquí definimos cómo es una película dentro de nuestra API.
// Es la "ficha" que guardamos y que devolvemos en las respuestas.
// =====================================================================

namespace PeliculasApi.Modelos;

// Una clase sencilla con propiedades. En Java sería como un POJO con getters y setters,
// pero en C# el { get; set; } ya nos da las dos cosas en una sola línea.
public class Pelicula
{
    // El identificador único. Lo asigna el repositorio, nunca el cliente,
    // para que no haya dos películas con el mismo número.
    public int Id { get; set; }

    // El título de la película. Lo dejamos con "= string.Empty" para que nunca quede en null.
    public string Titulo { get; set; } = string.Empty;

    // Quién la dirigió.
    public string Director { get; set; } = string.Empty;

    // El año de estreno.
    public int Anio { get; set; }

    // El género: comedia, drama, ciencia ficción, etc.
    public string Genero { get; set; } = string.Empty;

    // Cuánto dura, en minutos.
    public int DuracionMinutos { get; set; }

    // La calificación de 0 a 10. El signo "?" quiere decir que puede venir vacía,
    // porque una película recién agregada puede no tener calificación todavía.
    public decimal? Calificacion { get; set; }
}
