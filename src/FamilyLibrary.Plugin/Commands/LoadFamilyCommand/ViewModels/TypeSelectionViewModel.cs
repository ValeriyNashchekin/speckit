using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using FamilyLibrary.Plugin.Commands.LoadFamilyCommand.Services;

namespace FamilyLibrary.Plugin.Commands.LoadFamilyCommand.ViewModels
{
    /// <summary>
    /// ViewModel for the Type Selection dialog.
    /// Manages type catalog entries with search, selection, and filtering.
    /// </summary>
    public sealed partial class TypeSelectionViewModel : ObservableObject
    {
        private readonly TypeCatalog _catalog;

        /// <summary>
        /// Patterns for parameters that should be hidden from the grid.
        /// </summary>
        private static readonly string[] HiddenParameterPatterns = { "Comment", "GPM", "Legacy Part Number" };

        [ObservableProperty]
        private string _searchText = string.Empty;

        [ObservableProperty]
        private int _selectedCount;

        /// <summary>
        /// Collection of all available types from the catalog.
        /// </summary>
        public ObservableCollection<TypeSelectionItem> AllTypes { get; }

        /// <summary>
        /// Collection of types after applying search filter.
        /// </summary>
        public ObservableCollection<TypeSelectionItem> FilteredTypes { get; }

        /// <summary>
        /// List of visible field names (excluding hidden parameters) for dynamic columns.
        /// </summary>
        public List<string> VisibleFields { get; }

        /// <summary>
        /// Gets whether the user confirmed the selection.
        /// </summary>
        public bool UserConfirmed { get; private set; }

        public TypeSelectionViewModel(TypeCatalog catalog)
        {
            _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
            AllTypes = new ObservableCollection<TypeSelectionItem>();
            FilteredTypes = new ObservableCollection<TypeSelectionItem>();
            VisibleFields = FilterVisibleFields(_catalog.Fields);

            InitializeTypes();
        }

        /// <summary>
        /// Filters out hidden parameters from the field list.
        /// </summary>
        private static List<string> FilterVisibleFields(List<string> fields)
        {
            return fields
                .Where(f => !IsHiddenParameter(f))
                .ToList();
        }

        /// <summary>
        /// Checks if a parameter should be hidden based on name patterns.
        /// </summary>
        private static bool IsHiddenParameter(string fieldName)
        {
            return HiddenParameterPatterns.Any(pattern =>
                fieldName.Contains(pattern, StringComparison.OrdinalIgnoreCase));
        }

        private void InitializeTypes()
        {
            foreach (var entry in _catalog.Types)
            {
                var item = new TypeSelectionItem(entry, VisibleFields);
                item.PropertyChanged += OnItemPropertyChanged;
                AllTypes.Add(item);
            }

            ApplyFilter();
        }

        private void OnItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(TypeSelectionItem.IsSelected))
            {
                UpdateSelectedCount();
            }
        }

        partial void OnSearchTextChanged(string value)
        {
            ApplyFilter();
        }

        private void ApplyFilter()
        {
            FilteredTypes.Clear();

            var filtered = string.IsNullOrEmpty(SearchText)
                ? AllTypes
                : AllTypes.Where(t => t.TypeName.Contains(SearchText, StringComparison.OrdinalIgnoreCase));

            foreach (var item in filtered)
            {
                FilteredTypes.Add(item);
            }
        }

        private void UpdateSelectedCount()
        {
            SelectedCount = AllTypes.Count(t => t.IsSelected);
        }

        [RelayCommand]
        private void SelectAll()
        {
            foreach (var item in FilteredTypes)
            {
                item.IsSelected = true;
            }
        }

        [RelayCommand]
        private void SelectNone()
        {
            foreach (var item in AllTypes)
            {
                item.IsSelected = false;
            }
        }

        [RelayCommand]
        private void Load()
        {
            if (SelectedCount == 0)
            {
                MessageBox.Show(
                    "Please select at least one type to load.",
                    "No Selection",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            UserConfirmed = true;
            CloseDialog(true);
        }

        [RelayCommand]
        private void Cancel()
        {
            UserConfirmed = false;
            CloseDialog(false);
        }

        private void CloseDialog(bool dialogResult)
        {
            foreach (Window window in System.Windows.Application.Current.Windows)
            {
                if (window.DataContext == this)
                {
                    window.DialogResult = dialogResult;
                    window.Close();
                    break;
                }
            }
        }

        /// <summary>
        /// Gets the list of selected type catalog entries.
        /// </summary>
        public List<TypeCatalogEntry> GetSelectedTypes()
        {
            return AllTypes
                .Where(t => t.IsSelected)
                .Select(t => t.Entry)
                .ToList();
        }
    }

    /// <summary>
    /// Represents a single type entry in the selection list.
    /// Supports selection state and displays type name with parameter values.
    /// </summary>
    public partial class TypeSelectionItem : ObservableObject
    {
        /// <summary>
        /// The underlying type catalog entry.
        /// </summary>
        public TypeCatalogEntry Entry { get; }

        /// <summary>
        /// Gets the type name for display.
        /// </summary>
        public string TypeName => Entry.TypeName;

        /// <summary>
        /// Dictionary of parameter values for dynamic column binding.
        /// Only contains values for visible fields.
        /// </summary>
        public Dictionary<string, string> ParameterValues { get; }

        [ObservableProperty]
        private bool _isSelected;

        public TypeSelectionItem(TypeCatalogEntry entry, List<string> visibleFields)
        {
            Entry = entry ?? throw new ArgumentNullException(nameof(entry));
            ParameterValues = new Dictionary<string, string>();
            _isSelected = true; // Select all by default

            // Populate parameter values only for visible fields
            foreach (var field in visibleFields)
            {
                if (field != "TypeName" && entry.Values.TryGetValue(field, out var value))
                {
                    ParameterValues[field] = value;
                }
            }
        }
    }
}
