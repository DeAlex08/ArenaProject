using System;
using UnityEngine;

public enum LocationId
{
    Barracks,
    Arena,
    Market,
    Forge,
    Mage,
    TrialTower,
    Bank,
    ResidentialDistrict,
    Tavern,
    Temple,
    Guild,
    CraftDistrict
}

public class LocationNavigationController : MonoBehaviour
{
    [Serializable]
    private class LocationWindowBinding
    {
        public LocationId locationId;
        public string displayName;
        public GameObject window;
        public BarracksWindowUI barracksWindowUI;
    }

    [Header("Location Windows")]
    [SerializeField] private LocationWindowBinding[] locationWindows;

    private LocationId? currentLocation;

    private void Awake()
    {
        CloseAllLocationWindows();
    }

    public void OpenBarracks() => OpenLocation(LocationId.Barracks);
    public void OpenArena() => OpenLocation(LocationId.Arena);
    public void OpenMarket() => OpenLocation(LocationId.Market);
    public void OpenForge() => OpenLocation(LocationId.Forge);
    public void OpenMage() => OpenLocation(LocationId.Mage);
    public void OpenTrialTower() => OpenLocation(LocationId.TrialTower);
    public void OpenBank() => OpenLocation(LocationId.Bank);
    public void OpenResidentialDistrict() => OpenLocation(LocationId.ResidentialDistrict);
    public void OpenTavern() => OpenLocation(LocationId.Tavern);
    public void OpenTemple() => OpenLocation(LocationId.Temple);
    public void OpenGuild() => OpenLocation(LocationId.Guild);
    public void OpenCraftDistrict() => OpenLocation(LocationId.CraftDistrict);

    public void OpenLocation(LocationId locationId)
    {
        LocationWindowBinding binding = GetBinding(locationId);
        GameObject targetWindow = binding != null && binding.window != null
            ? binding.window
            : FindWindowForLocation(locationId);

        if (targetWindow == null)
        {
            Debug.LogWarning(
                "LocationNavigationController: Location is unavailable or has no window assigned: " +
                locationId);
            return;
        }

        CloseAllLocationWindows();

        currentLocation = locationId;

        if (binding != null && binding.barracksWindowUI != null)
        {
            binding.barracksWindowUI.OpenBarracks();
            return;
        }

        targetWindow.SetActive(true);
    }

    public void CloseCurrentLocation()
    {
        CloseAllLocationWindows();
        currentLocation = null;
    }

    private void CloseAllLocationWindows()
    {
        if (locationWindows == null)
            return;

        foreach (LocationWindowBinding binding in locationWindows)
        {
            if (binding != null && binding.window != null)
                binding.window.SetActive(false);
        }

        foreach (string windowName in GetAllLocationWindowNames())
        {
            GameObject window = FindSceneObjectByName(windowName);

            if (window != null)
                window.SetActive(false);
        }
    }

    private LocationWindowBinding GetBinding(LocationId locationId)
    {
        if (locationWindows == null)
            return null;

        foreach (LocationWindowBinding binding in locationWindows)
        {
            if (binding != null && binding.locationId == locationId)
                return binding;
        }

        return null;
    }

    private GameObject FindWindowForLocation(LocationId locationId)
    {
        foreach (string windowName in GetWindowNameCandidates(locationId))
        {
            GameObject window = FindSceneObjectByName(windowName);

            if (window != null)
                return window;
        }

        return null;
    }

    private string[] GetWindowNameCandidates(LocationId locationId)
    {
        switch (locationId)
        {
            case LocationId.Barracks:
                return new[] { "BarracksWindow" };
            case LocationId.Arena:
                return new[] { "ArenaWindow" };
            case LocationId.Market:
                return new[] { "MarketWindow" };
            case LocationId.Forge:
                return new[] { "ForgeWindow", "BlacksmithWindow" };
            case LocationId.Mage:
                return new[] { "MageWindow" };
            case LocationId.TrialTower:
                return new[] { "TrialTowerWindow" };
            case LocationId.Bank:
                return new[] { "BankWindow" };
            case LocationId.ResidentialDistrict:
                return new[] { "ResidentialDistrictWindow" };
            case LocationId.Tavern:
                return new[] { "TavernWindow" };
            case LocationId.Temple:
                return new[] { "TempleWindow" };
            case LocationId.Guild:
                return new[] { "GuildWindow" };
            case LocationId.CraftDistrict:
                return new[] { "CraftDistrictWindow" };
        }

        return Array.Empty<string>();
    }

    private string[] GetAllLocationWindowNames()
    {
        return new[]
        {
            "BarracksWindow",
            "ArenaWindow",
            "MarketWindow",
            "ForgeWindow",
            "BlacksmithWindow",
            "MageWindow",
            "TrialTowerWindow",
            "BankWindow",
            "ResidentialDistrictWindow",
            "TavernWindow",
            "TempleWindow",
            "GuildWindow",
            "CraftDistrictWindow"
        };
    }

    private GameObject FindSceneObjectByName(string objectName)
    {
        GameObject[] sceneObjects = Resources.FindObjectsOfTypeAll<GameObject>();

        foreach (GameObject sceneObject in sceneObjects)
        {
            if (sceneObject.name == objectName && sceneObject.scene.IsValid())
                return sceneObject;
        }

        return null;
    }
}
