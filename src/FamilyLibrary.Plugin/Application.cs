using Nice3point.Revit.Toolkit.External;
using FamilyLibrary.Plugin.Commands;

namespace FamilyLibrary.Plugin;

/// <summary>
///     Application entry point
/// </summary>
[UsedImplicitly]
public class Application : ExternalApplication
{
    public override void OnStartup()
    {
        CreateRibbon();
    }

    private void CreateRibbon()
    {
        var panel = Application.CreatePanel("Commands", "FamilyLibrary.Plugin");

        panel.AddPushButton<StartupCommand>("Execute")
            .SetImage("/FamilyLibrary.Plugin;component/Resources/Icons/RibbonIcon16.png")
            .SetLargeImage("/FamilyLibrary.Plugin;component/Resources/Icons/RibbonIcon32.png");
    }
}