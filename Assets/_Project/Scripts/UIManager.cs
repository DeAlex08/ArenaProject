using UnityEngine;

public class UIManager : MonoBehaviour
{
    [Header("Windows")]
    [SerializeField] private GameObject statsWindow;
    [SerializeField] private GameObject infoWindow;
    [SerializeField] private GameObject equipWindow;

    private void CloseAllWindows()
    {
        statsWindow.SetActive(false);
        infoWindow.SetActive(false);
        equipWindow.SetActive(false);
    }

    public void ToggleStatsWindow()
    {
        bool isOpen = statsWindow.activeSelf;
        CloseAllWindows();

        if (!isOpen)
            statsWindow.SetActive(true);
    }

    public void CloseStatsWindow()
    {
        statsWindow.SetActive(false);
    }

    public void ToggleInfoWindow()
    {
        bool isOpen = infoWindow.activeSelf;
        CloseAllWindows();

        if (!isOpen)
            infoWindow.SetActive(true);
    }

    public void CloseInfoWindow()
    {
        infoWindow.SetActive(false);
    }

    public void ToggleEquipWindow()
    {
        bool isOpen = equipWindow.activeSelf;
        CloseAllWindows();

        if (!isOpen)
            equipWindow.SetActive(true);
    }

    public void CloseEquipWindow()
    {
        equipWindow.SetActive(false);
    }
}