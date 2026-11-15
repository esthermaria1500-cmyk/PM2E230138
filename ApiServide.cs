using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace PM2E230138
{
    public class ApiException : Exception
    {
        public HttpStatusCode Codigo { get; }

        public ApiException(string mensaje, HttpStatusCode codigo) : base(mensaje)
        {
            Codigo = codigo;
        }
    }

    public class PerfilModel
    {
        public string Nombre { get; set; } = "";
        public bool Disponible { get; set; }
        public int AniosExperiencia { get; set; }
    }

    public static class ApiService
    {
        private const string RutaLogin = "api/usuarios/login";
        private const string RutaPerfil = "api/usuarios/perfil";

        private static readonly HttpClient Http = new()
        {
            BaseAddress = new Uri(DeviceInfo.Platform == DevicePlatform.Android
                ? "http://10.0.2.2:5080/"
                : "http://localhost:5080/"),
            Timeout = TimeSpan.FromSeconds(10)   
        };

        private static string? Token { get; set; }

        public static async Task LoginAsync(string correo, string clave)
        {
            var resp = await Http.PostAsJsonAsync(RutaLogin, new { correo, clave });
            await RevisarAsync(resp);

            var datos = await resp.Content.ReadFromJsonAsync<TokenRespuesta>();
            if (datos == null || string.IsNullOrEmpty(datos.Token))
            {
                throw new ApiException("La API no devolvió un token.", HttpStatusCode.InternalServerError);
            }

            Token = datos.Token;

            Http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token);
        }

        public static async Task<PerfilModel> ObtenerPerfilAsync()
        {
            var resp = await Http.GetAsync(RutaPerfil);
            await RevisarAsync(resp);

            var perfil = await resp.Content.ReadFromJsonAsync<PerfilModel>();
            return perfil ?? new PerfilModel();
        }

        public static void CerrarSesion()
        {
            Token = null;
            Http.DefaultRequestHeaders.Authorization = null;
        }

        private static async Task RevisarAsync(HttpResponseMessage resp)
        {
            if (resp.IsSuccessStatusCode)
            {
                return;
            }

            string mensaje = resp.StatusCode == HttpStatusCode.Unauthorized
                ? "Sesión no válida o credenciales incorrectas."
                : "La API respondió con un error.";

            try
            {

                var error = await resp.Content.ReadFromJsonAsync<ErrorRespuesta>();
                if (!string.IsNullOrWhiteSpace(error?.Mensaje))
                {
                    mensaje = error.Mensaje;
                }
            }
            catch
            {

            }

            throw new ApiException(mensaje, resp.StatusCode);
        }

        private class TokenRespuesta
        {
            public string Token { get; set; } = "";
        }

        private class ErrorRespuesta
        {
            public string? Mensaje { get; set; }
        }
    }
}