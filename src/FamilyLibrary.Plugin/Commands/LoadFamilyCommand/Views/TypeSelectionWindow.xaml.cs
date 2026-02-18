using System.Windows;
using System.Windows.Controls;
using FamilyLibrary.Plugin.Commands.LoadFamilyCommand.Services;
using FamilyLibrary.Plugin.Commands.LoadFamilyCommand.ViewModels;

namespace FamilyLibrary.Plugin.Commands.LoadFamilyCommand.Views
{
    /// <summary>
    /// Code-behind for TypeSelectionWindow.
    /// Displays type catalog entries and allows selection for loading.
    /// </summary>
    public sealed partial class TypeSelectionWindow
    {
        private readonly TypeSelectionViewModel _viewModel;

        /// <summary>
        /// Creates a new TypeSelectionWindow instance.
        /// </summary>
        /// <param name="catalog">The type catalog to display.</param>
        public TypeSelectionWindow(TypeCatalog catalog)
        {
            _viewModel = new TypeSelectionViewModel(catalog);
            DataContext = _viewModel;
            InitializeComponent();
        }

        /// <summary>
        /// Gets the list of selected type catalog entries.
        /// </summary>
        public List<TypeCatalogEntry> GetSelectedTypes()
        {
            return _viewModel.GetSelectedTypes();
        }

        /// <summary>
        /// Gets whether the user confirmed the selection.
        /// </summary>
        public bool UserConfirmed => _viewModel.UserConfirmed;

        /// <summary>
        /// Handles double-click on a data grid row to toggle selection.
        /// </summary>
        private void OnRowDoubleClick(object sender, RoutedEventArgs e)
        {
            if (sender is DataGridRow row && row.Item is TypeSelectionItem item)
            {
                item.IsSelected = !item.IsSelected;
            }
        }
    }
}
