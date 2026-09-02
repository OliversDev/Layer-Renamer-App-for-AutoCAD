using Autodesk.AutoCAD.Runtime;

[assembly: CommandClass(typeof(AutoCADLayerRenamer.Commands))]

namespace AutoCADLayerRenamer
{
    public sealed class Commands
    {
        [CommandMethod("OW:LayerRenamer", CommandFlags.Modal)]
        public void ShowLayerRenamer()
        {
            using (var form = new LayerRenameForm())
                Autodesk.AutoCAD.ApplicationServices.Application.ShowModalDialog(form);
        }
    }
}
