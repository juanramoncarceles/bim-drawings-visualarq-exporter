using System.Collections.Generic;

namespace VisualARQDataExporter
{
    public class FileTemplate
    {
        public FileTemplate(Dictionary<string, string> ProjectInfo, Dictionary<string, string> Drawings, Dictionary<CustomType, ObjectTypeDataStruct> ElementsData)
        {
            projectInfo = ProjectInfo;
            drawings = Drawings;
            elementsData = ElementsData;
        }

        public Dictionary<string, string> projectInfo;
        public Dictionary<string, string> drawings;
        public Dictionary<CustomType, ObjectTypeDataStruct> elementsData;
    }
}
