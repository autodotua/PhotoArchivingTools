using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhotoArchivingTools.ViewModels
{
    public class SimpleFileViewModel : SimpleFileOrDirViewModel
    {
        public SimpleFileViewModel(string path) : base(path)
        {
            Time = System.IO.File.GetLastWriteTime(path);
        }

        public DateTime Time { get; set; }
    }
}
