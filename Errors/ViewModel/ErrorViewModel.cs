using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using Errors.Core;
using Errors.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PropertyChanged;

namespace Errors.ViewModel
{
    public class ErrorViewModel:BaseViewModel
    {

        #region Fields

        private static string PathToErrors = "D:\\Работа\\Временные файлы\\LoggerJournal";
        private bool _isSelectedUnfixedError;
        private string _selectedNameAddin;
        #endregion

        #region Properties
        public List<string> NameAddinCollection { get; set; }= new List<string>();

        public string SelectedNameAddin
        {
            get => _selectedNameAddin;
            set
            {
                if (value==_selectedNameAddin)
                    return;
                _selectedNameAddin = value;
                ErrorCollectionViewSource?.View?.Refresh();
            }
        }

        public bool IsSelectedUnfixedError
        {
            get => _isSelectedUnfixedError;
            set
            {
                _isSelectedUnfixedError = value;
                Stopwatch watch = new Stopwatch();
                watch.Start();
                ErrorCollectionViewSource?.View?.Refresh();
                watch.Stop();
                TimeSpan ts = watch.Elapsed;
                Debug.WriteLine($"-----------Time:{ts}");
            }
        }
        public ObservableCollection<Error> ErrorCollection { get; set; }

        public CollectionViewSource ErrorCollectionViewSource { get; set; }

        public Error ErrorDetail { get; set; }

        #endregion

        #region Commands

        public RelayCommand SaveAllFixedErrorCommand => new RelayCommand(o =>
        {
            foreach (var error in ErrorCollection)
            {
                string format = "yyyyddMMHHmmss";
                string fn = error.Time.ToString(format);
                JsonSerializer serializer = new JsonSerializer();
                string path = Path.Combine(PathToErrors, fn + ".json");
                if (File.Exists(path))
                {
                    Serialize(path, error);
                }
                else
                {
                    string fnplus1 = error.Time.AddSeconds(1).ToString(format);
                    JsonSerializer serializerplus1 = new JsonSerializer();
                    string pathplus1 = Path.Combine(PathToErrors, fnplus1 + ".json");
                    Serialize(pathplus1,error);
                }

            }
        });

        #endregion

        #region Methods

        public void Initialize()
        {
            ErrorCollection=new ObservableCollection<Error>();
            var files = Directory.GetFiles(PathToErrors);
            foreach (var file in files)
            {
                StreamReader sr = new StreamReader(file);
                string input = sr.ReadToEnd();
                Error error = JsonConvert.DeserializeObject<Error>(input);
                ErrorCollection.Add(error);
                sr.Close();
            }
            ErrorCollection = new ObservableCollection<Error>(ErrorCollection.OrderBy(x => x.NameAddin).ThenBy(x => x.Massage).ToList());

            NameAddinCollection = ErrorCollection.Select(x => x.NameAddin).Distinct().ToList();
            NameAddinCollection.Add("Все ошибки");

            ErrorCollectionViewSource = new CollectionViewSource
            {
                Source = ErrorCollection,
                IsLiveFilteringRequested = true
            };
            ErrorCollectionViewSource.Filter += (sender, args) =>
            {
                var error = args.Item as Error;
                if (_isSelectedUnfixedError)
                {
                    if (error?.IsFixed == true)
                        args.Accepted = false;
                }

                if (SelectedNameAddin != "Все ошибки")
                {
                    if (error?.NameAddin != _selectedNameAddin)
                        args.Accepted = false;
                }
            };

            SelectedNameAddin = "Все ошибки";
        }

        void Serialize(string path, Error error)
        {
            FileInfo myFile = new FileInfo(path);
            myFile.Attributes &= ~FileAttributes.Hidden;
            var options = new JsonSerializerSettings { Formatting = Formatting.Indented };
            var jsonString = JsonConvert.SerializeObject(error, options);
            File.WriteAllText(path, jsonString);
        }

        #endregion

        #region Constructors

        public ErrorViewModel()
        {
            Initialize();
        }

        #endregion
    }
}
