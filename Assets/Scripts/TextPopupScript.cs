using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


public class TextPopupScript : MonoBehaviour
{
    private void Start()
    {
        StartCoroutine(FadeOut());
    }

    private void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, transform.position + transform.up, Time.deltaTime / 1.2f);
    }

    private IEnumerator FadeOut()
    {
        var text = GetComponentInChildren<TextMeshPro>();
        for (float t = 0; t < 1; t += Time.deltaTime * 8)
        {
            gameObject.transform.localScale = new Vector3(t, t, t);
            yield return new WaitForEndOfFrame();
        }
        gameObject.transform.localScale = new Vector3(1, 1, 1);
        yield return new WaitForSeconds(1f);
        for (float t = 1f; t > 0; t -= Time.deltaTime * 3)
        {
            var tempColor = text.color;
            tempColor.a = t;
            text.color = tempColor;
            yield return new WaitForEndOfFrame();
        }
        Destroy(gameObject);
    }
}
