using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    [SerializeField] GameObject inventoryPannel;
    [SerializeField] AnimationCurve showCurve;
    [SerializeField] AnimationCurve hideCurve;
    [SerializeField] float animationSpeed = 1f;
    bool isOpen = false;


    IEnumerator ShowPannel(GameObject pannel)
    {
        RectTransform rt = pannel.GetComponent<RectTransform>();
        float moveDistance = rt.rect.width;
        float timer = 0f;
        pannel.SetActive(true);
        while (timer < 1f)
        {
            timer += Time.deltaTime * animationSpeed;
            rt.anchoredPosition = new Vector2(moveDistance * showCurve.Evaluate(timer), rt.anchoredPosition.y);
            yield return null;
        }
    }

    IEnumerator HidePannel(GameObject pannel)
    {
        RectTransform rt = pannel.GetComponent<RectTransform>();
        float moveDistance = rt.rect.width;
        float timer = 0f;

        while (timer < 1f)
        {
            timer += Time.deltaTime * animationSpeed;
            rt.anchoredPosition = new Vector2(moveDistance * hideCurve.Evaluate(timer), rt.anchoredPosition.y);
            yield return null;
        }

        pannel.SetActive(false);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if(isOpen) StartCoroutine(HidePannel(inventoryPannel));
            else StartCoroutine(ShowPannel(inventoryPannel));
            isOpen = !isOpen;
        }
    }
}