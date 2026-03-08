using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

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

        if (joystickBackground == null || joystickKnob == null)
        {
            Debug.LogWarning("MobileJoystick: Assign joystickBackground and joystickKnob in Inspector!");
            return;
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
        joystickKnob.gameObject.SetActive(true);
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
        if (inputVector.magnitude > 1f)
            inputVector = inputVector.normalized;

        joystickKnob.anchoredPosition = inputVector * radius;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        inputVector = Vector2.zero;
        joystickKnob.anchoredPosition = Vector2.zero;
        StartCoroutine(FadeOut());
    }

    IEnumerator FadeOut()
    {
        while (canvasGroup.alpha > 0)
        {
            canvasGroup.alpha -= Time.deltaTime * fadeSpeed;
            yield return null;
        }

        joystickBackground.gameObject.SetActive(false);
        joystickKnob.gameObject.SetActive(false);
    }

    public float Horizontal() => inputVector.x;
    public float Vertical() => inputVector.y;
}