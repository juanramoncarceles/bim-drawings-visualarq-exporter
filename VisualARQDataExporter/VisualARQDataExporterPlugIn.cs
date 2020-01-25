using System;
using System.Collections.Generic;
using System.IO;
using Rhino;
using Rhino.Geometry;
using VisualARQ;
using System.Xml;

namespace VisualARQDataExporter
{
    ///<summary>
    /// <para>Every RhinoCommon .rhp assembly must have one and only one PlugIn-derived
    /// class. DO NOT create instances of this class yourself. It is the
    /// responsibility of Rhino to create an instance of this class.</para>
    /// <para>To complete plug-in information, please also see all PlugInDescription
    /// attributes in AssemblyInfo.cs (you might need to click "Project" ->
    /// "Show All Files" to see it in the "Solution Explorer" window).</para>
    ///</summary>
    public class VisualARQDataExporterPlugIn : Rhino.PlugIns.FileExportPlugIn

    {
        public VisualARQDataExporterPlugIn()
        {
            Instance = this;
        }

        ///<summary>Gets the only instance of the VisualARQDataExporterPlugIn plug-in.</summary>
        public static VisualARQDataExporterPlugIn Instance
        {
            get; private set;
        }

        /// <summary>Defines file extensions that this export plug-in is designed to write.</summary>
        /// <param name="options">Options that specify how to write files.</param>
        /// <returns>A list of file types that can be exported.</returns>
        protected override Rhino.PlugIns.FileTypeList AddFileTypes(Rhino.FileIO.FileWriteOptions options)
        {
            var result = new Rhino.PlugIns.FileTypeList();
            result.AddFileType("Data of the VisualARQ objects (*.json)", "json");
            return result;
        }

        /// <summary>
        /// Is called when a user requests to export a ."json file.
        /// It is actually up to this method to write the file itself.
        /// </summary>
        /// <param name="filename">The complete path to the new file.</param>
        /// <param name="index">The index of the file type as it had been specified by the AddFileTypes method.</param>
        /// <param name="doc">The document to be written.</param>
        /// <param name="options">Options that specify how to write file.</param>
        /// <returns>A value that defines success or a specific failure.</returns>
        /// 
        protected override Rhino.PlugIns.WriteFileResult WriteFile(string filename, int index, RhinoDoc doc, Rhino.FileIO.FileWriteOptions options)
        {

            

            Rhino.RhinoApp.WriteLine("Hello");

            List<InstanceDefinitionGeometry> planViewsDefs = new List<InstanceDefinitionGeometry>();



            // All SVG docs will be stored here.
            List<XmlDocument> svgDocs = new List<XmlDocument>();

            foreach (Rhino.DocObjects.InstanceDefinition planViewDef in planViewsDefs)
            {
                SVGWriter svg = new SVGWriter();

                // Get all the objects in the Plan View.
                Rhino.DocObjects.RhinoObject[] rhobjs = planViewDef.GetObjects();
                // TODO: Remove the border and the label of the plan view.

                // Create the svg with this objects and add it to the list of SVG docs.
                svgDocs.Add(svg.CreateSVG(Rhino.RhinoDoc.ActiveDoc, rhobjs)); // TODO: should return an XmlDocument
            }

            foreach (XmlDocument svgDoc in svgDocs)
            {
                // svgDoc.Save("Test.svg"); // set the path
            }







            /*
            // Write the string array to a new file named "WriteLines.txt".
            using (var outputFile = new StreamWriter(filename))
            {
                var lines = string.Empty;

                // TODO TIENE QUE ESTAR AQUI

                // List<Guid> used_column_styles = new List<Guid>();
                // IDictionary<Guid, List<Guid>> used_profiles = new Dictionary<Guid, List<Guid>>();

                foreach (var rhino_object in RhinoDoc.ActiveDoc.Objects.GetObjectList(Rhino.DocObjects.ObjectType.InstanceReference))
                {
                    if (VisualARQ.Script.IsColumn(rhino_object.Id))
                    {
                        lines += "C";
                    }
                }

                outputFile.WriteLine(lines);
            }
            */

            return Rhino.PlugIns.WriteFileResult.Success;
        }

        // You can override methods here to change the plug-in behavior on
        // loading and shut down, add options pages to the Rhino _Option command
        // and maintain plug-in wide options in a document.
    }
}