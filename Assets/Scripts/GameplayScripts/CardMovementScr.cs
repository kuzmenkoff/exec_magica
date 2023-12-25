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

        IsDraggable = GameManagerScr.Instance.PlayersTurn &&
                      (
                      (DefaultParent.GetComponent<DropPlaceScr>().Type == FieldType.SELF_HAND &&
                      GameManagerScr.Instance.CurrentGame.Player.Mana >= CC.Card.ManaCost) ||
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

    public void MoveToField(Transform field)
    {
        transform.SetParent(GameObject.Find("Canvas").transform);
        transform.DOMove(field.position, .5f).SetEase(Ease.InOutSine);

        HorizontalLayoutGroup layout = transform.parent.GetComponent<HorizontalLayoutGroup>();
        if (layout != null)
        {
            layout.enabled = false;
            layout.enabled = true;
        }

        //RebuildLayout();
    }

    public void MoveToTarget(Transform target)
    {
        StartCoroutine(MoveToTargetCor(target));

        //RebuildLayout();
    }

    IEnumerator MoveToTargetCor(Transform target)
    {
        GameManagerScr.Instance.EnemyAI.SubSubCourutineIsRunning = true;

        Vector3 pos = transform.position;
        Transform parent = transform.parent;
        int index = transform.GetSiblingIndex();

        HorizontalLayoutGroup layout = transform.parent.GetComponent<HorizontalLayoutGroup>();
        if (layout != null) layout.enabled = false;

        transform.SetParent(GameObject.Find("Canvas").transform);

        // Ќачало анимации с плавным стартом и завершением
        Tween moveTween = transform.DOMove(target.position, .5f).SetEase(Ease.InOutSine);

        // ќжидание завершени€ анимации
        yield return moveTween.WaitForCompletion();

        // ¬озможно, вам захочетс€ добавить небольшую паузу здесь
        yield return new WaitForSeconds(0.5f);

        // ќбратное перемещение
        moveTween = transform.DOMove(pos, .5f).SetEase(Ease.InOutSine);

        // ќжидание завершени€ обратного перемещени€
        yield return moveTween.WaitForCompletion();

        // ¬осстановление исходной иерархии
        transform.SetParent(parent);
        transform.SetSiblingIndex(index);

        if (layout != null) layout.enabled = true;

        GameManagerScr.Instance.EnemyAI.SubSubCourutineIsRunning = false;
    }


}
