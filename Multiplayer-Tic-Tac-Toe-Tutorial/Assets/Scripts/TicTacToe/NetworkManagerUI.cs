using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;


public class NetworkManagerUI : MonoBehaviour
{
    [SerializeField] Button startHostButton;
    [SerializeField] Button startClientButton;

    private void Awake()
    {
        startHostButton.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartHost();
            Hide();
        });

        startClientButton.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartClient();
            Hide();
        });
    }

    void Hide()
    {
        gameObject.SetActive(false);
    }
}