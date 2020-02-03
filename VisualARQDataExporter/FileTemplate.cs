using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Dynamic;

namespace VisualARQDataExporter
{
    public class FileTemplate
    {
        public FileTemplate(Dictionary<string, string> drawings, Dictionary<Guid, ExpandoObject> elementsData)
        {
            Drawings = drawings;
            ElementsData = elementsData;
        }

        public Dictionary<string, string> Drawings;

        public Dictionary<Guid, ExpandoObject> ElementsData;
    }
}
