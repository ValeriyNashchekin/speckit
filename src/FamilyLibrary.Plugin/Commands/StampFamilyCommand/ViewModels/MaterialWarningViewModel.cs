using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace FamilyLibrary.Plugin.Commands.StampFamilyCommand.ViewModels;

/// <summary>
/// ViewModel for the Material Warning dialog.
/// Manages the list of missing materials and user selection for creating new ones.
/// </summary>
public sealed partial class MaterialWarningViewModel : ObservableObject
{
    /// <summary>
    /// Collection of missing materials that may not exist in target project.
    /// </summary>
    public ObservableCollection<MissingMaterialItem> MissingMaterials { get; }

    /// <summary>
    /// Gets whether the user chose to proceed with the operation.
    /// </summary>
    public bool UserChoseToProceed { get; private set; }

    public MaterialWarningViewModel()
    {
        MissingMaterials = new ObservableCollection<MissingMaterialItem>();
    }

    /// <summary>
    /// Initializes the view model with a list of missing materials.
    /// </summary>
    public void Initialize(System.Collections.Generic.IList<MissingMaterialItem> materials)
    {
        MissingMaterials.Clear();
        foreach (var material in materials)
        {
            MissingMaterials.Add(material);
        }
    }

    /// <summary>
    /// Gets the list of materials that should be created new.
    /// </summary>
    public System.Collections.Generic.List<MissingMaterialItem> GetMaterialsToCreate()
    {
        return MissingMaterials
            .Where(m => m.CreateNew)
            .ToList();
    }

    [RelayCommand]
    private void Proceed()
    {
        UserChoseToProceed = true;
        CloseDialog(true);
    }

    [RelayCommand]
    private void Cancel()
    {
        UserChoseToProceed = false;
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
}

/// <summary>
/// Represents a single missing material item in the warning dialog.
/// </summary>
public partial class MissingMaterialItem : ObservableObject
{
    /// <summary>
    /// The name of the missing material.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Description of which layer uses this material (e.g., "Core", "Finish 1").
    /// </summary>
    public string LayerFunction { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether a new material should be created.
    /// Default is true.
    /// </summary>
    [ObservableProperty]
    private bool _createNew = true;
}
