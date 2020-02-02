using System;
using System.Collections.Generic;
using Rhino;
using Rhino.Commands;
using Rhino.Geometry;
using Rhino.Input.Custom;
using static VisualARQ.Script;
using System.Xml;
using System.Windows.Forms;
using System.IO;
using System.Dynamic;
using Newtonsoft.Json;

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

        public static List<Guid> objectsGuids = new List<Guid>();

        /// <summary>
        /// Determines if the passed Rhino object is a plan view or a section view or not.
        /// </summary>
        /// <param name="rhObject"></param>
        /// <param name="geometry"></param>
        /// <param name="componentIndex"></param>
        /// <returns></returns>
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

            // Export options.
            GetOption gopt = new GetOption();
            gopt.SetCommandPrompt("Output mode");
            List<int> opt_list = new List<int>();

            int opt_index = gopt.AddOption("SingleFile");
            opt_list.Add(opt_index);
            opt_index = gopt.AddOption("FolderStructure");
            opt_list.Add(opt_index);

            // Get the command option.
            gopt.Get();
            if (gopt.CommandResult() != Result.Success)
                return gopt.CommandResult();

            int selected_index = gopt.Option().Index;

            RhinoApp.WriteLine(selected_index.ToString());

            if (selected_index == 2)
            {
                // Browse for a folder.
                FolderBrowserDialog fbd = new FolderBrowserDialog();

                string folderName;

                // Show the FolderBrowserDialog.
                DialogResult result = fbd.ShowDialog();
                if (result == DialogResult.OK)
                {
                    folderName = fbd.SelectedPath;
                    RhinoApp.WriteLine(folderName);
                }
            }

            // Save single file dialog.
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
                    SVGWriter svg = new SVGWriter();

                    // Get all the objects in the Plan View.
                    Rhino.DocObjects.RhinoObject[] rhobjs = pv.GetObjects();
                    // TODO: Remove the border and the label of the plan view.

                    // Create the SVG with this objects and add it to the list of SVG docs.
                    svgDocs.Add(svg.CreateSVG(RhinoDoc.ActiveDoc, rhobjs));
                }

                // Store all the unique id of the styles used.
                List<Guid> styleGuids = new List<Guid>();

                // Store all the data for each object.
                Dictionary<Guid, ExpandoObject> instancesData = new Dictionary<Guid, ExpandoObject>();

                foreach (Guid id in objectsGuids)
                {
                    Guid styleId = GetProductStyle(id);

                    // Add the styleId.
                    if (styleGuids.Contains(styleId))
                    {
                        styleGuids.Add(styleId);
                    }

                    instancesData.Add(id, Utilities.GetObjectData(id));
                }

                // TODO: also get the data for each style used. Maybe store also in a Dictionary<Guid, ExpandoObject>
                foreach (Guid id in styleGuids)
                {
                    //... get styles data.
                }

                string json = JsonConvert.SerializeObject(instancesData, Newtonsoft.Json.Formatting.None);

                RhinoApp.WriteLine(json);

                // TEMP
                string directory = Path.GetDirectoryName(sfd.FileName);
                
                //foreach (XmlDocument svgDoc in svgDocs)
                //{
                //    svgDoc.Save(Path.GetFullPath(sfd.FileName));
                //}
                for (int i = 0; i < svgDocs.Count; i++)
                {
                    svgDocs[i].Save(Path.Combine(directory, i + "drawing.svg"));
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
