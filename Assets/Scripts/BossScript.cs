using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BossScript : MonoBehaviour
{
    private bool doOnce;
    public string result;
    public bool done;
    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!doOnce)
        {
            Hit();
        }

        if (collision.gameObject.CompareTag("ResultPlane")) 
            result = collision.gameObject.GetComponentInChildren<TextMeshPro>().text;
    }

    public void Hit()
    {
        doOnce = true;
        StartCoroutine(ReleaseDoOnce());
        if (rb.velocity.magnitude > 5)
        {
            SoundManager.instance.RandomizeSfx(1f, SoundManager.instance.duckSounds[2]);
        }
        else if (rb.velocity.magnitude > 2)
        {
            SoundManager.instance.RandomizeSfx(1f, SoundManager.instance.duckSounds[1]);
        }
        else if (rb.velocity.magnitude > .3f)
        {
            SoundManager.instance.RandomizeSfx(1f, SoundManager.instance.duckSounds[0]);
        }
    }

    private IEnumerator ReleaseDoOnce()
    {
        yield return new WaitForSeconds(.2f);
        doOnce = false;
    }

    public IEnumerator DoneCoroutine()
    {
        while (!done)
        {
            var dist = transform.position;
            yield return new WaitForSeconds(1f);
            if (Vector3.Distance(transform.position, dist) < .1f || transform.position.y < -10) done = true;
        }
    }
}
