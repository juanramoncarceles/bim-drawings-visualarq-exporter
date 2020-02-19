using System;
using System.Collections.Generic;

namespace VisualARQDataExporter
{
    public enum CustomType
    {
        Wall,
        CurtainWall,
        Beam,
        Column,
        Door,
        Window,
        Stair,
        Railing,
        Slab,
        Furniture,
        GenericElement,
        Unknown,
        RhinoGeometry
    }


    public struct ObjectTypeDataStruct
    {
        public ObjectTypeDataStruct(Dictionary<Guid, ObjectDataStruct> Instances, Dictionary<Guid, ObjectDataStruct> Styles)
        {
            instances = Instances;
            styles = Styles;
        }

        public Dictionary<Guid, ObjectDataStruct> instances;
        public Dictionary<Guid, ObjectDataStruct> styles;
    }


    public struct ObjectDataStruct
    {
        public ObjectDataStruct(IEnumerable<DataEntry> Properties, IEnumerable<DataEntry> Parameters)
        {
            properties = Properties;
            parameters = Parameters;
        }

        public IEnumerable<DataEntry> properties;
        public IEnumerable<DataEntry> parameters;
    }


    public struct DataEntry
    {
        public DataEntry(string Name, object Value, string Category = "unknown")
        {
            name = Name;
            value = Value;
            category = Category;
        }

        public string name;
        public object value;
        public string category;
    }


    public struct DataStructures
    {
        public DataStructures(Dictionary<string, string> ProjectInfo, Dictionary<string, string> Drawings, Dictionary<CustomType, ObjectTypeDataStruct> ElementsData)
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
