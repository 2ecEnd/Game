using Assets.Scripts;
using Assets.Scripts.Player;
using UnityEngine;
public class PlayerGUI : MonoBehaviour
{
    public GUISkin MainGUISkin;
    private GameObject player;
    private WeaponHandler weaponHandler;
    private Rect[] rects = new Rect[2];
    private int screenWidth;
    private int screenHeight;
    private void Start()
    {
        player = transform.parent.gameObject;
        weaponHandler = transform.parent.GetComponent<PlayerWeaponManager>().ActiveWeapon;

        screenWidth = Screen.width;
        screenHeight = Screen.height;
        //rects[0] = new Rect(0, 0, 400, 200);
        rects[0] = new Rect(screenWidth * 0.5f - 1, screenHeight * 0.5f - 1, 2, 2);
        rects[1] = new Rect(screenWidth * 0.9f, screenHeight * 0.9f, screenWidth * 0.1f, screenHeight * 0.1f);
    }
    private void OnGUI()
    {
        GUI.skin.box.fontSize = 60;
        GUI.depth = 1;
        GUI.Box(rects[0], "");
        GUI.Box(rects[1], weaponHandler.CurrentAmmo.ToString());
        //GUI.Label(new Rect(100, 100, 400, 200), "υσι");
    }
}