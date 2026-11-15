
using System.Net;

namespace PM2E230138;

public partial class PerfilPage : ContentPage
{
    public PerfilPage()
    {
        InitializeComponent();

        NavigationPage.SetHasBackButton(this, false);
    }

    // Cargar información cuando se muestra el perfil
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await CargarPerfil();
    }

    // Obtener y mostrar los datos del usuario
    private async Task CargarPerfil()
    {
        try
        {
            var datos = await ApiService.ObtenerPerfilAsync();

            NombreLabel.Text = datos.Nombre;
            DisponibleSwitch.IsToggled = datos.Disponible;

            ExperienciaSlider.Value =
                Math.Clamp(datos.AniosExperiencia, 0, 15);

            ActualizarExperiencia();
        }
        catch (ApiException error)
        {
            await MostrarError(error.Message);

            if (error.Codigo == HttpStatusCode.Unauthorized)
            {
                ApiService.CerrarSesion();
                await Navigation.PopAsync();
            }
        }
        catch (Exception)
        {
            await DisplayAlert(
                "Conexión fallida",
                "No fue posible obtener los datos del perfil. Comprueba tu conexión e intenta nuevamente.",
                "Aceptar"
            );
        }
    }

    // Detectar cambios en los años de experiencia
    private void OnSliderChanged(object? sender, ValueChangedEventArgs e)
    {
        ActualizarExperiencia();
    }

    // Actualizar información de experiencia
    private void ActualizarExperiencia()
    {
        int anios = (int)ExperienciaSlider.Value;

        AniosLabel.Text = anios == 1
            ? "1 año de experiencia"
            : $"{anios} años de experiencia";

        switch (anios)
        {
            case <= 2:
                NivelLabel.Text = "Nivel Inicial";
                NivelBorder.BackgroundColor =
                    Color.FromArgb("#FCE7F3");
                NivelLabel.TextColor =
                    Color.FromArgb("#BE185D");
                break;

            case <= 7:
                NivelLabel.Text = "Nivel Intermedio";
                NivelBorder.BackgroundColor =
                    Color.FromArgb("#FBCFE8");
                NivelLabel.TextColor =
                    Color.FromArgb("#9D174D");
                break;

            default:
                NivelLabel.Text = "Nivel Avanzado";
                NivelBorder.BackgroundColor =
                    Color.FromArgb("#F9A8D4");
                NivelLabel.TextColor =
                    Color.FromArgb("#831843");
                break;
        }
    }

    // Mostrar mensajes de error
    private async Task MostrarError(string mensaje)
    {
        await DisplayAlert(
            "Aviso",
            mensaje,
            "Entendido"
        );
    }

    // Finalizar sesión del usuario
    private async void OnCerrarSesionClicked(object? sender, EventArgs e)
    {
        bool confirmar = await DisplayAlert(
            "Cerrar sesión",
            "¿Estás seguro de que deseas salir de tu cuenta?",
            "Sí, salir",
            "Cancelar"
        );

        if (!confirmar)
            return;

        ApiService.CerrarSesion();

        await Navigation.PopAsync();
    }
}
