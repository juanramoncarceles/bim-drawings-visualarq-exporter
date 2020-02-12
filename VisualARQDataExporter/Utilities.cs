using System;
using System.Collections.Generic;
using System.Dynamic;
using static VisualARQ.Script;

namespace VisualARQDataExporter
{
    public class CategoryData
    {
        public CategoryData(Dictionary<Guid, ExpandoObject> Instances, Dictionary<Guid, ExpandoObject> Styles)
        {
            instances = Instances;
            styles = Styles;
        }

        public Dictionary<Guid, ExpandoObject> instances;
        public Dictionary<Guid, ExpandoObject> styles;
    }


    public static class Utilities
    {
        public static string GetCustomType(Guid id)
        {
            Rhino.DocObjects.RhinoObject sourceObj = Rhino.RhinoDoc.ActiveDoc.Objects.FindId(id);

            if (sourceObj.ObjectType == Rhino.DocObjects.ObjectType.InstanceReference)
            {
                if (IsProduct(id))
                {
                    if (IsWall(id))
                    {
                        return "wall";
                    }
                    // TODO: missing IsCurtainWall(id)
                    else if (IsBeam(id))
                    {
                        return "beam";
                    }
                    else if (IsColumn(id))
                    {
                        return "column";
                    }
                    else if (IsDoor(id))
                    {
                        return "door";
                    }
                    else if (IsWindow(id))
                    {
                        return "window";
                    }
                    // TODO: missing IsStair(id)
                    // TODO: missing IsRailing(id)
                    else if (IsSlab(id))
                    {
                        return "slab";
                    }
                    // TODO: missing IsRoof(id)
                    else if (IsFurniture(id))
                    {
                        return "furniture";
                    }
                    else if (IsElement(id))
                    {
                        return "element";
                    }
                    else
                    {
                        return "unknown";
                    }
                }
                else
                {
                    return "rhinoobj";
                }
            }
            else
            {
                return "rhinoobj";
            }
        }


        /// <summary>
        /// Creates an object with all the instance data.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static ExpandoObject GetObjectData(Guid id)
        {
            Rhino.DocObjects.RhinoObject rhobj = Rhino.RhinoDoc.ActiveDoc.Objects.FindId(id);

            dynamic objData = new ExpandoObject();

            string type = GetCustomType(id);

            objData.Type = GetCustomType(id);

            objData.Name = rhobj.Name;

            objData.Tag = GetElementTag(id);

            // If it is a VisualARQ product.
            objData.Style = GetProductStyle(id);

            if (type == "wall")
            {
                objData.Height = GetWallHeight(id);
                objData.Thickness = GetWallThickness(id);
                objData.Alignment = GetWallAlignment(id);
            }
            else if  (type == "beam")
            {
                objData.LoadBearing = IsBeamLoadBearing(id);
                // TODO: Volume
                // TODO: Length
                objData.Profile = GetBeamProfile(id);
                objData.StartCut = GetBeamStartCutType(id);
                objData.EndCut = GetBeamEndCutType(id);
                objData.Path = GetBeamPathCurve(id);
                objData.ProfileAlignment = GetBeamProfileAlignment(id);
                objData.AlignmentOffset = GetBeamProfileAlignmentOffset(id);
                objData.Rotation = GetBeamProfileRotation(id);
            }
            else if (type == "column")
            {
                objData.Height = GetColumnHeight(id);
                objData.InsertPoint = GetColumnPosition(id);
                objData.Rotation = GetColumnRotation(id);
            }
            else if (type == "door" || type == "window")
            {
                // TODO: Volume
                // TODO: Area
                objData.Profile = GetProfileName(GetOpeningProfile(id));
                objData.CutDepth = GetOpeningCutDepth(id);
                objData.CenterPoint = GetOpeningCenterPoint(id);
                objData.Position = GetOpeningPosition(id);
                objData.Alignment = GetOpeningHostAlignment(id);
                objData.AlignmentOffset = GetOpeningHostAlignmentOffset(id);
                objData.HorizontalAlignment = GetOpeningHorizontalAlignment(id);
                objData.VerticalAlignment = GetOpeningVerticalAlignment(id);
                // TODO: Elevation
                objData.OpeningSide = GetOpeningSide(id);
                objData.MaxAperture = GetOpeningMaxAperture(id);
                objData.Aperture = GetOpeningAperture(id);
            }
            else if (type == "slab")
            {
                // TODO...
            }
            else if (type == "furniture")
            {
                // TODO...
            }
            else if (type == "element")
            {
                // TODO...
            }

            //GetAllObjectParameterIds(id, false);

            return objData;
        }


        /// <summary>
        /// Creates an object with all the style data.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static ExpandoObject GetStyleData(Guid id)
        {
            dynamic styleData = new ExpandoObject();

            styleData.Name = GetStyleName(id);
            //...

            return styleData;
        }


        /// <summary>
        /// Creates a collection with all the data organized in categories.
        /// </summary>
        /// <param name="objectsGuids"></param>
        /// <param name="objectsData"></param>
        public static void GetProjectData(Dictionary<string, List<Guid>> objectsGuids, out Dictionary<string, CategoryData> objectsData)
        {
            objectsData = new Dictionary<string, CategoryData>();

            foreach (KeyValuePair<string, List<Guid>> categoryGuids in objectsGuids)
            {
                // Store all the unique id of the styles used in that category.
                List<Guid> categoryStyles = new List<Guid>();

                // The instances
                Dictionary<Guid, ExpandoObject> categoryInstancesData = new Dictionary<Guid, ExpandoObject>();

                foreach (Guid id in categoryGuids.Value)
                {
                    Guid styleId = GetProductStyle(id);

                    // Add the styleId.
                    if (!categoryStyles.Contains(styleId))
                        categoryStyles.Add(styleId);

                    categoryInstancesData.Add(id, GetObjectData(id));
                }

                // The styles
                Dictionary<Guid, ExpandoObject> categoryStylesData = new Dictionary<Guid, ExpandoObject>();

                foreach (Guid id in categoryStyles)
                    categoryStylesData.Add(id, GetStyleData(id));
                
                // Add all the collected category data to the main Dictionary.
                objectsData.Add(categoryGuids.Key, new CategoryData(categoryInstancesData, categoryStylesData));
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
