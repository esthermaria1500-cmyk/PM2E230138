
namespace PM2E230138
{
    public partial class MainPage : ContentPage
    {
        private int totalClicks = 0;

        public MainPage()
        {
            InitializeComponent();

            ActualizarBoton();
        }

        // Evento que se ejecuta al presionar el botón
        private void OnCounterClicked(object? sender, EventArgs e)
        {
            totalClicks++;

            ActualizarBoton();
        }

        // Actualizar el texto del botón
        private void ActualizarBoton()
        {
            string mensaje = totalClicks == 1
                ? "Has hecho clic 1 vez"
                : $"Has hecho clic {totalClicks} veces";

            CounterBtn.Text = mensaje;

            SemanticScreenReader.Announce(mensaje);
        }
    }
}
