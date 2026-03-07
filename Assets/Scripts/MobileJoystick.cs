using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MobileJoystick : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("Joystick")]
    public RectTransform joystickBackground;
    public RectTransform joystickKnob;

    [Header("Settings")]
    public float radius = 120f;
    public float fadeSpeed = 10f;

    private CanvasGroup canvasGroup;
    private Vector2 inputVector;

    void Start()
{
    canvasGroup = GetComponent<CanvasGroup>();

    // Tjek at references er tildelt
    if (joystickBackground == null || joystickKnob == null)
    {
        Debug.LogWarning("MobileJoystick: joystickBackground eller joystickKnob er ikke tildelt i inspector!");
        return; // stop hvis de mangler
    }

    // Start skjult
    joystickBackground.gameObject.SetActive(false);
    joystickKnob.gameObject.SetActive(false);
    canvasGroup.alpha = 0;
}

    

    public void OnPointerDown(PointerEventData eventData)
    {
        joystickBackground.position = eventData.position;

        joystickBackground.gameObject.SetActive(true);
        canvasGroup.alpha = 1;

        OnDrag(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 pos;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            joystickBackground,
            eventData.position,
            eventData.pressEventCamera,
            out pos
        );

        inputVector = pos / radius;
        inputVector = (inputVector.magnitude > 1f) ? inputVector.normalized : inputVector;

        joystickKnob.anchoredPosition = inputVector * radius;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        inputVector = Vector2.zero;
        joystickKnob.anchoredPosition = Vector2.zero;

        StartCoroutine(FadeOut());
    }

    System.Collections.IEnumerator FadeOut()
    {
        while (canvasGroup.alpha > 0)
        {
            canvasGroup.alpha -= Time.deltaTime * fadeSpeed;
            yield return null;
        }

        joystickBackground.gameObject.SetActive(false);
    }

    public float Horizontal()
    {
        return inputVector.x;
    }

    public float Vertical()
    {
        return inputVector.y;
    }
}