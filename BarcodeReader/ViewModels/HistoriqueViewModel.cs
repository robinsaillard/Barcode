using BarcodeReader.Models;
using BarcodeReader.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;

namespace BarcodeReader.ViewModels
{
    public class HistoriqueViewModel : ViewModelBase
    {
        public HistoriqueViewModel()
        {
            List<Historique> historique = DbManager.GetScanList();
            DataGridContent = CollectionViewSource.GetDefaultView(historique.AsEnumerable());
            DataGridContent.Filter = new Predicate<object>(Filter);
        }

        private bool Filter(object obj)
        {
            var data = obj as Historique;
            if (data != null)
            {
                if (!string.IsNullOrEmpty(_filterString))
                {
                    return data.Url.Contains(_filterString);
                }
                else if (!string.IsNullOrEmpty(_dateFilterString))
                {
                    DateTime date = DateTime.Parse(_dateFilterString);
                    return data.Date.Equals(date);
                }
               return true;
            }
            return false;
        }

        private string _filterString;
        public string FilterString
        {
            get { return _filterString; }
            set
            {
                _filterString = value;
                OnPropertyChanged("FilterString");
                FilterCollection();
            }
        }

        private string _dateFilterString;
        public string DateFilterString
        {
            get { return _dateFilterString; }
            set
            {
                _dateFilterString = value;
                OnPropertyChanged("DateFilterString");
                FilterCollection();
            }
        }

        private void FilterCollection()
        {
            if (_dataGridContent != null)
            {
                _dataGridContent.Refresh();
            }
        }

        private ICollectionView _dataGridContent;
        public ICollectionView DataGridContent
        {
            get => _dataGridContent;
            set => SetProperty(ref _dataGridContent, value);
        }
    }
}
