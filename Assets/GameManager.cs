using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Audio;

public class GameManager : MonoBehaviour {

    public GameObject deathCloudPrefab;
    public GameObject deathCloudParticlesPrefab;
    [SerializeField] CSVReader reader;
    public MapGen mapGen;
    public Sound sound;
    public AudioMixer mixer;

    public AnimationCurve moveCurve;
    public AnimationCurve spinCurve;

    public Transform tempMarker;
    public ParticleSystem markerParticles;
    public ParticleSystem.EmissionModule markerEmission; 

    public List<Monster> monsterList = new List<Monster>();
    public List<Monster> usableMonsterList = new List<Monster>();
     

    public Node curNode;
    public List<DeathCloud> clouds = new List<DeathCloud>();


    public TextMeshProUGUI knowText;
    public TextMeshProUGUI alertText;
    public TextMeshProUGUI tutText;
    public TextMeshProUGUI bigText;

    public TMP_FontAsset garandFont;

    public int knowledge = 0;
    public float whispers = 0;

    public float insanity = 0;
    float chance = 0;

    public bool gameStarted = false;

    public float startGrowrate = 0.01f;

    public bool hasBegunCountdown = false;

    Monster monsterToKeep;

    public string letters = "qazwsxerdcrfvtgbyhnujmikolpæåø,.-'¨´+0987654321QAZWSXEDCRFVTGBYHNUJMIKOLPÆÅØ;:_*!#¤%&//()=";

    // Use this for initialization
    void Start () {
        markerEmission = markerParticles.emission;
        monsterList = reader.LoadCSV();
        GameStart();
    }
	
	// Update is called once per frame
	void Update () {

        //DEBUG
        //if (Input.GetKeyDown(KeyCode.Home))
        //{
        //    NewLevel();
        //}

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
        
    }


    public void GameStart()
    {
        gameStarted = true;
        tempMarker.gameObject.SetActive(false);
        usableMonsterList.AddRange(monsterList);
        StopCoroutine("WaitToShowLoseText");
       // ShowBigText("Write a Name", 3);
        knowText.gameObject.SetActive(false);
        bigText.fontSharedMaterial.SetFloat("_OutlineWidth", 0);
        UpdateText();
        StartCoroutine(StartRoutine());
    }


    IEnumerator StartRoutine()
    {
        sound.PlaySound(sound.reverse_bell);
        yield return new WaitForSeconds(2);
        tutText.gameObject.SetActive(true);
        tutText.text = "Write a Name";

        StartLevel();
        tempMarker.gameObject.SetActive(true);
    }

     
    public void WinGame() 
    {
        gameStarted = false;
        print("you win!");
        ShowBigText("You have learned "+knowledge+" things.", 5);
        StartCoroutine("WaitToShowLoseText");

    }

    public void LoseGame()
    {
        print("you lose!");
        ShowBigText("You lost your mind. Oh well.", 5);
        StartCoroutine("IncreaseWhisper");
        StartCoroutine("InsanityChanges");
        gameStarted = false;
        StartCoroutine("WaitToShowLoseText");
    }

    IEnumerator WaitToShowLoseText()
    {
        yield return new WaitForSeconds(4);
        tutText.gameObject.SetActive(true);
        tutText.text = "Type learn to begin again.";
    }


    public void NewLevel()
    {
        StopCoroutine("IncreaseWhisper");
        StopInsanity();
        mixer.SetFloat("whispervolume", -20f);
        mixer.SetFloat("whisperdistortion", 0);

        waitingForNewLevel = false;

        monsterToKeep = curNode.monster;
        ClearClouds();
        mapGen.DeleteMap();

        hasBegunCountdown = false;
        markerEmission.rateOverTime = (10 - insanity);

        if (insanity > 11)
        {
            LoseGame();
        }
        else if(usableMonsterList.Count > 0)
        {
            StartLevel();
        }
        else
        {
            WinGame();
        }
    }

    void ClearClouds()
    {
        for (int i = 0; i < clouds.Count; i++)
        {
            Destroy(clouds[i].particles.gameObject);
            Destroy(clouds[i].gameObject);
        }
        clouds.Clear();
    }

    public void StartLevel()
    {
        Node startNode;
        if (monsterToKeep != null)
        {
            mapGen.CreateMapCorrect(knowledge, monsterToKeep);
            startNode = mapGen.mapnodes.Find(x => x.monster == monsterToKeep);
        }
        else
        {
            mapGen.CreateMapCorrect(knowledge);
            startNode = mapGen.mapnodes[Random.Range(0, mapGen.mapnodes.Count)];
        }

        SetCurNode(startNode);

        StartCoroutine("IncreaseWhisper");
        StartCoroutine("InsanityChanges");
    }

    public void SetCurNode(Node n)
    {
        curNode = n;
        curNode.SetTravelled(true);
        MoveMarker(n.transform.position - Vector3.forward*6);
        foreach (Node nc in n.connections)
        {
            nc.visText.gameObject.SetActive(true);
        }
    }

    public void MoveMarker(Vector3 pos)
    {
        StartCoroutine(MarkerMoveRoutine(pos));
        StartCoroutine(SpinMarker());
    }

    IEnumerator MarkerMoveRoutine(Vector3 endPos)
    {
        Vector3 startPos = tempMarker.position;
        float t = 0;
        while(t < 1)
        {
            tempMarker.position = Vector3.Lerp(startPos, endPos, moveCurve.Evaluate(t));
            t += Time.deltaTime * 2f;
            yield return new WaitForEndOfFrame();
        }

        //while (t < 1)
        //{
        //    tempMarker.rotation = Quaternion.SlerpUnclamped(startPos, endPos, moveCurve.Evaluate(t));
        //    t += Time.deltaTime * 2f;
        //    yield return new WaitForEndOfFrame();
        //}


        tempMarker.position = endPos;
    }

    public IEnumerator SpinMarker()
    {
        float t = 0;
        Quaternion startRot = tempMarker.rotation;
        float orgAngle;

        Vector3 orgAxis;
        startRot.ToAngleAxis(out orgAngle, out orgAxis);
        float targetAngle = orgAngle + 90;
        
        float currentAngle;
        while (t < 1)
        {
            currentAngle = Mathf.LerpUnclamped(orgAngle, targetAngle, spinCurve.Evaluate(t));
            tempMarker.rotation = Quaternion.AngleAxis(currentAngle, Vector3.forward);

            
            t += Time.deltaTime * 1.8f;

            yield return new WaitForEndOfFrame();
        }
        tempMarker.rotation = Quaternion.AngleAxis(targetAngle, Vector3.forward);

    }


    public void CheckConnection(string s)
    {
        ExtraTypingChecks(s);

        foreach (Node n in curNode.connections)
        {
            
            if (s.ToLower() == n.monster.mName.ToLower())
            {
                TravelToNode(n);
            }
        }
    }

    public void ExtraTypingChecks(string s)
    { 
        if (!gameStarted)
        {
            if (s == "learn")
            {
                GameStart();
            }
        }
        if(s == "quit")
        {
            Application.Quit();
        }
        if (s == "help")
        {
            ShowBigText("Help? Funny.", 2);
        }

    }


    public void TravelToNode(Node n)
    {
        knowledge++;
        UpdateText();
        if (tutText.gameObject.activeInHierarchy)
        {
            tutText.gameObject.SetActive(false);
        }

        if (!n.travelled)
        {

            SpawnDeathCloud(n);
        }

        if (!hasBegunCountdown)
        {
            sound.PlaySound(sound.slam);
            hasBegunCountdown = true;
        }

        foreach(Node nn in mapGen.mapnodes)
        {
            nn.visText.gameObject.SetActive(false);
        }

        SetCurNode(n);

 

        if (curNode.monster.epithets.Count > 0)
        {
            ShowBigText(curNode.monster.epithets[Random.Range(0,curNode.monster.epithets.Count)], 2);
            sound.PlaySound(sound.slam_low);
        }
        else
        {
            ShowBigText("Unknown Horror",2);

        }

        DoDoneCheck();

    }


    public bool DoDoneCheck()
    {
        if (CheckIfDone())
        {
            StartCoroutine(WaitForNewLevel(2, true));

            //sound here! :D reverse bell?

            return true;
        }
        else
        {
            return false;
        }
    }

    bool waitingForNewLevel = false;
    IEnumerator WaitForNewLevel(float time, bool fromWin)
    {
        StopCoroutine("IncreaseWhisper");

        if (waitingForNewLevel)
        {

        }
        else
        {
            waitingForNewLevel = true;

            if (fromWin)
            {
                sound.PlaySound(sound.reverse_bell);
            }
            yield return new WaitForSeconds(time - 1);
            float t = 0;
            while (t < 1)
            {
                bigText.fontSharedMaterial.SetFloat("_OutlineWidth", t);
                bigText.color = new Color(bigText.color.r, bigText.color.g, bigText.color.b, 1-t);
                t += Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }
            bigText.fontSharedMaterial.SetFloat("_OutlineWidth", 0);
            bigText.color = new Color(bigText.color.r, bigText.color.g, bigText.color.b, 1);

            NewLevel();
        }
    }

     
    public void UpdateText()
    {
        knowText.text = "" + knowledge;
        alertText.fontSize = knowledge;
    }


    public bool CheckIfDone()
    {
        if(mapGen.mapnodes.Exists(x=>x.travelled == false))
        {
            return false;
        }
        else
        {
            return true;
        }

    }



    public void SpawnDeathCloud(Node n)
    {
        GameObject g = (GameObject)Instantiate(deathCloudPrefab, n.transform.position, Quaternion.identity);
        GameObject p = (GameObject)Instantiate(deathCloudParticlesPrefab, n.transform.position, Quaternion.identity);
        DeathCloud d = g.GetComponent<DeathCloud>();
        d.particles = p.GetComponent<ParticleSystem>();
        d.growRate = startGrowrate;
        d.BeginGrow(this,n);
        clouds.Add(d);
    }



    public void ShowBigText(string s, float time)
    {
        bigText.text = s;
        bigText.gameObject.SetActive(true);
        knowText.gameObject.SetActive(true);
        
        StopCoroutine("BigTextTimer");
        StartCoroutine("BigTextTimer",time);
    }

    IEnumerator BigTextTimer(float time)
    {

        yield return new WaitForSeconds(time);
        bigText.gameObject.SetActive(false);
        knowText.gameObject.SetActive(false); 
        
    }
    


    IEnumerator IncreaseWhisper()
    {
        while (true)
        {
           // print("sound");
            float totalCloudSize = 0;
            foreach (DeathCloud c in clouds)
            {
                totalCloudSize += c.transform.localScale.x;
            }

            for (int i = 0; i < clouds.Count; i++)
            {
                whispers *= 0.8f;
            }

        //    whispers *= (clouds.Count*0.8f);

            whispers = Mathf.Clamp((totalCloudSize / 40f), 0, 1);


            float distAmount = whispers;
            float volAmount =  (Mathf.Clamp(totalCloudSize / 10f,0,1) *20f - 20f); 

            mixer.SetFloat("whispervolume", volAmount);

            mixer.SetFloat("whisperdistortion", distAmount);

            if(distAmount == 1)
            {
                insanity += 3;
                StartCoroutine(WaitForNewLevel(0.3f, false));
            }

            yield return new WaitForSeconds(0.2f);

        }
    }


    void StopInsanity()
    {
        StopCoroutine("InsanityChanges");
        StopCoroutine("TextSwap");
        StopCoroutine("DescRoll");
        alertText.text = "";
    }

    IEnumerator InsanityChanges()
    {

        chance = insanity * (whispers*10);


        bool hasBegunTextswapping = false;
        bool hasBegunTextWriting = false;


        while (true)
        {
            chance = insanity * (whispers * 10);
           // print("chance " + chance);

            if (chance > 5 && !hasBegunTextswapping) //can do text swap
            {
                hasBegunTextswapping = true;
               // print("starting!");
                StartCoroutine("TextSwap");

            }

            if (chance > 3 && !hasBegunTextWriting) //can do text swap
            {
                hasBegunTextWriting = true;
                // print("starting!");
                StartCoroutine("DescRoll");

            }
            yield return new WaitForEndOfFrame();
        }


        //write out the description somewhere with high enough chance. 

        
    }

    IEnumerator TextSwap()
    {
        while (true)
        {
            foreach (Node n in mapGen.mapnodes)
            {
                float r = Random.Range(0, 500);

                if (r < chance)
                {
                    string tToChange = n.visText.text;
                    char[] what = tToChange.ToCharArray();

                    what[Random.Range(0, tToChange.Length)] = letters[Random.Range(0, letters.Length)];
                    n.visText.text = new string(what);
                    //sound.PlaySound(sound.noise);
                    yield return new WaitForSeconds(Util.Map(chance, 0, insanity * 10, 1, 0.1f));
                   // sound.StopSound(sound.noise);
                    n.visText.text = n.monster.mName;
                }
            }
            yield return new WaitForSeconds(Util.Map(chance, 0, insanity * 10, 1, 0.01f));
        }
    }



    IEnumerator DescRoll()
    {
        foreach (Node n in mapGen.mapnodes)
        { 
            string desc = n.monster.mName+ ": "+ n.monster.desc;

            char[] arrayofDesc = desc.ToCharArray();

            for (int i = 0; i < arrayofDesc.Length; i++)
            {
                sound.PlayWriting();

                alertText.text += arrayofDesc[i]; 
                yield return new WaitForSeconds(Util.Map(chance, 0, insanity * 10, 0.5f, 0.001f));
            }
            alertText.text += " ";
            yield return new WaitForSeconds(1f);
        }
          
    }





}
