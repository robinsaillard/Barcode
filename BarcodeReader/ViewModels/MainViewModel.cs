using System;
using BarcodeReader.Commands;
using BarcodeReader.Models;
using BarcodeReader.Services;


namespace BarcodeReader.ViewModels
{
    public class MainViewModel : ViewModelBase
    {

        public MainViewModel()
        {
            NavCommand = new CommandView<string>(OnNav);
            _CurrentViewModel = scanViewModel;
            MainTitle = ApplicationInfo.AppNameVersion;
            LoadAutoPrintState();
        }
        public string MainTitle { get; set; }

        private readonly ScanViewModel scanViewModel = new ScanViewModel();

       // private readonly OptionsViewModel optionsViewModel = new OptionsViewModel();

       // private readonly HistoriqueViewModel historiqueViewModel = new HistoriqueViewModel();

        private ViewModelBase _CurrentViewModel;

        public ViewModelBase CurrentViewModel
        {
            get => _CurrentViewModel;
            set => SetProperty(ref _CurrentViewModel, value);
        }

        public CommandView<string> NavCommand { get; private set; }

        private bool _IsAutoPrintEnabled;
        public bool IsAutoPrintEnabled
        {
            get => _IsAutoPrintEnabled;
            set
            {
                if (SetProperty(ref _IsAutoPrintEnabled, value))
                {
                    // Sauvegarder en base de données SEULEMENT si la valeur change
                    SaveAutoPrintState();
                }
            }
        }

        private void LoadAutoPrintState()
        {
            try
            {
                string postName = Environment.MachineName;

                // Récupère toutes les options du poste depuis la DB
                var options = DbManager.GetOptions(postName);

                // Cherche la clé AUTOPRINT_ENABLED
                if (options != null && options.ContainsKey("AUTOPRINT_ENABLED"))
                {
                    string value = options["AUTOPRINT_ENABLED"].Value;
                    // Accède directement au champ privé pour ne pas déclencher le setter
                    _IsAutoPrintEnabled = bool.TryParse(value, out bool result) && result;
                    System.Diagnostics.Debug.WriteLine($"AutoPrint chargé depuis DB: {_IsAutoPrintEnabled}");
                }
                else
                {
                    // Si la clé n'existe pas, la créer avec la valeur par défaut (false)
                    _IsAutoPrintEnabled = false;
                    CreateAutoPrintOptionIfNotExists(postName);
                    System.Diagnostics.Debug.WriteLine("AUTOPRINT_ENABLED n'existe pas en DB, valeur par défaut: false");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur lors du chargement AutoPrint: {ex.Message}");
            }
        }

        private void SaveAutoPrintState()
        {
            try
            {

                string postName = Environment.MachineName;

                var option = new Options
                {
                    Variable = "AUTOPRINT_ENABLED",
                    Value = IsAutoPrintEnabled.ToString()
                };

                DbManager.UpdateOptions(postName, option);

                System.Diagnostics.Debug.WriteLine($"AutoPrint sauvegardé en DB: {IsAutoPrintEnabled}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur lors de la sauvegarde AutoPrint: {ex.Message}");
            }
        }

        private void CreateAutoPrintOptionIfNotExists(string postName)
        {
            try
            {
                var options = DbManager.GetOptions(postName);
                
                if (options == null || !options.ContainsKey("AUTOPRINT_ENABLED"))
                {
                    System.Diagnostics.Debug.WriteLine("Création de AUTOPRINT_ENABLED en DB");
                    DbManager.InsertOption(postName, "AUTOPRINT_ENABLED", "false");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur lors de la création de l'option: {ex.Message}");
            }
        }


        private void OnNav(string destination)
        {
            switch (destination)
            {
                case "options":
                    OptionsViewModel optionsViewModel = new OptionsViewModel();
                    CurrentViewModel = optionsViewModel;
                    break;
                case "scan":
                    CurrentViewModel = scanViewModel;
                    break;
                case "historique":
                    HistoriqueViewModel historiqueViewModel = new HistoriqueViewModel();
                    CurrentViewModel = historiqueViewModel;
                    break;
                default:
                   CurrentViewModel = scanViewModel;
                   break;
            }
        }
    }
}
