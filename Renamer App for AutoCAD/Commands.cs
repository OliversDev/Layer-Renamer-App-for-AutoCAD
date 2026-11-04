using Autodesk.AutoCAD.Runtime;

[assembly: CommandClass(typeof(AutoCADRenamer.Commands))]

namespace AutoCADRenamer
{
    public sealed class Commands
    {
        [CommandMethod("OW:Renamer", CommandFlags.Modal)]
        public void ShowRenamer()
        {
            using (var form = new RenamerForm())
                Autodesk.AutoCAD.ApplicationServices.Application.ShowModalDialog(form);
        }
    }
}
