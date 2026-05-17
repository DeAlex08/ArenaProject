using UnityEngine;

public class BarracksWindowUI : MonoBehaviour
{
    public GameObject barracksWindow;

    private BarracksInventoryUI inventoryUI;

    private void Start()
    {
        inventoryUI = FindFirstObjectByType<BarracksInventoryUI>();
    }

    public void CloseBarracks()
    {
        barracksWindow.SetActive(false);
    }

    public void OpenBarracks()
    {
        barracksWindow.SetActive(true);

        if (inventoryUI != null)
        {
            inventoryUI.ShowHelmets();
        }
    }
}