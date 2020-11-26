using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Errors.Models
{
    public class Error
    {
        private DateTime _time;
        public string TimeEx { get; set; }
        public string NameAddin { get; set; }
        public string ModelPathStr { get; set; }
        public string UserName { get; set; }
        public string Massage { get; set; }
        public string StackTrace { get; set; }
        public bool IsFixed { get; set; }
        public string Version { get; set; }
        public DateTime Time
        {
            get => DateTime.Parse(TimeEx);
            set => _time = value;
        }
    }
}
