using System;
using System.Collections.Generic;
using Rhino;
using Rhino.Commands;
using Rhino.Geometry;
using Rhino.Input;
using Rhino.Input.Custom;
using VisualARQ;
using System.Xml;
using System.Windows.Forms;
using System.IO;

namespace VisualARQDataExporter
{
    public class VisualARQDataExporterCommand : Command
    {
        public VisualARQDataExporterCommand()
        {
            // Rhino only creates one instance of each command class defined in a
            // plug-in, so it is safe to store a refence in a static property.
            Instance = this;
        }

        ///<summary>The only instance of this command.</summary>
        public static VisualARQDataExporterCommand Instance
        {
            get; private set;
        }

        
        ///<returns>The command name as it appears on the Rhino command line.</returns>
        public override string EnglishName
        {
            get { return "VisualARQDataExporterCommand"; }
        }


        private bool PlanAndSectionViews(Rhino.DocObjects.RhinoObject rhObject, GeometryBase geometry, ComponentIndex componentIndex)
        {
            //if (VisualARQ.Script.IsPlanView(rhObject.Id) || VisualARQ.Script.IsSectionView(rhObject.Id))
            //{
            //    return true;
            //}
            //else
            //{
            //    return false;
            //}
            return true;
        }

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            
            GetObject go = new GetObject();

            go.SetCommandPrompt("Select Plan Views");

            go.GeometryFilter = Rhino.DocObjects.ObjectType.InstanceReference;

            go.SetCustomGeometryFilter(PlanAndSectionViews);

            go.DisablePreSelect();

            go.SubObjectSelect = false;

            go.GetMultiple(1, 0);

            if (go.CommandResult() != Result.Success)
                return go.CommandResult();

            List<InstanceDefinitionGeometry> planViewsDefs = new List<InstanceDefinitionGeometry>();

            for (int i = 0; i < go.ObjectCount; i++)
            {
                Rhino.DocObjects.InstanceObject planViewInstance = (Rhino.DocObjects.InstanceObject)go.Object(i).Object();

                if (planViewInstance != null)
                {
                    // Get the instance definition from the instance object.
                    Rhino.DocObjects.InstanceDefinition planViewDef = planViewInstance.InstanceDefinition;
                    
                    // Skip instanceDefinitions already added.
                    if (!planViewsDefs.Exists(x => x.Id == planViewDef.Id))
                    {
                        planViewsDefs.Add(planViewDef);
                    }
                }
            }

            SaveFileDialog sfd = new SaveFileDialog();

            // sfd.Filter = "svg files (*.svg)|*.svg|All files (*.*)|*.*";
            // sfd.FilterIndex = 2;
            // sfd.RestoreDirectory = true;

            string sfdname = sfd.FileName;

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                // All SVG docs will be stored here.
                List<XmlDocument> svgDocs = new List<XmlDocument>();

                foreach (Rhino.DocObjects.InstanceDefinition pv in planViewsDefs)
                {
                    // RhinoApp.WriteLine(pv.Id.ToString());

                    SVGWriter svg = new SVGWriter();

                    // Get all the objects in the Plan View.
                    Rhino.DocObjects.RhinoObject[] rhobjs = pv.GetObjects();
                    // TODO: Remove the border and the label of the plan view.

                    // Create the SVG with this objects and add it to the list of SVG docs.
                    svgDocs.Add(svg.CreateSVG(RhinoDoc.ActiveDoc, rhobjs));
                }

                foreach (XmlDocument svgDoc in svgDocs)
                {
                    svgDoc.Save(Path.GetFullPath(sfd.FileName));
                }

                return Result.Success;
            }
            else
            {
                return Result.Nothing;
            }
        }
    }
}
