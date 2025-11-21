using HuertosApp.Models;
using HuertosApp.Services;

namespace HuertosApp.Pages;

public partial class EditarRegistroFertirriegoPage : ContentPage
{
    private readonly Fertirriego _registro;

    public EditarRegistroFertirriegoPage(Fertirriego registro)
    {
        InitializeComponent();
        _registro = registro;

        // Mostrar el año actual en el Label
        AnoActualLabel.Text = DateTime.Now.Year.ToString();

        CargarDatos();
    }

    private void CargarDatos()
    {
        // Cargar los valores existentes en los controles
        PickerHuerto.SelectedItem = _registro.Huerto;
        PickerEtapa.SelectedItem = _registro.EtapaFertilizacion;
        FechaRiegoDatePicker.Date = DateTime.Parse(_registro.FechaRiego);
        PickerFertilizantes.SelectedItem = _registro.Fertilizantes;
        PickerTipoRiego.SelectedItem = _registro.TipoRiego;
        PickerSector.SelectedItem = _registro.Sector;
        TiempoRiegoEntry.Text = _registro.TiempoRiego;
        M3SistemaEntry.Text = _registro.M3Sistema;
        MmSistemaEntry.Text = _registro.MmSistema;
        EVPPromEntry.Text = _registro.EVP_Prom;
        FertEst1Entry.Text = _registro.FertilizanteEst1;
        FertEst2Entry.Text = _registro.FertilizanteEst2;
        FertEst3Entry.Text = _registro.FertilizanteEst3;
        FertKgEntry.Text = _registro.FertilizanteKg;
        ObservacionEntry.Text = _registro.Observacion;
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        // Actualizar los datos del registro
        _registro.Huerto = PickerHuerto.SelectedItem?.ToString();
        _registro.EtapaFertilizacion = PickerEtapa.SelectedItem?.ToString();
        _registro.FechaRiego = FechaRiegoDatePicker.Date.ToString("yyyy-MM-dd");
        _registro.Fertilizantes = PickerFertilizantes.SelectedItem?.ToString();
        _registro.TipoRiego = PickerTipoRiego.SelectedItem?.ToString();
        _registro.Sector = PickerSector.SelectedItem?.ToString();
        _registro.TiempoRiego = TiempoRiegoEntry.Text;
        _registro.M3Sistema = M3SistemaEntry.Text;
        _registro.MmSistema = MmSistemaEntry.Text;
        _registro.EVP_Prom = EVPPromEntry.Text;
        _registro.FertilizanteEst1 = FertEst1Entry.Text;
        _registro.FertilizanteEst2 = FertEst2Entry.Text;
        _registro.FertilizanteEst3 = FertEst3Entry.Text;
        _registro.FertilizanteKg = FertKgEntry.Text;
        _registro.Observacion = ObservacionEntry.Text;

        try
        {
            // Guardar cambios en la base de datos
            await Database.GetDatabase().UpdateFertirriegoAsync(_registro);
            await DisplayAlert("Éxito", "Registro actualizado correctamente.", "OK");
            await Navigation.PopAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Error al guardar los cambios: {ex.Message}", "OK");
        }
    }
}
