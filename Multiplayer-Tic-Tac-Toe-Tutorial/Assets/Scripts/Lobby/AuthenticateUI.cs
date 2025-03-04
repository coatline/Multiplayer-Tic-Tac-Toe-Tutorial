using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AuthenticateUI : MonoBehaviour {

    [SerializeField] Button authenticateButton;

    void Awake() {
        authenticateButton.onClick.AddListener(() => {
            LobbyManager.I.Authenticate(EditPlayerName.Instance.GetPlayerName());
            Hide();
        });
    }

    void Hide() {
        gameObject.SetActive(false);
    }
}