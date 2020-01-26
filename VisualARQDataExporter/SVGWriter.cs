using System;
using System.Collections;
using System.Collections.Generic;

using Rhino;
using Rhino.Geometry;

using System.Xml;

public class SVGWriter
{
    public bool AlsoHidden;
    public Vector3d DirectionOfProjection;
    public int Digits;
    public RhinoDoc doc;
    public Point2d svgOrigin;
    double height;
    double width;
    public XmlDocument xmlDoc;

    public XmlDocument CreateSVG(RhinoDoc _doc, Rhino.DocObjects.RhinoObject[] rhobjs)
    {
        this.doc = _doc;
        this.DirectionOfProjection = new Vector3d(0, 0, 1.0);
        this.Digits = 2;
        this.AlsoHidden = false;

        BoundingBox rhobjsBBox = this.GetBoundingBox(rhobjs);

        this.CalculateViewBox(rhobjsBBox);

        // Classify objects by its source object.
        // All objects from the same source object are placed under the same list.
        IDictionary<Guid, List<Rhino.DocObjects.RhinoObject>> rhobjsGroups = new Dictionary<Guid, List<Rhino.DocObjects.RhinoObject>>();
        foreach (Rhino.DocObjects.RhinoObject rhobj in rhobjs)
        {
            Guid myTestGuid = Guid.NewGuid(); // Guid sourceGuid = VisualARQ.Script.vaGetSourceObjectId(rhobj.Id);
            // TODO for now repeat the same guid for all the items

            if (rhobjsGroups.ContainsKey(myTestGuid))
            {
                rhobjsGroups[myTestGuid].Add(rhobj);
            }
            else
            {
                List<Rhino.DocObjects.RhinoObject> rhobjsGroup = new List<Rhino.DocObjects.RhinoObject> { rhobj };
                rhobjsGroups.Add(myTestGuid, rhobjsGroup);
            }
        }

        // TODO for each key value pair join the sectioned curves.

        return this.WriteSVG(rhobjsGroups);
    }

    public BoundingBox GetBoundingBox(Rhino.DocObjects.RhinoObject[] rhobjs)
    {
        BoundingBox bb = BoundingBox.Empty;
        if (rhobjs != null && rhobjs.Length > 0)
        {
            foreach (Rhino.DocObjects.RhinoObject rhobj in rhobjs)
            {
                if (this.AlsoHidden || (!rhobj.IsHidden)) // this.AlsoHidden || (!rhobj.IsHidden && this.doc.Layers[rhobj.Attributes.LayerIndex].IsVisible)
                {
                    Rhino.DocObjects.ObjectType type = rhobj.Geometry.ObjectType;
                    if (type == Rhino.DocObjects.ObjectType.Curve || type == Rhino.DocObjects.ObjectType.Hatch) // || type == ObjectType.Point || type == ObjectType.Annotation
                    {
                        bb.Union(rhobj.Geometry.GetBoundingBox(false));
                    }
                }
            }
        }
        return bb;
    }

    // Sets the main values for the svg ViewBox.
    public void CalculateViewBox(BoundingBox bb)
    {
        Point3d pt_min = new Point3d(0, 0, 0);
        Point3d pt_max = new Point3d(0, 0, 0);
        if (bb.IsValid)
        {
            pt_min = new Point3d(bb.Min);
            pt_max = new Point3d(bb.Max);
            this.width = Math.Abs(pt_max.X - pt_min.X);
            this.height = Math.Abs(pt_max.Y - pt_min.Y);
            if (this.width == 0.0) this.width = 10.0;
            if (this.height == 0.0) this.height = 10.0;
        }
        else
        {
            this.width = 10.0;
            this.height = 10.0;
        }
        this.svgOrigin = new Point2d(Math.Min(pt_min.X, pt_max.X), Math.Max(pt_min.Y, pt_max.Y));
    }

    public System.Drawing.PointF RhinoToSvgPt(Point3d rhinoPt)
    {
        double x, y;
        x = Math.Round(rhinoPt.X - this.svgOrigin.X, this.Digits);
        y = Math.Round(this.svgOrigin.Y - rhinoPt.Y, this.Digits);
        return new System.Drawing.PointF((float)x, (float)y);
    }

    // ******************** CURVE **************************
    public XmlNode WriteSVGCurve(XmlDocument xmlDoc, Curve curve, Rhino.DocObjects.RhinoObject rhobj)
    {
        if (curve.IsLinear())
        {
            return this.WriteSVGLine(xmlDoc, curve, rhobj);
        }
        else
        {
            if (curve.TryGetPolyline(out Polyline polyline))
            {
                return this.WriteSVGPolyline(xmlDoc, polyline, rhobj);
            }
            else
            {
                if (curve.TryGetCircle(out Circle circle))
                {
                    if (Vector3d.Multiply(circle.Normal, DirectionOfProjection) < 0.9999)
                    {
                        return this.WriteSVGBezier(xmlDoc, curve, rhobj);
                    }
                    else
                    {
                        return WriteSVGCircle(xmlDoc, circle, rhobj);
                    }
                }
                else
                {
                    return this.WriteSVGBezier(xmlDoc, curve, rhobj);
                }
            }
        }
    }

    // ******************** LINE **************************
    public XmlNode WriteSVGLine(XmlDocument xmlDoc, Curve curve, Rhino.DocObjects.RhinoObject rhobj)
    {
        XmlNode userNode = xmlDoc.CreateElement("line");

        System.Drawing.PointF startPt = this.RhinoToSvgPt(curve.PointAtStart);

        XmlAttribute attribute = xmlDoc.CreateAttribute("x1");
        attribute.Value = startPt.X.ToString();
        userNode.Attributes.Append(attribute);

        attribute = xmlDoc.CreateAttribute("y1");
        attribute.Value = startPt.Y.ToString();
        userNode.Attributes.Append(attribute);

        System.Drawing.PointF endPt = this.RhinoToSvgPt(curve.PointAtEnd);

        attribute = xmlDoc.CreateAttribute("x2");
        attribute.Value = endPt.X.ToString();
        userNode.Attributes.Append(attribute);

        attribute = xmlDoc.CreateAttribute("y2");
        attribute.Value = endPt.Y.ToString();
        userNode.Attributes.Append(attribute);

        attribute = xmlDoc.CreateAttribute("style");
        attribute.Value = this.CreateStyleAttribute(rhobj, false);
        userNode.Attributes.Append(attribute);

        return userNode;
    }

    // ******************** POLYLINE **************************
    public XmlNode WriteSVGPolyline(XmlDocument xmlDoc, Polyline curve, Rhino.DocObjects.RhinoObject rhobj)
    {
        // TODO: This should be if polyline is closed use polygon element otherwise polyline element?
        // TODO: logic here...

        XmlNode userNode = xmlDoc.CreateElement("path");

        XmlAttribute attribute = xmlDoc.CreateAttribute("d");
        attribute.Value = "M0,0 L10,10";
        userNode.Attributes.Append(attribute);

        // attribute = xmlDoc.CreateAttribute("style");
        // attribute.Value = this.CreateStyleAttribute(rhobj, curve.IsClosed); // polyline open but polygon closed
        // userNode.Attributes.Append(attribute);

        return userNode;
    }

    // ******************** CIRCLE **************************
    public XmlNode WriteSVGCircle(XmlDocument xmlDoc, Circle circle, Rhino.DocObjects.RhinoObject rhobj)
    {
        XmlNode userNode = xmlDoc.CreateElement("circle");

        System.Drawing.PointF center = this.RhinoToSvgPt(circle.Center);

        XmlAttribute attribute = xmlDoc.CreateAttribute("cx");
        attribute.Value = center.X.ToString();
        userNode.Attributes.Append(attribute);

        attribute = xmlDoc.CreateAttribute("cy");
        attribute.Value = center.Y.ToString();
        userNode.Attributes.Append(attribute);

        attribute = xmlDoc.CreateAttribute("r");
        attribute.Value = Math.Round(circle.Radius, this.Digits).ToString();
        userNode.Attributes.Append(attribute);

        attribute = xmlDoc.CreateAttribute("style");
        attribute.Value = this.CreateStyleAttribute(rhobj, true);
        userNode.Attributes.Append(attribute);

        return userNode;
    }

    // ******************** BEZIER CURVE **************************
    public XmlNode WriteSVGBezier(XmlDocument xmlDoc, Curve curve, Rhino.DocObjects.RhinoObject rhobj)
    {
        BezierCurve[] beziers = BezierCurve.CreateCubicBeziers(curve, this.doc.ModelAbsoluteTolerance, this.doc.ModelAbsoluteTolerance);
        System.Drawing.PointF controlVertex = this.RhinoToSvgPt(beziers[0].GetControlVertex3d(0));
        String poly = String.Format("M{0},{1} ", controlVertex.X, controlVertex.Y);
        for (int i = 0; i < beziers.Length; i++)
        {
            System.Drawing.PointF controlVertex1 = this.RhinoToSvgPt(beziers[i].GetControlVertex3d(1));
            poly += String.Format("C{0},{1} ", controlVertex1.X, controlVertex1.Y);
            System.Drawing.PointF controlVertex2 = this.RhinoToSvgPt(beziers[i].GetControlVertex3d(2));
            poly += String.Format("{0},{1} ", controlVertex2.X, controlVertex2.Y);
            System.Drawing.PointF controlVertex3 = this.RhinoToSvgPt(beziers[i].GetControlVertex3d(3));
            poly += String.Format("{0},{1} ", controlVertex3.X, controlVertex3.Y);
        }
        if (curve.IsClosed) poly += "z";

        XmlNode userNode = xmlDoc.CreateElement("path");

        XmlAttribute attribute = xmlDoc.CreateAttribute("d");
        attribute.Value = poly;
        userNode.Attributes.Append(attribute);
        
        attribute = xmlDoc.CreateAttribute("style");
        attribute.Value = this.CreateStyleAttribute(rhobj, curve.IsClosed);
        userNode.Attributes.Append(attribute);

        return userNode;
    }

    // ******************** HATCH **************************
    // TODO...

    // ******************** TEXT **************************
    // TODO...

    // ******************** STYLE ATTRIBUTE **************************
    public string CreateStyleAttribute(Rhino.DocObjects.RhinoObject rhobj, bool is_closed)
    {
        List<string> styleAttributes = new List<string>();

        // Stroke color
        styleAttributes.Add("stroke:");
        styleAttributes.Add(String.Format("rgb({0},{1},{2})", new object[] { rhobj.Attributes.ObjectColor.R, rhobj.Attributes.ObjectColor.G, rhobj.Attributes.ObjectColor.B }));

        // Stroke width
        styleAttributes.Add(";stroke-width:");
        if (rhobj.Attributes.PlotWeight == 0) // Is the "Default" value.
            styleAttributes.Add("1");
        else if (rhobj.Attributes.PlotWeight == -1) // Is the "No print" value.
            styleAttributes.Add("1");
        else
            styleAttributes.Add(Math.Round(rhobj.Attributes.PlotWeight, this.Digits).ToString());

        // Fill color
        styleAttributes.Add(";fill:");
        if (!is_closed)
        {
            styleAttributes.Add("none");
        }
        else
        {
            // TODO: since there are only curves and hatch maybe is for hatch ?
            // For now always none...
            styleAttributes.Add("none");
        }

        return String.Join("", styleAttributes);
    }

    // ******************** WRITE SVG **************************
    public XmlDocument WriteSVG(IDictionary<Guid, List<Rhino.DocObjects.RhinoObject>> rhobjsGroups)
    {
        xmlDoc = null;
        xmlDoc = new XmlDocument();
        // In order to not go on internet, save time but no validation.
        xmlDoc.XmlResolver = null;
        XmlDeclaration xmlDeclaration = xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", "no");
        XmlElement root = xmlDoc.DocumentElement;
        xmlDoc.InsertBefore(xmlDeclaration, root);

        // XmlDocumentType doctype = xmlDoc.CreateDocumentType("svg", "-//W3C//Dtd SVG 1.1//EN", "http://www.w3.org/Graphics/SVG/1.1/Dtd/svg11.dtd", null);
        // xmlDoc.AppendChild(doctype);

        XmlNode rootNode = xmlDoc.CreateElement("svg");
        xmlDoc.AppendChild(rootNode);

        XmlAttribute attribute = xmlDoc.CreateAttribute("xmlns");
        attribute.Value = "http://www.w3.org/2000/svg";
        rootNode.Attributes.Append(attribute);

        attribute = xmlDoc.CreateAttribute("viewBox");

        attribute.Value = "0 0 " + Math.Round(this.width, this.Digits) + " " + Math.Round(this.height, this.Digits);
        rootNode.Attributes.Append(attribute);

        String name;
        if (this.doc.Name == null)
            name = "SVG of Rhinoceros 6 file untitled.3dm";
        else
            name = String.Format("SVG of Rhinoceros 6 file _{0}_", this.doc.Name);
        XmlNode title = xmlDoc.CreateComment(name);
        rootNode.AppendChild(title);

        title = xmlDoc.CreateComment("Export Rhino to SVG, version 0.1, Ramon Carceles email:juanramoncarceles[at]gmail.com");
        rootNode.AppendChild(title);

        // List used to store the xml nodes corresponding to each group. All will be appended to rootNode.
        // List<XmlNode> listOfXmlNodes = new List<XmlNode>();

        foreach(KeyValuePair<Guid, List<Rhino.DocObjects.RhinoObject>> rhobjsGroup in rhobjsGroups)
        {
            if (rhobjsGroup.Value != null && rhobjsGroup.Value.Count > 0)
            {
                XmlNode node = xmlDoc.CreateElement("g");
        
                // listOfXmlNodes.Add(node);
                // Custom attribute to allow selection of the element in the app.
                attribute = xmlDoc.CreateAttribute("selectable");
                attribute.Value = "";
                node.Attributes.Append(attribute);
                // The id of the element.
                attribute = xmlDoc.CreateAttribute("data-id");
                attribute.Value = rhobjsGroup.Key.ToString();
                node.Attributes.Append(attribute);
                // The category of the element.
                attribute = xmlDoc.CreateAttribute("data-category");
                attribute.Value = "the-category-of-the-object";
                node.Attributes.Append(attribute);

                rootNode.AppendChild(node);

                XmlNode nodeTest = null;

                foreach (Rhino.DocObjects.RhinoObject rhobj in rhobjsGroup.Value)
                {
                    switch ((int)rhobj.Geometry.ObjectType)
                    {
                        // ********** CURVE **********
                        case (int)Rhino.DocObjects.ObjectType.Curve:
                            Curve curve = ((Curve)(rhobj.Geometry));
                            nodeTest = this.WriteSVGCurve(this.xmlDoc, curve, rhobj);
                            if (nodeTest != null) node.AppendChild(nodeTest);
                            break;
                        // ********** HATCH ***********
                        case (int)Rhino.DocObjects.ObjectType.Hatch:
                            Hatch hatch = rhobj.Geometry as Hatch;
                            GeometryBase[] hatches = hatch.Explode();
                            // nodeTest = this.WriteSVGHatch(xmlDoc, hatches, rhobj, layerId);
                            // if (nodeTest != null) node.AppendChild(nodeTest);
                            break;
                            // TODO: Other types like text?
                    }
                }
            }
        }

        return xmlDoc;
    }
}