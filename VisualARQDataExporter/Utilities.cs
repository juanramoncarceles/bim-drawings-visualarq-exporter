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
                // TODO use this... VisualARQ.Script.IsProduct()
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

            objData.Type = GetCustomType(id);

            objData.Name = rhobj.Name;

            objData.Style = GetProductStyle(id);


            //GetAllObjectParameterIds(id, false);

            return objData;
        }
    }
}
