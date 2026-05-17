using UnityEngine;

public class UIManager : MonoBehaviour
{
    [Header("Windows")]
    [SerializeField] private GameObject statsWindow;
    [SerializeField] private GameObject infoWindow;
    [SerializeField] private GameObject equipWindow;

    [Header("Character Panel")]
    [SerializeField] private CharacterPanelStatsViewUI characterPanelStatsView;
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private EquipmentManager equipmentManager;

    private void Awake()
    {
        SetupCharacterPanelStatsView();
    }

    private void CloseAllWindows()
    {
        if (statsWindow != null)
            statsWindow.SetActive(false);

        if (infoWindow != null)
            infoWindow.SetActive(false);

        if (equipWindow != null)
            equipWindow.SetActive(false);

        if (characterPanelStatsView != null)
            characterPanelStatsView.ShowPortrait();
    }

    public void ToggleStatsWindow()
    {
        if (characterPanelStatsView != null)
        {
            if (!characterPanelStatsView.IsShowingStats())
                CloseAllWindows();

            characterPanelStatsView.ToggleView();
            return;
        }

        bool isOpen = statsWindow != null && statsWindow.activeSelf;
        CloseAllWindows();

        if (!isOpen && statsWindow != null)
            statsWindow.SetActive(true);
    }

    public void CloseStatsWindow()
    {
        if (statsWindow != null)
            statsWindow.SetActive(false);

        if (characterPanelStatsView != null)
            characterPanelStatsView.ShowPortrait();
    }

    public void ToggleInfoWindow()
    {
        bool isOpen = infoWindow != null && infoWindow.activeSelf;
        CloseAllWindows();

        if (!isOpen && infoWindow != null)
            infoWindow.SetActive(true);
    }

    public void CloseInfoWindow()
    {
        if (infoWindow != null)
            infoWindow.SetActive(false);
    }

    public void ToggleEquipWindow()
    {
        bool isOpen = equipWindow != null && equipWindow.activeSelf;
        CloseAllWindows();

        if (!isOpen && equipWindow != null)
            equipWindow.SetActive(true);
    }

    public void CloseEquipWindow()
    {
        if (equipWindow != null)
            equipWindow.SetActive(false);
    }

    private void SetupCharacterPanelStatsView()
    {
        if (playerStats == null)
            playerStats = FindFirstObjectByType<PlayerStats>();

        if (equipmentManager == null)
            equipmentManager = FindSceneObject<EquipmentManager>();

        if (characterPanelStatsView == null)
            characterPanelStatsView = FindSceneObject<CharacterPanelStatsViewUI>();

        if (characterPanelStatsView == null)
        {
            GameObject characterPanel = GameObject.Find("CharacterPanel");

            if (characterPanel != null)
                characterPanelStatsView = characterPanel.AddComponent<CharacterPanelStatsViewUI>();
        }

        if (characterPanelStatsView != null)
            characterPanelStatsView.Initialize(playerStats, equipmentManager);
    }

    private T FindSceneObject<T>() where T : Object
    {
        T[] objects = Resources.FindObjectsOfTypeAll<T>();

        foreach (T item in objects)
        {
            Component component = item as Component;
            if (component != null && component.gameObject.scene.IsValid())
                return item;
        }

        return null;
    }
}
