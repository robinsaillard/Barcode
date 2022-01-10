namespace BarcodeReader.ViewModels
{
    public class HistoriqueViewModel : ViewModelBase
    {
        public HistoriqueViewModel()
        {

        }

        private string _textUrlContent; 
        public string TextUrlContent { 
            get => _textUrlContent; 
            set => SetProperty(ref _textUrlContent, value); 
        }
    }
}
