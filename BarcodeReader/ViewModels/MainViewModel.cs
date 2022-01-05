using BarcodeReader.Commands;
using BarcodeReader.Services;
using System.Drawing;
using System.Reflection;
using System.Windows.Media;

namespace BarcodeReader.ViewModels
{
    public class MainViewModel : ViewModelBase
    {

        public MainViewModel()
        {
            NavCommand = new CommandView<string>(OnNav);
            _CurrentViewModel = scanViewModel;
            MainTitle = ApplicationInfo.AppNameVersion;
        }
        public string MainTitle { get; set; }

        private readonly ScanViewModel scanViewModel = new ScanViewModel();

        private readonly OptionsViewModel optionsViewModel = new OptionsViewModel();

        private readonly HistoriqueViewModel historiqueViewModel = new HistoriqueViewModel();

        private ViewModelBase _CurrentViewModel;

        public ViewModelBase CurrentViewModel
        {
            get => _CurrentViewModel;
            set => SetProperty(ref _CurrentViewModel, value);
        }

        public CommandView<string> NavCommand { get; private set; }


        private void OnNav(string destination)
        {
            switch (destination)
            {
                case "options":
                    CurrentViewModel = optionsViewModel;
                    break;
                case "scan":
                    CurrentViewModel = scanViewModel;
                    break;
                case "historique":
                    CurrentViewModel = historiqueViewModel;
                    break;
                default:
                   CurrentViewModel = scanViewModel;
                   break;
            }
        }
    }
}
