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

                string folderPath;

                // Show the FolderBrowserDialog.
                DialogResult result = fbd.ShowDialog();
                if (result == DialogResult.OK)
                {
                    folderPath = fbd.SelectedPath;
                    RhinoApp.WriteLine(folderPath);

                    string folderName = "VisualARQReviewArchive";
                    Result rc = Rhino.Input.RhinoGet.GetString("Folder name", false, ref folderName);
                    if (rc != Result.Success)
                        return rc;

                    string newFolderPath = Path.Combine(folderPath, folderName);

                    RhinoApp.WriteLine(newFolderPath);

                    // TODO Check if there is a folder with this name
                    if (!Directory.Exists(newFolderPath))
                    {
                        Directory.CreateDirectory(newFolderPath);

                        // Create the drawings directory.
                        Directory.CreateDirectory(Path.Combine(newFolderPath, "drawings"));
                        // Create the elements data directory.
                        Directory.CreateDirectory(Path.Combine(newFolderPath, "elementsData"));

                        // TODO Populate the folders...

                        // All SVG docs will be stored here.
                        List<XmlDocument> svgDocs = new List<XmlDocument>();

                        foreach (Rhino.DocObjects.InstanceDefinition pv in planViewsDefs)
                        {
                            SVGWriter svg = new SVGWriter();

                            // Get all the objects in the Plan View.
                            Rhino.DocObjects.RhinoObject[] rhobjs = pv.GetObjects();
                            // TODO: here the name...

                            // Create the SVG with this objects and add it to the list of SVG docs.
                            svgDocs.Add(svg.CreateSVG(RhinoDoc.ActiveDoc, rhobjs));
                        }

                        // Store all the data for each object.
                        Dictionary<Guid, ExpandoObject> instancesData = new Dictionary<Guid, ExpandoObject>();

                        // Store all the data for each style.
                        Dictionary<Guid, ExpandoObject> stylesData = new Dictionary<Guid, ExpandoObject>();

                        Utilities.GetProjectData(objectsGuids, out instancesData, out stylesData);

                        // Data to JSON
                        string instancesJsonData = JsonConvert.SerializeObject(instancesData, Newtonsoft.Json.Formatting.Indented); // Newtonsoft.Json.Formatting.None
                        string stylesJsonData = JsonConvert.SerializeObject(stylesData, Newtonsoft.Json.Formatting.Indented);

                        // TODO: Using the folder path save the files.
                        // TEMP
                        //string directory = Path.GetDirectoryName(sfd.FileName);

                        // Create the JSON file.
                        //File.WriteAllText(Path.Combine(directory + "\\instancesData.json"), instancesJsonData);
                        //File.WriteAllText(Path.Combine(directory + "\\stylesData.json"), stylesJsonData);

                        // Old option
                        //foreach (XmlDocument svgDoc in svgDocs)
                        //{
                        //    svgDoc.Save(Path.GetFullPath(sfd.FileName));
                        //}

                        // Current option
                        //for (int i = 0; i < svgDocs.Count; i++)
                        //{
                        //    svgDocs[i].Save(Path.Combine(directory, i + "drawing.svg"));
                        //}
                    }
                    else
                    {
                        RhinoApp.WriteLine("There is already a folder with this name.");
                        return Result.Failure;
                    }




                    return Result.Success;
                }
                else
                {
                    return Result.Nothing;
                }
            }
            else if (selected_index == 1)
            {
                // Save single file dialog.
                SaveFileDialog sfd = new SaveFileDialog();

                sfd.Filter = "json files (*.json)|*.json|All files (*.*)|*.*";
                // sfd.FilterIndex = 2;
                // sfd.RestoreDirectory = true;

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    // The file name.
                    string sfdname = sfd.FileName;

                    // All SVG docs will be stored here.
                    Dictionary<string, string> svgDrawings = new Dictionary<string, string>();

                    // Store the drawings names to avoid duplicates.
                    List<string> drawingsTitles = new List<string>();

                    foreach (Rhino.DocObjects.InstanceDefinition pv in planViewsDefs)
                    {
                        SVGWriter svg = new SVGWriter();

                        // Get all the objects in the Plan View.
                        Rhino.DocObjects.RhinoObject[] rhobjs = pv.GetObjects();
                        // TODO: Remove the border and the label of the plan view.

                        // TODO: Get the Title of a the Plan View with the API, here or above on the Plan View Instance.
                        // Temporary solution since there is only one text object:
                        Rhino.DocObjects.TextObject pvTitle = (Rhino.DocObjects.TextObject)Array.Find(rhobjs, o => o.ObjectType == Rhino.DocObjects.ObjectType.Annotation);
                        string drawingTitle = pvTitle.TextGeometry.PlainText;
                        List<string> titlesMatched = drawingsTitles.FindAll(dt => dt == drawingTitle);
                        drawingsTitles.Add(drawingTitle);
                        if (titlesMatched.Count > 0)
                            drawingTitle += (" (" + titlesMatched.Count + ")");

                        RhinoApp.WriteLine(drawingTitle);
                        
                        // Create the SVG and store it as string.
                        svgDrawings.Add(drawingTitle, svg.CreateSVG(RhinoDoc.ActiveDoc, rhobjs).OuterXml);
                    }
                    
                    // Store all the data for each object.
                    Dictionary<Guid, ExpandoObject> instancesData = new Dictionary<Guid, ExpandoObject>();

                    // Store all the data for each style.
                    Dictionary<Guid, ExpandoObject> stylesData = new Dictionary<Guid, ExpandoObject>();

                    Utilities.GetProjectData(objectsGuids, out instancesData, out stylesData);

                    // A single JSON file.
                    Dictionary<string, string> projInfo = new Dictionary<string, string>();
                    projInfo.Add("title", "Villa S");
                    FileTemplate reviewFile = new FileTemplate(projInfo, svgDrawings, instancesData);
                    File.WriteAllText(Path.Combine(sfdname), JsonConvert.SerializeObject(reviewFile, Newtonsoft.Json.Formatting.Indented)); // directory + "\\archive.json"

                    

                    return Result.Success;
                }
                else
                {
                    return Result.Nothing;
                }
            }
            else
            {
                return Result.Nothing;
            }
        }
    }
}
