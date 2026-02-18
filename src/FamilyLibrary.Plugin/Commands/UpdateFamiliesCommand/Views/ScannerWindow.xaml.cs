using System;
using Autodesk.Revit.UI;
using FamilyLibrary.Plugin.Infrastructure.WebView2;

namespace FamilyLibrary.Plugin.Commands.UpdateFamiliesCommand.Views
{
    /// <summary>
    /// Window hosting WebView2 for family scanner interface.
    /// Communicates with Angular frontend via RevitBridge.
    /// </summary>
    public partial class ScannerWindow
    {
        private readonly RevitBridge _bridge;
        private readonly UIApplication _uiApplication;

        public ScannerWindow(UIApplication uiApplication)
        {
            InitializeComponent();
            _uiApplication = uiApplication;

            // Initialize bridge for WebView2 communication
            _bridge = new RevitBridge(WebView);
            _bridge.Initialize();

            // Set active document for load operations
            var document = uiApplication.ActiveUIDocument?.Document;
            if (document != null)
            {
                _bridge.SetActiveDocument(document);
            }

            // Navigate to scanner page after initialization
            Loaded += OnWindowLoaded;
        }

        private void OnWindowLoaded(object sender, System.Windows.RoutedEventArgs e)
        {
            WebView.Initialize();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            _bridge?.SendEvent("revit:window-closed", new { window = "scanner" });
        }
    }
}
