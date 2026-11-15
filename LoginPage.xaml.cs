
using System.Net;

namespace PM2E230138;

public partial class LoginPage : ContentPage
{
    private int intentos = 0;
    private const int limiteIntentos = 3;

    public LoginPage()
    {
        InitializeComponent();
        MostrarIntentos();
    }

    // Mostrar u ocultar la contraseña
    private void OnMostrarClaveChanged(object? sender, CheckedChangedEventArgs e)
    {
        ClaveEntry.IsPassword = !MostrarClaveCheck.IsChecked;
    }

    // Validar que los campos tengan información
    private bool ValidarCampos()
    {
        if (string.IsNullOrWhiteSpace(CorreoEntry.Text) ||
            string.IsNullOrWhiteSpace(ClaveEntry.Text))
        {
            DisplayAlert(
                "Información incompleta",
                "Debes ingresar tu correo y contraseña.",
                "Aceptar"
            );

            return false;
        }

        return true;
    }

    // Evento principal del inicio de sesión
    private async void OnLoginClicked(object? sender, EventArgs e)
    {
        if (intentos >= limiteIntentos)
            return;

        if (!ValidarCampos())
        {
            await DisplayAlert(
                "Campos vacíos",
                "Completa todos los datos solicitados.",
                "OK"
            );
            return;
        }

        string correo = CorreoEntry.Text!.Trim();
        string contrasena = ClaveEntry.Text!;

        CambiarEstadoCarga(true);

        try
        {
            await ApiService.LoginAsync(correo, contrasena);

            // Acceso correcto
            intentos = 0;
            MostrarIntentos();

            ClaveEntry.Text = "";

            await Navigation.PushAsync(new PerfilPage());
        }
        catch (ApiException error)
        {
            await ProcesarErrorAPI(error);
        }
        catch (Exception)
        {
            await DisplayAlert(
                "Error de conexión",
                "No fue posible comunicarse con el servidor. Intenta nuevamente.",
                "Aceptar"
            );
        }
        finally
        {
            CambiarEstadoCarga(false);
        }
    }

    // Procesar las respuestas de error de la API
    private async Task ProcesarErrorAPI(ApiException error)
    {
        if (error.Codigo == HttpStatusCode.Unauthorized)
        {
            intentos++;

            MostrarIntentos();

            if (intentos == limiteIntentos)
            {
                await DisplayAlert(
                    "Acceso bloqueado",
                    "Has alcanzado el límite de intentos permitidos.",
                    "Aceptar"
                );
            }
            else
            {
                await DisplayAlert(
                    "Datos incorrectos",
                    error.Message,
                    "Intentar nuevamente"
                );
            }
        }
        else
        {
            await DisplayAlert(
                "Error del servidor",
                error.Message,
                "Aceptar"
            );
        }
    }

    // Actualizar el estado visual de carga y del botón
    private void CambiarEstadoCarga(bool cargando)
    {
        Cargando.IsVisible = cargando;
        Cargando.IsRunning = cargando;

        LoginBtn.IsEnabled = !cargando && intentos < limiteIntentos;
    }

    // Mostrar la cantidad de intentos utilizados
    private void MostrarIntentos()
    {
        IntentosLabel.Text =
            $"Intentos fallidos: {intentos} de {limiteIntentos}";
    }
}
