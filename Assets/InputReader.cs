using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class InputReader : MonoBehaviour {


    public GameManager gm;

    public string readString = "";

    public TextMeshProUGUI inputText;

	// Use this for initialization
	void Start () {
        inputText.text = "";
	}
	
	// Update is called once per frame
	void Update () {


        if (Input.inputString != "")
        {
            if (Input.GetKeyDown(KeyCode.Backspace))
            {
                if (readString == "")
                {
                    return;
                }
                readString = readString.Remove(readString.Length - 1, 1);
            }
            else if (Input.GetKeyDown(KeyCode.Return))
            {
                if (readString == "")
                {
                    return;
                }
                //print("TO CHECK|" + readString[0] + "|");
                gm.CheckConnection(readString);
                readString = "";
                inputText.text = "";
            }
            else
            {
                if (!Input.GetKey(KeyCode.Backspace))
                {
                    //StartCoroutine(PlayNoiseBurst());
                    gm.sound.PlayWriting();
                    readString += Input.inputString;
                }

            }

            // gm.CheckTextsForInput(readString);
            inputText.text = readString;
        }
        

    }


    IEnumerator PlayNoiseBurst()
    {
        gm.sound.PlaySound(gm.sound.noise);
        gm.sound.noise.pitch = Random.Range(0.8f, 1.2f);
        yield return new WaitForSeconds(0.1f);
        gm.sound.StopSound(gm.sound.noise);
    }

 


}
