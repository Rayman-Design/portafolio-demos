# Guía paso a paso de la API de películas

Esta guía explica cómo está armada la API, qué hace cada archivo, qué pasa por dentro cuando llega una petición y cómo usar lo mismo para crear otra API. La idea es que entiendas el porqué de cada pieza, no solo que funcione.

Como vienes trabajando Java, en varios puntos va la comparación con Spring Boot, porque la lógica es casi la misma.

## 1. La idea general en un minuto

Una API REST es un programa que se queda escuchando peticiones HTTP y responde con datos, normalmente en JSON. Cada combinación de **método** (GET, POST, PUT, DELETE) y **ruta** (`/api/peliculas/3`) es un endpoint.

Esta API está dividida en capas, y cada una tiene un solo trabajo:

| Capa | Archivo | Su trabajo |
|---|---|---|
| Arranque | `Program.cs` | Prender la app y conectar las piezas |
| Controlador | `PeliculasController.cs` | Recibir la petición y decidir la respuesta HTTP |
| DTO | `PeliculaEntradaDto.cs` | Definir qué datos se aceptan y sus reglas |
| Modelo | `Pelicula.cs` | Definir cómo es una película |
| Repositorio | `IPeliculaRepositorio.cs` y `PeliculaRepositorioEnMemoria.cs` | Guardar y buscar los datos |

La regla de oro: **el controlador no sabe dónde se guardan los datos, y el repositorio no sabe nada de HTTP.** Esa separación es la que hace que el proyecto se vea profesional y sea fácil de cambiar.

## 2. Recorrido archivo por archivo

Te recomiendo abrir cada archivo mientras lees esta parte. Todos tienen comentarios en el código que dicen lo mismo, pero pegados a cada línea.

### Paso 1: PeliculasApi.csproj

Es la ficha del proyecto. Dice tres cosas:

- `Sdk="Microsoft.NET.Sdk.Web"`: que es una app web, así .NET incluye todo lo de ASP.NET Core (servidor, controladores, JSON).
- `net10.0`: que usamos .NET 10.
- `Nullable` e `ImplicitUsings`: dos ayudas del compilador. La primera te avisa si algo puede venir en `null`; la segunda te ahorra escribir los `using` más comunes.

En Java esto sería el `pom.xml`. Aquí no agregamos ningún paquete externo, todo viene con .NET.

### Paso 2: Modelos/Pelicula.cs

Es la clase que representa una película: `Id`, `Titulo`, `Director`, `Anio`, `Genero`, `DuracionMinutos` y `Calificacion`.

Dos detalles de C# que vale la pena aprender aquí:

- `{ get; set; }` crea la propiedad con su getter y setter en una línea. En Java tendrías que escribir `getTitulo()` y `setTitulo()`.
- `decimal?` con signo de interrogación quiere decir que puede venir vacía. La calificación es opcional, por eso la marcamos así.

Cuando .NET devuelve una `Pelicula` en JSON, convierte los nombres a camelCase solito: `DuracionMinutos` sale como `duracionMinutos`.

### Paso 3: Dtos/PeliculaEntradaDto.cs

El DTO es lo que **acepta** la API cuando alguien crea o actualiza. Tiene los mismos campos que el modelo, menos el `Id`.

¿Por qué una clase aparte? Porque si usáramos `Pelicula` directamente, el cliente podría mandar `"id": 1` y pisar otra película. Con el DTO, el `Id` ni siquiera existe para el que manda los datos. Es una costumbre de seguridad que se ve mucho en proyectos reales.

Encima de cada campo hay **atributos de validación**, que son las reglas:

- `[Required]`: el campo es obligatorio y no puede venir vacío ni con puros espacios.
- `[StringLength(200)]`: máximo 200 caracteres.
- `[Range(1900, 2100)]`: el número tiene que estar en ese rango.

Cada uno tiene su `ErrorMessage` en español, que es lo que ve el cliente si se equivoca. En Spring sería lo mismo con `@NotBlank`, `@Size` y `@Min`/`@Max`.

### Paso 4: Repositorios/IPeliculaRepositorio.cs

Es una **interfaz**: una lista de métodos sin código adentro. Es el contrato que dice "esto es lo que se puede hacer con las películas":

- `ObtenerTodas(genero)`
- `ObtenerPorId(id)`
- `Crear(pelicula)`
- `Actualizar(id, datosNuevos)`
- `Eliminar(id)`

Fíjate en lo que devuelven, porque es la forma en que el repositorio le avisa al controlador si algo salió mal, sin saber nada de HTTP:

- `ObtenerPorId` y `Actualizar` devuelven `null` si la película no existe.
- `Eliminar` devuelve `true` o `false`.

El controlador traduce eso a códigos HTTP (por ejemplo, `null` se vuelve 404).

### Paso 5: Repositorios/PeliculaRepositorioEnMemoria.cs

Aquí está el cómo. Las películas se guardan en una `List<Pelicula>`, que es como un `ArrayList` de Java.

Lo más importante de este archivo:

- **El constructor** carga cuatro películas inventadas para que la API no arranque vacía, y deja listo el contador `_siguienteId` en 5.
- **El Id lo pone el repositorio** en `Crear`, con `_siguienteId++`. Así nunca se repite.
- **`lock (_candado)`** envuelve cada método. Una API puede recibir varias peticiones al mismo tiempo; el candado hace que entren de a una a la lista, para que no se dañe. Es como `synchronized` en Java.
- **`Copiar(...)`** devuelve copias, no los objetos originales. Así nadie puede cambiar la lista "por la puerta de atrás".
- **LINQ**: `Where`, `FirstOrDefault`, `OrderBy` y `RemoveAll` son las herramientas de C# para filtrar y buscar en listas. Son muy parecidas a los Streams de Java (`filter`, `findFirst`, `sorted`).

Como todo está en memoria, al apagar la API se pierde lo que hayas creado. Para una demo está bien.

### Paso 6: Controladores/PeliculasController.cs

Este es el corazón de la API. Cada método público es un endpoint.

Arriba de la clase hay tres atributos:

- `[ApiController]`: activa la validación automática. Si el JSON no cumple las reglas del DTO, .NET responde 400 antes de entrar a tu método.
- `[Route("api/peliculas")]`: la ruta base de todos los endpoints de la clase.
- `[Produces("application/json")]`: siempre responde en JSON.

El constructor recibe un `IPeliculaRepositorio`. Nadie escribe `new` en ningún lado: .NET lo entrega porque lo registramos en `Program.cs`. Eso se llama **inyección de dependencias**, y es lo mismo que hace Spring con `@Autowired` o con el constructor.

Los atributos de cada método dicen a qué petición responde:

| Atributo | Ejemplo de petición | Método |
|---|---|---|
| `[HttpGet]` | `GET /api/peliculas` | `ObtenerTodas` |
| `[HttpGet("{id:int}")]` | `GET /api/peliculas/3` | `ObtenerPorId` |
| `[HttpPost]` | `POST /api/peliculas` | `Crear` |
| `[HttpPut("{id:int}")]` | `PUT /api/peliculas/3` | `Actualizar` |
| `[HttpDelete("{id:int}")]` | `DELETE /api/peliculas/3` | `Eliminar` |

De dónde salen los datos de cada parámetro:

- `{id:int}` en la ruta llena el parámetro `int id`. El `:int` hace que `/api/peliculas/hola` no entre (responde 404).
- `[FromQuery]` toma lo que va después del `?` en la URL, como `?genero=Comedia`.
- `[FromBody]` toma el JSON del cuerpo de la petición y lo convierte en un `PeliculaEntradaDto`.

Y los métodos de respuesta, que eligen el código HTTP:

| Método en C# | Código | Cuándo lo usamos |
|---|---|---|
| `Ok(datos)` | 200 | Todo bien, aquí van los datos |
| `CreatedAtRoute(...)` | 201 | Se creó algo nuevo; incluye su URL en Location |
| `NoContent()` | 204 | Se hizo (por ejemplo, se borró) y no hay nada que mostrar |
| `NotFound(...)` | 404 | Ese id no existe |
| (automático) | 400 | Los datos no cumplen las reglas del DTO |

Al final está `ConvertirAModelo`, un ayudante privado que pasa el DTO a `Pelicula` y limpia los espacios sobrantes con `Trim()`. Lo usan `Crear` y `Actualizar`, así no repetimos código.

### Paso 7: Program.cs

Es lo primero que corre. Tiene dos partes:

1. **Registrar servicios**: `AddControllers()` activa los controladores y `AddSingleton<IPeliculaRepositorio, PeliculaRepositorioEnMemoria>()` le dice a .NET: "cuando alguien pida la interfaz, entrégale esta clase, y que sea siempre la misma instancia".
2. **Configurar y arrancar**: `MapGet("/")` crea el mensaje de bienvenida, `MapControllers()` conecta las rutas de los controladores y `app.Run()` deja el servidor escuchando.

Sobre `AddSingleton`: hay tres formas de registrar un servicio en .NET, y elegir bien importa.

- **Singleton**: una sola instancia para toda la app. La usamos aquí porque la lista tiene que sobrevivir entre peticiones.
- **Scoped**: una instancia nueva por cada petición. Es lo normal cuando usas una base de datos con Entity Framework.
- **Transient**: una instancia nueva cada vez que alguien la pide.

Si aquí pusieras `AddScoped`, cada petición tendría su propia lista y la película que creas con POST desaparecería en el siguiente GET. Es un buen experimento para entenderlo.

### Paso 8: los archivos de apoyo

- `Properties/launchSettings.json`: fija el puerto en 5080 cuando usas `dotnet run`.
- `appsettings.json`: la configuración de los mensajes que salen en la terminal (logs).
- `PeliculasApi.http`: diez peticiones listas para probar con un clic.
- `.gitignore`: le dice a Git que no suba las carpetas `bin` y `obj`, que se generan al compilar.

## 3. Qué pasa por dentro en cada endpoint

### Crear una película (POST /api/peliculas)

Este es el recorrido más completo, vale la pena seguirlo con calma:

1. El cliente manda un POST con un JSON en el cuerpo.
2. .NET mira la ruta y el método y encuentra `Crear` en `PeliculasController`.
3. Convierte el JSON en un `PeliculaEntradaDto` (por el `[FromBody]`).
4. Revisa las reglas del DTO. Si algo falla, responde 400 con la lista de errores y aquí termina todo; tu método nunca se ejecuta.
5. Si todo está bien, entra a `Crear`, que pasa el DTO a `Pelicula` con `ConvertirAModelo`.
6. Llama a `_repositorio.Crear(...)`, que le pone el siguiente Id y la agrega a la lista.
7. El controlador responde 201 con la película creada y el encabezado `Location: http://localhost:5080/api/peliculas/5`.

### Listar (GET /api/peliculas)

Entra a `ObtenerTodas`. Si la URL trae `?genero=...`, el repositorio filtra sin importar mayúsculas. Siempre responde 200, aunque la lista venga vacía: una lista vacía no es un error.

### Consultar por id (GET /api/peliculas/3)

Entra a `ObtenerPorId` con `id = 3`. Si el repositorio devuelve `null`, responde 404 con un mensaje; si no, 200 con la película.

### Actualizar (PUT /api/peliculas/3)

Junta los dos mundos: el id viene de la URL y los datos nuevos del cuerpo. Primero valida el DTO (400 si falla), luego intenta actualizar (404 si no existe) y si sale bien responde 200 con la película ya cambiada.

PUT reemplaza **todos** los campos. Si mandas solo el título, la validación falla porque faltan los demás. Para cambios parciales existe PATCH, que aquí no usamos para mantenerlo simple.

### Borrar (DELETE /api/peliculas/3)

Entra a `Eliminar`. Si la borró, responde 204 sin cuerpo; si no existía, 404. Si borras la misma película dos veces, la primera da 204 y la segunda 404.

## 4. Cómo crear otra API con lo que aprendiste

El patrón es siempre el mismo, cambie el tema o no. Supongamos que quieres una API de **productos** para una tienda. Estos son los pasos, en orden:

1. **Crea el proyecto vacío.** En una carpeta nueva:

   ```bash
   dotnet new webapi --use-controllers -o ProductosApi
   ```

   La plantilla trae un ejemplo del clima (`WeatherForecast`). Bórralo o déjalo de referencia. Si prefieres empezar como este proyecto, copia la carpeta `PeliculasApi` y ve cambiando nombres.

2. **Escribe el modelo** en `Modelos/Producto.cs`: `Id`, `Nombre`, `Precio`, `Stock`, `Categoria`. Pregunta clave: ¿qué campos son obligatorios y cuáles opcionales (con `?`)?

3. **Escribe el DTO** en `Dtos/ProductoEntradaDto.cs`, con todo menos el `Id`, y sus reglas. Por ejemplo, `[Range(0.01, 1000000)]` para el precio y `[Range(0, 100000)]` para el stock.

4. **Escribe la interfaz** `IProductoRepositorio` con los mismos cinco métodos. Cambia `Pelicula` por `Producto` y listo.

5. **Escribe el repositorio en memoria.** Copia `PeliculaRepositorioEnMemoria`, cambia el tipo, los datos de ejemplo y el filtro (por ejemplo, por categoría en vez de género). Ajusta `Copiar` y `Actualizar` a los campos nuevos.

6. **Escribe el controlador** `ProductosController` con `[Route("api/productos")]`. Los cinco métodos son iguales; solo cambian los tipos, los nombres de los campos y los mensajes. Acuérdate de cambiar también el `Name` de la ruta del GET por id, por ejemplo `"ObtenerProductoPorId"`, y usarlo igual en `CreatedAtRoute`.

7. **Regístralo en Program.cs:**

   ```csharp
   builder.Services.AddSingleton<IProductoRepositorio, ProductoRepositorioEnMemoria>();
   ```

8. **Pruébalo** con un archivo `.http` como el de esta API: un caso bueno y uno malo por cada endpoint.

Un truco para no perderte: escribe una tabla como la del README (método, ruta, qué hace, respuestas) **antes** de escribir código. Si la tabla está clara, el controlador sale casi solo.

## 5. Siguientes pasos para seguir aprendiendo

Cuando ya domines esta versión, estos son buenos retos, en este orden:

1. **Un endpoint nuevo**: por ejemplo `GET /api/peliculas/mejores`, que devuelva las de calificación 8 o más. Te obliga a tocar las tres capas.
2. **PATCH**: actualizar solo algunos campos.
3. **Base de datos real con Entity Framework Core y SQLite**: creas una clase `PeliculaRepositorioEf` que cumpla la misma interfaz y cambias una sola línea en `Program.cs`. Ahí vas a ver por qué valía la pena separar el repositorio.
4. **Documentación OpenAPI**: con el paquete `Microsoft.AspNetCore.OpenApi`, que es el que trae la plantilla oficial de .NET 10, la API publica una descripción de todos sus endpoints.
5. **Pruebas automáticas con xUnit**: la línea `public partial class Program { }` al final de `Program.cs` ya está puesta para eso.

## 6. Glosario rápido

- **Endpoint**: una combinación de método HTTP y ruta que hace algo.
- **CRUD**: Create, Read, Update, Delete (crear, leer, actualizar, borrar).
- **DTO**: clase que define qué datos entran o salen de la API.
- **Repositorio**: la clase que guarda y busca los datos.
- **Interfaz**: un contrato con la lista de métodos, sin código adentro.
- **Inyección de dependencias**: .NET crea los objetos que necesitas y te los pasa por el constructor.
- **Atributo**: lo que va entre corchetes encima de una clase o método, como `[HttpGet]`. Es como una anotación de Java (`@GetMapping`).
- **LINQ**: las herramientas de C# para filtrar, ordenar y buscar en listas.

## Para el portafolio

Con este proyecto puedes mostrar que sabes armar una API REST en .NET con capas separadas, validación y códigos HTTP correctos. Preséntalo como lo que es: un proyecto demostrativo y de aprendizaje. No equivale a años de experiencia profesional en .NET.
