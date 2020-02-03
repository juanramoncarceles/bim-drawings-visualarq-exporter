using System;
using System.Dynamic;
using static VisualARQ.Script;

namespace VisualARQDataExporter
{
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
        /// 
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

            // If it is a VisualARQ product.
            objData.Style = GetProductStyle(id);

            if (type == "wall")
            {
                objData.Height = GetWallHeight(id);
                objData.Thickness = GetWallThickness(id);
                objData.Alignment = GetWallAlignment(id);
            }
            else if (type == "column")
            {
                objData.Height = GetColumnHeight(id);
                objData.InsertPoint = GetColumnPosition(id);
                objData.Rotation = GetColumnRotation(id);
            }
            
            //GetAllObjectParameterIds(id, false);

            return objData;
        }

        public static ExpandoObject GetStyleData(Guid id)
        {
            dynamic styleData = new ExpandoObject();

            styleData.Name = GetStyleName(id);
            //...

            return styleData;
        }
    }
}
