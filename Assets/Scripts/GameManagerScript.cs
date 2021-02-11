using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManagerScript : MonoBehaviour
{
    public bool disableInput;
    public int level;
    public int overallResult;
    private GameObject player;
    private Camera cam;
    private Vector3 camOffset;
    private Vector3 velocity = Vector3.zero;
    private GameObject tracksObj;
    private Transform envT;
    private List<float> wallCoordinatesList = new List<float> { 0, .5f, 1f, 1.5f};
    private List<float> barrierCoordinatesList = new List<float> { .25f, .75f, 1.25f };
    private GameObject boss;
    [HideInInspector]
    public bool centerReached;

    public GameObject[] wallPrefabs;
    public GameObject barrierPrefab;
    public GameObject[] weaponPrefabs;
    public GameObject[] foodPrefabs;
    public GameObject textPopupPrefab;
    public GameObject[] particlesPrefabs;
    public Material[] materials;
    private Text levelText;
    private Text overallResultText;
    public static GameManagerScript instance = null;
    private GameMenuHandler GMH;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else if (instance != this) Destroy(gameObject);
    }

    private void Start()
    {
        float screenRatio = (float)Screen.height / Screen.width; //Adjust UI and camera view to fit screen size
        if (screenRatio < 1.7f)
            Camera.main.GetComponent<CameraConstantWidth>().WidthOrHeight = 1f;
        else if (screenRatio > 1.95f)
            GameObject.Find("Canvas").GetComponent<CanvasScaler>().matchWidthOrHeight = .6f;
    }

    private void LateUpdate()
    {
        if (!disableInput)
        {
            Vector3 camTargetPos = player.transform.position + camOffset;
            camTargetPos.x = .75f;
            cam.transform.position = camTargetPos;
        }
        tracksObj.transform.position = new Vector3(0, 0, player.transform.position.z);
        if (boss.transform.position.z - player.transform.position.z < 4f && !disableInput)
        {
            disableInput = true;
            StartCoroutine(BossHitCoroutine());
        }
    }

    private void OnSceneLoaded(Scene aScene, LoadSceneMode aMode)
    {
        player = GameObject.Find("Pigman");
        cam = Camera.main;
        camOffset = cam.transform.position - player.transform.position;
        tracksObj = GameObject.Find("Environment/Tracks");
        envT = GameObject.Find("Environment").transform;
        boss = GameObject.Find("RubberDuck");
        levelText = GameObject.Find("Canvas/LevelText").GetComponent<Text>();
        levelText.text = "LEVEL " + level.ToString();
        overallResultText = GameObject.Find("Canvas/ScoreText").GetComponent<Text>();
        overallResultText.text = "SCORE " + overallResult.ToString();
        GMH = GameObject.Find("Canvas/GameMenuHandler").GetComponent<GameMenuHandler>();

        LevelGenerator();
        disableInput = false;
    }

    public void TextPopup(GameObject obj, string text)
    {
        GameObject popup = Instantiate(textPopupPrefab, obj.transform.position + new Vector3(0, 0, -.11f), obj.transform.rotation, envT);
        popup.GetComponentInChildren<TextMeshPro>().text = text;
    }

    public void WallBreak(GameObject wall)
    {
        var particle = Instantiate(particlesPrefabs[1], wall.transform.position, wall.transform.rotation);
        if (wall.name.Contains("Wood"))
        {
            SoundManager.instance.RandomizeSfx(1f, SoundManager.instance.wallSounds[1]);
            particle.GetComponent<ParticleSystemRenderer>().material = materials[0];
        }
        else
        {
            SoundManager.instance.RandomizeSfx(.5f, SoundManager.instance.wallSounds[3]);
            particle.GetComponent<ParticleSystemRenderer>().material = materials[1];
        }
        particle.GetComponent<ParticleSystem>().collision.SetPlane(0, GameObject.Find("Environment/GroundPlane").transform);
        Destroy(wall);
        Destroy(particle, 4);
    }

    private void LevelGenerator()
    {
        for (int row = 1; row <= (int)(boss.transform.position.z - 5) / 4; row++)
        {
            List<float> coordList = new List<float>(barrierCoordinatesList);
            if (Random.value < .3f)                         //Generate random barrier
            {
                var barrierPos = new Vector3(0, .2f, row * 4 + 2);
                if (barrierPos.z < (boss.transform.position.z - 7)) //prevent barrier close to boss
                {
                    for (int interTrack = 0; interTrack <= Random.Range(0, 3); interTrack++)
                    {
                        var randomIndex = Random.Range(0, coordList.Count);
                        var rndX = coordList[randomIndex];
                        coordList.RemoveAt(randomIndex);
                        barrierPos.x = rndX;
                        var barrier = Instantiate(barrierPrefab, barrierPos, Quaternion.identity, envT);
                        if (Random.value < .5f) barrier.transform.localScale += new Vector3(0, 0, 1.1f);
                    }
                }
            }

            if (Random.value < .7f)                             //Generate random weapon and food
            {
                coordList = new List<float>(wallCoordinatesList);
                var staffPos = new Vector3(0, .156f, row * 4 + 2);
                for (int track = 0; track <= Random.Range(0, 4); track++)
                {
                    var randomIndex = Random.Range(0, coordList.Count);
                    var rndX = coordList[randomIndex];
                    coordList.RemoveAt(randomIndex);
                    staffPos.x = rndX;

                    List<int> zPosOffsetList = new List<int> { -1, 0, 1 };
                    for (int z = 0; z < 3; z++)
                    {
                        if (Random.value < .3f) continue;
                        var rndZOffsetIndex = Random.Range(0, zPosOffsetList.Count);
                        var rndZOffset = zPosOffsetList[rndZOffsetIndex];
                        zPosOffsetList.RemoveAt(rndZOffsetIndex);
                        Vector3 staffRealPos = new Vector3(staffPos.x, staffPos.y, staffPos.z + rndZOffset);

                        var staffPrefab = foodPrefabs[Random.Range(0, 2)];
                        if (Random.value <= .05f) staffPrefab = weaponPrefabs[Random.Range(0, 2)];
                        Instantiate(staffPrefab, staffRealPos, Quaternion.identity, envT);
                    }
                }
            }

            if (Random.value < .04f) continue;                  //Random skip this row - no walls

            coordList = new List<float>(wallCoordinatesList);
            var wallPos = new Vector3(0, .4f, row * 4);     
            for (int track = 0; track < 4; track++)             //Generate walls
            {
                if (Random.value < .07f) continue;              //Skip this track

                var randomIndex = Random.Range(0, coordList.Count);
                var rndX = coordList[randomIndex];
                coordList.RemoveAt(randomIndex);
                wallPos.x = rndX;
                var wall = Instantiate(wallPrefabs[Random.Range(0, 2)], wallPos, Quaternion.identity, envT);
                int wallStrength;
                if (wall.name.Contains("Wood"))                 //Set random wall strength depending on wall type 
                {
                    wallStrength = Random.Range(5, 25);
                }
                else
                {
                    wallStrength = Random.Range(25, 51);
                }
                wall.GetComponent<ObstacleScript>().strength = wallStrength;
                SetWallStrengthAndColor(wall.GetComponentInChildren<TextMeshPro>(), wallStrength);

            }
            
        }
    }

    public void SetWallStrengthAndColor(TextMeshPro text, int strength)
    {
        text.text = strength.ToString();
        if (strength <= 10) text.color = Color.green;
        else if (strength <= 20) text.color = Color.yellow;
        else if (strength <= 30) text.color = new Color(1, .5f, 0);
        else if (strength <= 40) text.color = new Color(1, .24f, 0);
        else text.color = new Color(.82f, 0, 0);
    }

    private IEnumerator BossHitCoroutine()
    {
        StartCoroutine(CameraRelocationCoroutine());
        var PS = player.GetComponent<PlayerScript>();
        int dir;
        if (player.transform.position.x >= .75f) dir = -2;
        else dir = 2;
        while (!centerReached)
        {
            PS.horizontalMove = transform.right * dir * Time.fixedDeltaTime;
            yield return null;
        }
        PS.horizontalMove = Vector3.zero;
        yield return new WaitUntil(() => Vector3.Distance(player.transform.position, boss.transform.position) < 1.65f);
        SoundManager.instance.PlaySingle(1f, SoundManager.instance.pigSounds[0]);
        PS.anim.SetBool("run", false);
        PS.anim.SetBool("aggressive", true);
        PS.state = PlayerScript.State.idle;
        PS.anim.SetTrigger("attack1");
        yield return new WaitUntil(() => PS.animHitDone);
        float power = PS.strength * PS.attack;
        if (power < 5) SoundManager.instance.PlaySingle(1f, SoundManager.instance.pigSounds[5]);
        else SoundManager.instance.PlaySingle(.8f, SoundManager.instance.pigSounds[6]);
        boss.GetComponent<Rigidbody>().velocity = new Vector3(0, power / 2, power);
        boss.GetComponent<BossScript>().Hit();
        yield return new WaitForSeconds(1f);
        if (power > 14) SoundManager.instance.PlaySingle(1f, SoundManager.instance.pigSounds[3]);
        else if (power > 7) SoundManager.instance.PlaySingle(1f, SoundManager.instance.pigSounds[2]);
        StartCoroutine(ResultCoroutine());
    }

    private IEnumerator CameraRelocationCoroutine()
    {
        Vector3 endPos = cam.transform.position + new Vector3(2.5f, 3.5f, 6);
        Vector3 camTarget;
        float time = 0;
        while (disableInput)
        {
            cam.transform.position = Vector3.SmoothDamp(cam.transform.position, endPos, ref velocity, 3f);
            time += Time.deltaTime / 30;
            camTarget = Vector3.Lerp(cam.transform.position + cam.transform.forward, boss.transform.position, time);
            cam.transform.LookAt(camTarget);
            yield return new WaitForEndOfFrame();
        }
    }

    private IEnumerator ResultCoroutine()
    {
        StartCoroutine(boss.GetComponent<BossScript>().DoneCoroutine());
        while (centerReached)
        {
            if (boss.GetComponent<BossScript>().done)
            {
                yield return new WaitForSeconds(1f);
                int result = int.Parse(boss.GetComponent<BossScript>().result);
                if (result > 0)
                {
                    GMH.resultText.text = "Your Score:\n" + boss.GetComponent<BossScript>().result;
                    GMH.nextLevelObj.SetActive(true);
                    overallResult += result;
                    overallResultText.text = "SCORE " + overallResult.ToString();
                    level++;
                    centerReached = false;
                }
                else
                {
                    overallResult = 0;
                    level = 1;
                    centerReached = false;
                    GMH.gameOverObj.SetActive(true);
                }
            }
            yield return new WaitForSeconds(.2f);
        }
    }

    public void NextLevel()
    {
        StopAllCoroutines();
        velocity = Vector3.zero;
        SceneManager.LoadScene(0);
    }

    public IEnumerator GameOverCoroutine()
    {
        player.GetComponent<PlayerScript>().anim.SetBool("run", false);
        player.GetComponent<PlayerScript>().anim.SetTrigger("death1");
        SoundManager.instance.PlaySingle(1f, SoundManager.instance.pigSounds[4]);
        yield return new WaitForSeconds(2.5f);
        overallResult = 0;
        level = 1;
        centerReached = false;
        GMH.gameOverObj.SetActive(true);
    }
}
