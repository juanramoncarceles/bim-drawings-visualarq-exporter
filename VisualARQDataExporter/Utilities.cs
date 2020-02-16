using System;
using System.Collections.Generic;
using static VisualARQ.Script;

namespace VisualARQDataExporter
{
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


    public static class Utilities
    {
        /// <summary>
        /// Gets the type of the Rhino Object adding VisualARQ types.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static CustomType GetCustomType(Guid id)
        {
            Rhino.DocObjects.RhinoObject sourceObj = Rhino.RhinoDoc.ActiveDoc.Objects.FindId(id);

            if (sourceObj.ObjectType == Rhino.DocObjects.ObjectType.InstanceReference)
            {
                if (IsProduct(id))
                {
                    if (IsWall(id))
                    {
                        return CustomType.Wall;
                    }
                    // TODO: missing IsCurtainWall(id)
                    else if (IsBeam(id))
                    {
                        return CustomType.Beam;
                    }
                    else if (IsColumn(id))
                    {
                        return CustomType.Column;
                    }
                    else if (IsDoor(id))
                    {
                        return CustomType.Door;
                    }
                    else if (IsWindow(id))
                    {
                        return CustomType.Window;
                    }
                    // TODO: missing IsStair(id)
                    // TODO: missing IsRailing(id)
                    else if (IsSlab(id))
                    {
                        return CustomType.Slab;
                    }
                    // TODO: missing IsRoof(id)
                    else if (IsFurniture(id))
                    {
                        return CustomType.Furniture;
                    }
                    else if (IsElement(id))
                    {
                        return CustomType.GenericElement;
                    }
                    else
                    {
                        return CustomType.Unknown;
                    }
                }
                else
                {
                    return CustomType.RhinoGeometry;
                }
            }
            else
            {
                return CustomType.RhinoGeometry;
            }
        }
        

        /// <summary>
        /// Creates an object with all the Rhino Object data organized in properties and parameters.
        /// </summary>
        /// <param name="id">Id of the object.</param>
        /// <returns></returns>
        public static ObjectDataStruct GetObjectData(Guid id)
        {
            Rhino.DocObjects.RhinoObject rhobj = Rhino.RhinoDoc.ActiveDoc.Objects.FindId(id);

            // PROPERTIES

            List<DataEntry> properties = new List<DataEntry>();
            
            CustomType type = GetCustomType(id);

            properties.Add(new DataEntry("Type", type.ToString(), "General"));
            properties.Add(new DataEntry("Name", rhobj.Name ?? "", "General"));

            // Only if it is a VisualARQ product.
            if (IsProduct(id))
            {
                properties.Add(new DataEntry("Style", GetStyleName(GetProductStyle(id)), "General"));
                properties.Add(new DataEntry("Tag", GetElementTag(id), "General"));
            }

            if (type == CustomType.Wall)
            {
                properties.Add(new DataEntry("Thickness", GetWallThickness(id), "Geometry"));
                properties.Add(new DataEntry("Height", GetWallHeight(id), "Geometry"));
                properties.Add(new DataEntry("Alignment", GetWallAlignment(id), "Location"));
            }
            else if  (type == CustomType.Beam)
            {
                properties.Add(new DataEntry("Load bearing", IsBeamLoadBearing(id), "General"));
                // TODO: Volume
                // TODO: Length
                properties.Add(new DataEntry("Profile", GetBeamProfile(id), "Geometry"));
                properties.Add(new DataEntry("Start Cut Plane", GetBeamStartCutType(id), "Geometry"));
                properties.Add(new DataEntry("End Cut Plane", GetBeamEndCutType(id), "Geometry"));
                properties.Add(new DataEntry("Path Curve", GetBeamPathCurve(id), "Location"));
                properties.Add(new DataEntry("Alignment", GetBeamProfileAlignment(id), "Profile"));
                properties.Add(new DataEntry("Alignment Offset", GetBeamProfileAlignmentOffset(id), "Profile"));
                properties.Add(new DataEntry("Roll", GetBeamProfileRotation(id), "Profile"));
            }
            else if (type == CustomType.Column)
            {
                properties.Add(new DataEntry("Height", GetColumnHeight(id), "Geometry"));
                properties.Add(new DataEntry("Insert Point", GetColumnPosition(id), "Location"));
                properties.Add(new DataEntry("Rotation", GetColumnRotation(id), "Profile"));
            }
            else if (type == CustomType.Door || type == CustomType.Window)
            {
                // TODO: Volume
                // TODO: Area
                properties.Add(new DataEntry("Profile", GetProfileName(GetOpeningProfile(id)), "Geometry"));
                properties.Add(new DataEntry("Cut Depth", GetOpeningCutDepth(id), "Geometry"));
                properties.Add(new DataEntry("Position", GetOpeningPosition(id), "Location"));
                properties.Add(new DataEntry("Center Point", GetOpeningCenterPoint(id), "Location"));
                properties.Add(new DataEntry("Alignment", GetOpeningHostAlignment(id), "Location"));
                properties.Add(new DataEntry("Alignment Offset", GetOpeningHostAlignmentOffset(id), "Location"));
                properties.Add(new DataEntry("Horizontal Alignment", GetOpeningHorizontalAlignment(id), "Location"));
                properties.Add(new DataEntry("Vertical Alignment", GetOpeningVerticalAlignment(id), "Location"));
                // TODO: Elevation
                properties.Add(new DataEntry("Opening Side", GetOpeningSide(id), "Opening"));
                properties.Add(new DataEntry("Max Aperture (%)", GetOpeningMaxAperture(id), "Opening"));
                properties.Add(new DataEntry("Aperture (%)", GetOpeningAperture(id), "Opening"));
            }
            else if (type == CustomType.Slab)
            {
                // TODO...
            }
            else if (type == CustomType.Furniture)
            {
                // TODO...
            }
            else if (type == CustomType.GenericElement)
            {
                // TODO...
            }

            // PARAMETERS

            List<DataEntry> parameters = new List<DataEntry>();

            Guid[] paramIds = GetAllObjectParameterIds(id, false);
            foreach (Guid paramId in paramIds)
                parameters.Add(new DataEntry(GetParameterName(paramId), GetParameterValue(paramId, id), GetParameterCategory(paramId)));

            return new ObjectDataStruct(properties, parameters);
        }


        /// <summary>
        /// Creates an object with all the VisualARQ style data organized in properties and parameters.
        /// </summary>
        /// <param name="id">Id of the VisualARQ style.</param>
        /// <returns></returns>
        public static ObjectDataStruct GetStyleData(Guid id)
        {
            // PROPERTIES

            List<DataEntry> properties = new List<DataEntry>();

            properties.Add(new DataEntry("Name", GetStyleName(id), "General"));
            
            if (IsOpeningStyle(id))
            {
                properties.Add(new DataEntry("Aperture Type", GetOpeningStyleApertureType(id), "Type"));
                properties.Add(new DataEntry("Profile Shape", GetProfileName(GetOpeningStyleProfileTemplate(id)), "Geometry"));
            }
            
            // TODO missing properties...

            // PARAMETERS

            List<DataEntry> parameters = new List<DataEntry>();

            Guid[] paramIds = GetAllObjectParameterIds(id, false);
            foreach (Guid paramId in paramIds)
                parameters.Add(new DataEntry(GetParameterName(paramId), GetParameterValue(paramId, id), GetParameterCategory(paramId)));

            return new ObjectDataStruct(properties, parameters);
        }


        /// <summary>
        /// Creates a collection with all the data organized in object types.
        /// </summary>
        /// <param name="objectsGuids"></param>
        /// <param name="objectsData"></param>
        public static void GetProjectData(Dictionary<CustomType, List<Guid>> objectsGuids, out Dictionary<CustomType, ObjectTypeDataStruct> objectsData)
        {
            objectsData = new Dictionary<CustomType, ObjectTypeDataStruct>();

            foreach (KeyValuePair<CustomType, List<Guid>> categoryGuids in objectsGuids)
            {
                // The instances data.
                Dictionary<Guid, ObjectDataStruct> categoryInstancesData = new Dictionary<Guid, ObjectDataStruct>();
                
                if (categoryGuids.Key == CustomType.RhinoGeometry)
                {
                    foreach (Guid id in categoryGuids.Value)
                        categoryInstancesData.Add(id, GetObjectData(id));

                    // Add all the collected category data to the main Dictionary.
                    objectsData.Add(categoryGuids.Key, new ObjectTypeDataStruct { instances = categoryInstancesData });
                }
                else
                {
                    // Store all the unique id of the styles used in that category.
                    List<Guid> categoryStyles = new List<Guid>();

                    foreach (Guid id in categoryGuids.Value)
                    {
                        Guid styleId = GetProductStyle(id);

                        // Add the styleId.
                        if (!categoryStyles.Contains(styleId))
                            categoryStyles.Add(styleId);

                        categoryInstancesData.Add(id, GetObjectData(id));
                    }

                    // The styles data.
                    Dictionary<Guid, ObjectDataStruct> categoryStylesData = new Dictionary<Guid, ObjectDataStruct>();

                    foreach (Guid id in categoryStyles)
                        categoryStylesData.Add(id, GetStyleData(id));

                    // Add all the collected category data to the main Dictionary.
                    objectsData.Add(categoryGuids.Key, new ObjectTypeDataStruct(categoryInstancesData, categoryStylesData));
                }
            }
        }


        /// <summary>
        /// This is a temporary solution until the VisualARQ API method will be developed.
        /// </summary>
        /// <param name="drawing"></param>
        /// <returns></returns>
        public static string GetDrawingTitle(Rhino.DocObjects.InstanceDefinition drawing)
        {
            // Get all the objects in the Plan View.
            Rhino.DocObjects.RhinoObject[] rhobjs = drawing.GetObjects();

            // Temporary solution since there is only one text object:
            Rhino.DocObjects.TextObject pvTitle = (Rhino.DocObjects.TextObject)Array.Find(rhobjs, o => o.ObjectType == Rhino.DocObjects.ObjectType.Annotation);

            return pvTitle.TextGeometry.PlainText;
        }
    }
}
