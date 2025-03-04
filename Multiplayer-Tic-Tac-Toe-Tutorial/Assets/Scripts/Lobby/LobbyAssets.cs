using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LobbyAssets : MonoBehaviour {



    public static LobbyAssets Instance { get; private set; }


    [SerializeField] private Sprite marineSprite;
    [SerializeField] private Sprite ninjaSprite;
    [SerializeField] private Sprite zombieSprite;


    private void Awake() {
        Instance = this;
    }

    public Sprite GetSprite(LobbyManager.Weapon playerCharacter) {
        switch (playerCharacter) {
            default:
            case LobbyManager.Weapon.Marine:   return marineSprite;
            case LobbyManager.Weapon.Ninja:    return ninjaSprite;
            case LobbyManager.Weapon.Zombie:   return zombieSprite;
        }
    }

}