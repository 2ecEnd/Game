using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonSounds : MonoBehaviour, IPointerEnterHandler, IPointerDownHandler, IPointerUpHandler
{
    public AudioSource audioSource;
    public AudioClip clickSound;
    public AudioClip releaseSound;
    public AudioClip hoverSound;

    public void OnPointerEnter(PointerEventData eventData)
    {
        audioSource.PlayOneShot(hoverSound);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        audioSource.PlayOneShot(clickSound);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        audioSource.PlayOneShot(releaseSound);
    }
}
