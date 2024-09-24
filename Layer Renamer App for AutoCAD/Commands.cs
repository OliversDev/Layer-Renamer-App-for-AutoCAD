using System;
using System.Linq;
using System.Windows.Forms;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Windows;

[assembly: CommandClass(typeof(AutoCADLayerRenamer.Commands))]

namespace AutoCADLayerRenamer
{
    public class Commands
    {
        [CommandMethod("OW:LayerRenamer")]
        public void LayerRenamer()
        {
            using (var form = new LayerRenameForm())
            {
                Autodesk.AutoCAD.ApplicationServices.Application.ShowModalDialog(form);
            }
        }
    }
}
