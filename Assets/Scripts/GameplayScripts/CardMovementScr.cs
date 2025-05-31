using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardMovementScr : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public CardController CC;

    Camera MainCamera;
    Vector3 offset;
    public Transform DefaultParent, DefaultTempCardParent;
    GameObject TempCardGO;
    public bool IsDraggable;
    int startID;


    void Awake()
    {
        MainCamera = Camera.allCameras[0];
        TempCardGO = GameObject.Find("TempCardGO");
    }
    public void OnBeginDrag(PointerEventData eventData)
    {
        offset = transform.position - MainCamera.ScreenToWorldPoint(eventData.position);
        DefaultParent = DefaultTempCardParent = transform.parent;

        IsDraggable = GameManagerScr.Instance.PlayerTurn &&
                      (
                      (DefaultParent.GetComponent<DropPlaceScr>().Type == FieldType.SELF_HAND &&
                      GameManagerScr.Instance.Player.Mana >= CC.Card.ManaCost) ||
                      (DefaultParent.GetComponent<DropPlaceScr>().Type == FieldType.SELF_FIELD &&
                      CC.Card.CanAttack)
                      );

        if (!IsDraggable)
            return;

        startID = transform.GetSiblingIndex();

        if (CC.Card.IsSpell || CC.Card.CanAttack)
            GameManagerScr.Instance.HightLightTargets(CC, true);

        TempCardGO.transform.SetParent(DefaultParent);
        TempCardGO.transform.SetSiblingIndex(transform.GetSiblingIndex());

        transform.SetParent(DefaultParent.parent);

        GetComponent<CanvasGroup>().blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!IsDraggable)
            return;

        Vector3 newPos = MainCamera.ScreenToWorldPoint(eventData.position);
        transform.position = newPos + offset;

        if (!CC.Card.IsSpell)
        {

            if (TempCardGO.transform.parent != DefaultTempCardParent)
                TempCardGO.transform.SetParent(DefaultTempCardParent);

            if (DefaultParent.GetComponent<DropPlaceScr>().Type != FieldType.SELF_FIELD)
                CheckPosition();
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {

        if (!IsDraggable)
            return;

        GameManagerScr.Instance.HightLightTargets(CC, false);

        transform.SetParent(DefaultParent);
        GetComponent<CanvasGroup>().blocksRaycasts = true;

        transform.SetSiblingIndex(TempCardGO.transform.GetSiblingIndex());
        TempCardGO.transform.SetParent(GameObject.Find("Canvas").transform);
        TempCardGO.transform.localPosition = new Vector3(2362, 0);
    }

    void CheckPosition()
    {
        int newIndex = DefaultTempCardParent.childCount;
        for (int i = 0; i < DefaultTempCardParent.childCount; i++)
        {
            if (transform.position.x < DefaultTempCardParent.GetChild(i).position.x)
            {
                newIndex = i;
                if (TempCardGO.transform.GetSiblingIndex() < newIndex)
                {
                    newIndex--;
                }
                break;
            }
        }

        if (TempCardGO.transform.parent == DefaultParent)
            newIndex = startID;

        TempCardGO.transform.SetSiblingIndex(newIndex);
    }

    public IEnumerator MoveToField(Transform field)
    {
        transform.SetParent(GameObject.Find("Canvas").transform);

        // Чекаємо завершення анімації
        Tween moveTween = transform.DOMove(field.position, 0.5f).SetEase(Ease.InOutSine);
        yield return moveTween.WaitForCompletion();

        transform.SetParent(field);
        transform.localPosition = Vector3.zero;

        // Перебудова Layout (якщо є)
        HorizontalLayoutGroup layout = transform.parent.GetComponent<HorizontalLayoutGroup>();
        if (layout != null)
        {
            layout.enabled = false;
            layout.enabled = true;
        }
    }

    public IEnumerator MoveToTargetCor(Transform target)
    {

        Vector3 pos = transform.position;
        Transform parent = transform.parent;
        int index = transform.GetSiblingIndex();

        HorizontalLayoutGroup layout = transform.parent.GetComponent<HorizontalLayoutGroup>();
        if (layout != null) layout.enabled = false;

        transform.SetParent(GameObject.Find("Canvas").transform);

        // Начало анимации с плавным стартом и завершением
        Tween moveTween = transform.DOMove(target.position, .5f).SetEase(Ease.InOutSine);

        // Ожидание завершения анимации
        yield return moveTween.WaitForCompletion();

        // Возможно, вам захочется добавить небольшую паузу здесь
        yield return new WaitForSeconds(0.5f);

        // Обратное перемещение
        moveTween = transform.DOMove(pos, .5f).SetEase(Ease.InOutSine);

        // Ожидание завершения обратного перемещения
        yield return moveTween.WaitForCompletion();

        // Восстановление исходной иерархии
        transform.SetParent(parent);
        transform.SetSiblingIndex(index);

        if (layout != null) layout.enabled = true;

    }

    public IEnumerator MoveToCenterAndVanish(float displayDuration = 1f)
    {
        Transform canvas = GameObject.Find("Canvas").transform;

        // Встановити на верхній шар
        transform.SetParent(canvas);

        // Відключити layout, якщо є
        HorizontalLayoutGroup layout = canvas.GetComponent<HorizontalLayoutGroup>();
        if (layout != null) layout.enabled = false;

        // Центр екрану в world координатах
        Vector3 screenCenter = new Vector3(Screen.width / 2f, Screen.height / 2f, 0f);
        Vector3 worldCenter = Camera.main.ScreenToWorldPoint(screenCenter);
        worldCenter.z = 0;

        // Рух до центру
        Tween moveTween = transform.DOMove(worldCenter, 0.5f).SetEase(Ease.InOutSine);
        yield return moveTween.WaitForCompletion();

        // Пауза (показ ефекту)
        yield return new WaitForSeconds(displayDuration);

        // Зникнення
        Tween scaleTween = transform.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InOutSine);
        yield return scaleTween.WaitForCompletion();

        // Знищення обʼєкта
        Destroy(gameObject);
    }

    public IEnumerator MoveToTargetAndVanish(Transform target, float displayDuration = 1f)
    {
        // Зберігаємо початкові дані
        Transform canvas = GameObject.Find("Canvas").transform;
        transform.SetParent(canvas);

        // Вимикаємо layout, якщо був
        HorizontalLayoutGroup layout = canvas.GetComponent<HorizontalLayoutGroup>();
        if (layout != null) layout.enabled = false;

        // Анімація руху до цілі
        Tween moveTween = transform.DOMove(target.position, 0.5f).SetEase(Ease.InOutSine);
        yield return moveTween.WaitForCompletion();

        // Пауза — як ефект фокусу перед дією
        yield return new WaitForSeconds(displayDuration);

        // Анімація зникнення
        Tween scaleTween = transform.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InOutSine);
        yield return scaleTween.WaitForCompletion();

        Destroy(gameObject);
    }


}
