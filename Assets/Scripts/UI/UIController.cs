using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    GameObject DeathScreen;
    void OnEnable()
    {
        Events.OnPlayerDeath += HandlePlayerDeath;
        DeathScreen = transform.GetChild(0).gameObject;
    }

    void HandlePlayerDeath()
    {
        DeathScreen.SetActive(true);
        StartCoroutine(TurnScreenBlack());
    }

    IEnumerator TurnScreenBlack()
    {
        Image blackPanel = DeathScreen.transform.GetChild(0).GetComponent<Image>();

        float duration = 1f;
        float elapsed = 0f;
        
        Color startColor = blackPanel.color;
        Color endColor = new Color(startColor.r, startColor.g, startColor.b, 1f);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            blackPanel.color = Color.Lerp(startColor, endColor, elapsed / duration);
            yield return null;
        }


        blackPanel.color = endColor;
    }
}
