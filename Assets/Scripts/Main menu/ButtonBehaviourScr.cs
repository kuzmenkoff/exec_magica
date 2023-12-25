using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class ButtonBehaviourScr : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{

    public Color normalColor;
    public Color highlightColor;
    public Color pressedColor;
    public float YOffset = 5f;
    public AudioSource audioSource;
    public Button button;



    private Vector2 originalPosition;
    private Vector2 enteredPosition;



    public TextMeshProUGUI buttonText;

    public void Start()
    {

        //audioSource = GetComponent<AudioSource>();
        buttonText = GetComponentInChildren<TextMeshProUGUI>();
        originalPosition = buttonText.rectTransform.anchoredPosition;
        enteredPosition = buttonText.rectTransform.anchoredPosition;
        enteredPosition.y -= YOffset;

        ColorUtility.TryParseHtmlString("#CAC5C1", out normalColor);
        ColorUtility.TryParseHtmlString("#A2A09E", out highlightColor);
        ColorUtility.TryParseHtmlString("#5A5A5A", out pressedColor);
        buttonText.color = normalColor;

        if (button.IsInteractable() == false)
            buttonText.color = pressedColor;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (button.IsInteractable() == false)
            return;
        buttonText.color = highlightColor;
        buttonText.rectTransform.anchoredPosition = new Vector2(0, originalPosition.y - YOffset);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (button.IsInteractable() == false)
            return;
        buttonText.color = normalColor;
        buttonText.rectTransform.anchoredPosition = originalPosition;



    }

    public void OnPointerDown(PointerEventData eventData)
    {

        if (button.IsInteractable() == false)
            return;
        buttonText.color = pressedColor;
        buttonText.rectTransform.anchoredPosition = new Vector2(0, originalPosition.y - YOffset * 2);
        audioSource.Play();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (button.IsInteractable() == false)
            return;
        buttonText.color = eventData.hovered.Contains(gameObject) ? highlightColor : normalColor;

        buttonText.rectTransform.anchoredPosition = originalPosition;

    }


}
