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
        public FileTemplate(Dictionary<string, string> ProjectInfo, Dictionary<string, string> Drawings, Dictionary<Guid, ExpandoObject> ElementsData)
        {
            projectInfo = ProjectInfo;
            drawings = Drawings;
            elementsData = ElementsData;
        }

        public Dictionary<string, string> projectInfo;
        public Dictionary<string, string> drawings;
        public Dictionary<Guid, ExpandoObject> elementsData;
    }
}
