using UnityEngine;

public class BuildingButton : MonoBehaviour
{
    [Header("Location")]
    [SerializeField] private LocationId locationId;
    [SerializeField] private string displayName;

    [Header("Navigation")]
    [SerializeField] private LocationNavigationController navigationController;

    public void HandleClick()
    {
        if (navigationController == null)
        {
            Debug.LogWarning(
                "BuildingButton: Missing navigation controller for " +
                GetLocationName());
            return;
        }

        navigationController.OpenLocation(locationId);
    }

    private string GetLocationName()
    {
        if (!string.IsNullOrEmpty(displayName))
            return displayName;

        return locationId.ToString();
    }
}
