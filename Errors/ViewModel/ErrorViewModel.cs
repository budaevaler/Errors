using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        private ObservableCollection<Error> _unfixedErrorCollection; 

        #endregion

        #region Properties
        public List<string> NameAddinCollection { get; set; }= new List<string>();
        public string SelectedNameAddin { get; set; } = "Все ошибки";
        public bool IsSelectedUnfixedError { get; set; }


        public ObservableCollection<Error> UnfixedErrorCollection { get; set; }=new ObservableCollection<Error>();
        public ObservableCollection<Error> TempName { get; set; }
        public ObservableCollection<Error> TempFixing { get; 
            set; }
        public ObservableCollection<Error> ErrorCollection { get; 
            set; }


        public Error ErrorDetail { get; set; }

        #endregion

        #region Commands

        public RelayCommand SaveAllFixedErrorCommand => new RelayCommand(o =>
        {
            foreach (var error in TempName)
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

        public RelayCommand ChangeTypeOfAddinsCommand =>new RelayCommand(o =>
        {
            ErrorCollection=FilterName(TempName);
        });

        public RelayCommand FixingFilterCommand=>new RelayCommand(o =>
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();
            UnfixedErrorCollection = new ObservableCollection<Error>(TempFixing.Where(x => x.IsFixed == false));
            ErrorCollection = FilterFixed(TempFixing,UnfixedErrorCollection);
            watch.Stop();
            TimeSpan ts = watch.Elapsed;
            Debug.WriteLine($"----------Time:{ts}");
        });

        public RelayCommand UpdateFixedErrorCommand=>new RelayCommand(o =>
        {
            
            //if (o is Error error)
            //{
            //    if (TempFixing.Contains(error))
            //    {
            //        int n = TempFixing.IndexOf(error);
            //        TempFixing[n].IsFixed = error.IsFixed;
            //    }
            //    if (UnfixedErrorCollection.Contains(error))
            //    {
            //        int n = UnfixedErrorCollection.IndexOf(error);
            //        UnfixedErrorCollection[n].IsFixed = error.IsFixed;
            //    }
            //}
        });

        #endregion

        #region Methods

        ObservableCollection<Error> FilterFixed(ObservableCollection<Error> errorCollection, ObservableCollection<Error> unfixedErrorCollection)
        {
           if (IsSelectedUnfixedError == true)
               return unfixedErrorCollection;
           else return errorCollection;
        }

       ObservableCollection<Error> FilterName(ObservableCollection<Error> errorCollection)
       {
           if (SelectedNameAddin == "Все ошибки")
           {
               UnfixedErrorCollection = new ObservableCollection<Error>(errorCollection.Where(x => x.IsFixed == false));
               TempFixing = errorCollection;
               return FilterFixed(errorCollection, UnfixedErrorCollection);
           }
           else
           {
               TempFixing = new ObservableCollection<Error>();
               foreach (var error in errorCollection)
               {
                   if (SelectedNameAddin == error.NameAddin)
                   {
                       TempFixing.Add(error);
                   }

               }
               UnfixedErrorCollection = new ObservableCollection<Error>(TempFixing.Where(x => x.IsFixed == false));
               return FilterFixed(TempFixing, UnfixedErrorCollection);
           }
       }
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
            TempName = ErrorCollection;
            TempFixing = ErrorCollection;
            foreach (var error in ErrorCollection)
            {
                if (error.IsFixed==false)
                    UnfixedErrorCollection.Add(error);
            }
        }

        #endregion
    }
}
