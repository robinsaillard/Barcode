using BarcodeReader.Commands;
using BarcodeReader.Models;
using BarcodeReader.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace BarcodeReader.ViewModels
{
    public class OptionsViewModel : ViewModelBase
    {
        public CommandView<string> SaveOption { get; private set; }
        public List<Options> Options { get; private set; }

        
        public OptionsViewModel()
        {
            SaveOption = new CommandView<string>(OnSaveOption);

            string postName = Environment.MachineName.ToString();

            if (!DbManager.PostNameExist(postName))
            {
                DbManager.InsertPost(postName);
            }

            Options = DbManager.GetOptions(postName).Values.ToList();
            DataGridContent = Options;
        }
    
        private void OnSaveOption(string obj)
        {
            string postName = Environment.MachineName.ToString();
            Options = DbManager.GetOptions(postName).Values.ToList();
            int res = 0; 
            for (int i = 0; i < Options.Count; i++)
            {
                if (Options[i].Value != DataGridContent[i].Value)
                {
                    res += DbManager.UpdateOptions(postName, DataGridContent[i]);
                }
            }
            if (res > 0)
            {
               MessageBox.Show($"{res} variable(s) mise(s) à jour", "Base de données"); 
            }
        }

        private List<Options> _dataGridContent;
        public List<Options> DataGridContent
        {
            get => _dataGridContent;
            set => SetProperty(ref _dataGridContent, value);
        }
    }
}
