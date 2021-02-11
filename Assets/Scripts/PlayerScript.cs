using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerScript : MonoBehaviour
{
    public enum State
    {
        idle,
        run,
        attack
    }
    public State state;
    public float strength;
    public int attack;
    public GameObject obstacle;
    public bool attacking;
    public bool animHitDone;
    public Coroutine attackCor;
    private float attackTimer;

    private GameManagerScript GMS;
    private DynamicJoystick joystick;
    private Rigidbody rb;
    [HideInInspector]
    public Vector3 horizontalMove;
    [HideInInspector]
    public Animator anim;
    public LayerMask wallMask;
    public GameObject weaponPoint;
    public GameObject[] weaponObjs;
    public Transform bodyT;


    private void Start()
    {
        GMS = GameObject.Find("GameManager").GetComponent<GameManagerScript>();
        joystick = GameObject.Find("Canvas/Dynamic Joystick").GetComponent<DynamicJoystick>();
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
    }

    private void FixedUpdate()
    {
        switch (state)
        {
            default:
            case State.idle:
                break;

            case State.run:
                rb.MovePosition(rb.position + horizontalMove);
                break;

            case State.attack:
                attackTimer += Time.fixedDeltaTime;
                if (attackTimer >= .2f)
                {
                    float strengthReduce = -.02f;
                    strength += strengthReduce;
                    if (strength <= 0)
                    {
                        GMS.disableInput = true;
                        horizontalMove = Vector3.zero;
                        StartCoroutine(GMS.GameOverCoroutine());
                        state = State.idle;
                    }
                    else
                    {
                        attackTimer = 0;
                        SetBodySize(strengthReduce / 2);
                    }
                }

                if (!attacking)
                {
                    attacking = true;
                    if (attackCor != null) StopCoroutine(attackCor);
                    attackCor = StartCoroutine(AttackCoroutine());
                }
                break;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            print("!!!!!!!!");
        }
        if (Mathf.Abs(joystick.Horizontal) > 0 && !GMS.disableInput)
        {
            RunForestRun();
            if (joystick.Horizontal > 0 && transform.position.x < 1.3f) HorizontalMove();
            else if (joystick.Horizontal < 0 && transform.position.x > 0.2f) HorizontalMove();
        }

        if (transform.position.y > .015f) transform.position = new Vector3(transform.position.x, .01f, transform.position.z);
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Wall") && state != State.attack && joystick.Horizontal == 0 && !GMS.disableInput)
        {
            if (IsObstacleOnWay())
            {
                anim.SetBool("run", false);
                anim.SetBool("aggressive", true);
                obstacle = collision.gameObject;
                state = State.attack;
                horizontalMove = Vector3.zero;
            }
            else RunForestRun();
        }
        else if (collision.gameObject.CompareTag("Barrier"))
        {
            horizontalMove = Vector3.zero;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("TrackTrigger") && (joystick.Horizontal == 0 || transform.position.x >= 1.5f || transform.position.x <= 0))
        {
            horizontalMove = Vector3.zero;
        }
        else if (other.CompareTag("Food"))
        {
            float vol = 0;
            if (other.name.Contains("Hamburger")) vol = .1f;
            else if (other.name.Contains("Meat")) vol = .2f;
            strength += vol;
            if (strength > 2) strength = 2;
            else SetBodySize(vol / 2);
            SoundManager.instance.RandomizeSfx(.7f, SoundManager.instance.pigSounds[7]);
            var particle = Instantiate(GMS.particlesPrefabs[2], transform.position, Quaternion.identity);
            Destroy(particle, 2);
            Destroy(other.gameObject);
        }
        else if (other.CompareTag("Weapon"))
        {
            for (int w = 0; w < weaponObjs.Length; w++) weaponObjs[w].SetActive(false);
            if (other.name.Contains("Cudgel"))
            {
                weaponObjs[1].SetActive(true);
                attack = 7;
            }
            else if (other.name.Contains("Bat"))
            {
                weaponObjs[2].SetActive(true);
                attack = 9;
            }
            SoundManager.instance.PlaySingle(1f, SoundManager.instance.pigSounds[1]);

            var particle = Instantiate(GMS.particlesPrefabs[2], transform.position, Quaternion.identity);
            Destroy(particle, 2);
            Destroy(other.gameObject);
        }
        else if (GMS.disableInput && other.name == "Center") GMS.centerReached = true;
    }

    public void HorizontalMove()
    {
        horizontalMove = transform.right * joystick.Horizontal * 2 * Time.fixedDeltaTime;
    }

    private IEnumerator AttackCoroutine()
    {
        anim.SetTrigger("attack2");
        yield return new WaitUntil(() => animHitDone);
        animHitDone = false;
        if (IsObstacleOnWay())
        {
            GMS.TextPopup(obstacle, attack.ToString());
            obstacle.GetComponent<ObstacleScript>().strength -= attack;
            if (obstacle.GetComponent<ObstacleScript>().strength <= 0) GMS.WallBreak(obstacle);
            else
            {
                if (obstacle.name.Contains("Wood")) SoundManager.instance.RandomizeSfx(1f, SoundManager.instance.wallSounds[0]);
                else SoundManager.instance.RandomizeSfx(.8f, SoundManager.instance.wallSounds[2]);
                GMS.SetWallStrengthAndColor(obstacle.GetComponentInChildren<TextMeshPro>(), obstacle.GetComponent<ObstacleScript>().strength);
                var particle = Instantiate(GMS.particlesPrefabs[0], obstacle.transform.position + new Vector3(0, 0, -.11f), obstacle.transform.rotation);
                Destroy(particle, 1);
            }
            yield return new WaitForSeconds(.5f);
            if (obstacle == null) RunForestRun();

            if (IsObstacleOnWay()) attacking = false;
            else RunForestRun();
        }
        else RunForestRun();
    }

    public void AnimHit()
    {
        if (IsObstacleOnWay() || GMS.disableInput) animHitDone = true;
    }

    private bool IsObstacleOnWay()
    {
        Vector3 objPos = new Vector3(transform.position.x, .5f, transform.position.z);
        return Physics.Raycast(objPos, transform.forward, 1, wallMask);
    }

    private void RunForestRun()
    {
        if (attackCor != null) StopCoroutine(attackCor);
        obstacle = null;
        animHitDone = false;
        anim.SetBool("run", true);
        state = State.run;
        attacking = false;
    }

    private void SetBodySize(float vol)
    {
        bodyT.localScale += new Vector3(vol, vol / 2, vol);
        weaponPoint.transform.localScale -= new Vector3(vol, vol / 2, vol);
    }
}
