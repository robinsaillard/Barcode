using BarcodeReader.Commands;
using BarcodeReader.Models;
using BarcodeReader.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BarcodeReader.ViewModels
{
    public class OptionsViewModel : ViewModelBase
    {
        public CommandView<string> SaveOption { get; private set; }
        public List<Options> Options { get; private set; }

        public List<Options> OptionInDataBase { get;}

        
        public OptionsViewModel()
        {
            SaveOption = new CommandView<string>(OnSaveOption);

            string postName = Environment.MachineName.ToString();

            if (!DbManager.PostNameExist(postName))
            {
                DbManager.InsertPost(postName);
            }

            OptionInDataBase = DbManager.GetOptions(postName).Values.ToList();
            Options = DbManager.GetOptions(postName).Values.ToList();
            DataGridContent = Options;
        }

        private void OnSaveOption(string obj)
        {
            var un = Options;
            var deux = DataGridContent;

            var diffArray = OptionInDataBase.SequenceEqual(DataGridContent);
            string postName = Environment.MachineName.ToString();

            if (diffArray == false)
            {

                for (int i = 0; i < Options.Count; i++)
                {

                    if (Options[i].Value != OptionInDataBase[i].Value)
                    {
                        
                        DbManager.UpdateOptions(postName, Options[i]);
                    }
                }

                
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
