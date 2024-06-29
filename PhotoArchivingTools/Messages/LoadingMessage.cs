using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhotoArchivingTools.Messages
{
    public class LoadingMessage(bool isVisible)
    {
        public bool IsVisible { get; set; } = isVisible;
    }
}
