using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
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
            GenerateDynamicColumns();
        }

        /// <summary>
        /// Generates DataGrid columns dynamically based on visible fields.
        /// </summary>
        private void GenerateDynamicColumns()
        {
            // Skip first field (TypeName) as it's already a static column
            foreach (var field in _viewModel.VisibleFields.Skip(1))
            {
                var column = new DataGridTextColumn
                {
                    Header = field,
                    Binding = new System.Windows.Data.Binding
                    {
                        Converter = new DictionaryValueConverter(),
                        ConverterParameter = field,
                        Mode = BindingMode.OneWay
                    },
                    Width = new DataGridLength(1, DataGridLengthUnitType.Auto)
                };

                TypesDataGrid.Columns.Add(column);
            }
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

    /// <summary>
    /// Converter for extracting values from ParameterValues dictionary by key.
    /// </summary>
    public class DictionaryValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TypeSelectionItem item && parameter is string fieldName)
            {
                return item.ParameterValues.TryGetValue(fieldName, out var result) ? result : string.Empty;
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
