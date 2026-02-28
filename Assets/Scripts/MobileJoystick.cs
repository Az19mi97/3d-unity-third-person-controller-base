using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MobileJoystick : MonoBehaviour, IDragHandler, IPointerUpHandler, IPointerDownHandler
{
    [Header("Joystick References")]
    public Image joystickBackground;
    public Image joystickKnob;

    [Header("Settings")]
    public float moveThreshold = 50f; 

    private Vector2 inputVector;

    void Start()
    {
        joystickKnob.rectTransform.anchoredPosition = Vector2.zero;
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 pos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            joystickBackground.rectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out pos
        );

        pos.x = (pos.x / joystickBackground.rectTransform.sizeDelta.x) * 2;
        pos.y = (pos.y / joystickBackground.rectTransform.sizeDelta.y) * 2;

        inputVector = new Vector2(pos.x, pos.y);
        inputVector = (inputVector.magnitude > 1f) ? inputVector.normalized : inputVector;

        joystickKnob.rectTransform.anchoredPosition = inputVector * moveThreshold;
    }

    public void OnPointerDown(PointerEventData eventData) => OnDrag(eventData);

    public void OnPointerUp(PointerEventData eventData)
    {
        inputVector = Vector2.zero;
        joystickKnob.rectTransform.anchoredPosition = Vector2.zero;
    }

    public float Horizontal() => inputVector.x;
    public float Vertical() => inputVector.y;
}