// =====================================================================
// PeliculaEntradaDto.cs: el DTO de ENTRADA.
// DTO quiere decir "Data Transfer Object" (objeto para transportar datos).
// Aquí definimos qué datos puede mandar el cliente cuando crea o actualiza
// una película, y qué reglas deben cumplir.
//
// ¿Por qué no usamos directamente la clase Pelicula?
// Porque así el cliente no puede mandar el Id ni otros campos que no le tocan.
// El modelo es lo que guardamos; el DTO es lo que aceptamos desde afuera.
// =====================================================================

using System.ComponentModel.DataAnnotations;

namespace PeliculasApi.Dtos;

public class PeliculaEntradaDto
{
    // [Required] hace que el campo sea obligatorio. También rechaza textos vacíos o con puros espacios.
    // [StringLength] pone un límite de caracteres para que no nos manden textos gigantes.
    // Los mensajes en ErrorMessage son los que verá el cliente si se equivoca.
    [Required(ErrorMessage = "El título es obligatorio.")]
    [StringLength(200, ErrorMessage = "El título no puede pasar de 200 caracteres.")]
    public string Titulo { get; set; } = string.Empty;

    [Required(ErrorMessage = "El director es obligatorio.")]
    [StringLength(100, ErrorMessage = "El director no puede pasar de 100 caracteres.")]
    public string Director { get; set; } = string.Empty;

    // [Range] revisa que el número esté dentro de un rango.
    // Aquí aceptamos años de 1900 a 2100 para cortar errores como "20255" o "0".
    [Range(1900, 2100, ErrorMessage = "El año debe estar entre 1900 y 2100.")]
    public int Anio { get; set; }

    [Required(ErrorMessage = "El género es obligatorio.")]
    [StringLength(50, ErrorMessage = "El género no puede pasar de 50 caracteres.")]
    public string Genero { get; set; } = string.Empty;

    // Duración entre 1 y 600 minutos (10 horas), para que no llegue 0 ni un número absurdo.
    [Range(1, 600, ErrorMessage = "La duración debe estar entre 1 y 600 minutos.")]
    public int DuracionMinutos { get; set; }

    // La calificación es opcional (por eso el "?"), pero si viene, debe estar entre 0 y 10.
    [Range(0, 10, ErrorMessage = "La calificación debe estar entre 0 y 10.")]
    public decimal? Calificacion { get; set; }
}
