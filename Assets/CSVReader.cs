using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class CSVReader : MonoBehaviour {

    public string fileName = "theoldones";

    public List<Monster> monsters = new List<Monster>();

	// Use this for initialization
	void Start () {
        
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public List<Monster> LoadCSV()
    {
        StreamReader streamReader = new StreamReader(new MemoryStream((Resources.Load(fileName) as TextAsset).bytes));
        //  TextAsset raw = Resources.Load<TextAsset>(fileName);

        while (!streamReader.EndOfStream)
        {

            string line = streamReader.ReadLine();

            string[] fields = line.Split(';');
           // print(line);
            //print(fields[0]);
            //print(fields[1]);

            if (fields[0] == "Name")
            {
                continue;
            }

            if (fields[0] == string.Empty || fields[0] == null)
            {
                //have to deal with it otherwise! means it's a strange one. look at that next.
                if(!fields[1].Contains("name(s)"))
                {
                   // print("ADDING SPECIAL EPITHET TO " + monsters[monsters.Count-1].name + " " + fields[1]); 
                    monsters[monsters.Count-1].epithets.Add(RemoveBrackets(fields[1]));
                }

                continue;
            }

            Monster nm = new Monster();

            if (fields[0].Contains("["))
            {
                string nam = fields[0].Remove(fields[0].IndexOf('['));
                nm.mName = nam;
            }
            else
            {
                nm.mName = fields[0];
            }

            if (fields[1] != "—")
            {
                string epit = RemoveBrackets(fields[1]);
                string[] epits = epit.Split(',');

                foreach(string s in epits)
                {
                    if (!string.IsNullOrEmpty(s))
                    {
                        nm.epithets.Add(s);
                    }
                }
            }
            nm.desc = RemoveBrackets(fields[2]);
            monsters.Add(nm);
            
           // print("Added " + nm.name);

        }

        print("Loaded all monsters " + monsters.Count);
        return monsters;
    }



    public string RemoveBrackets(string s)
    {
        if (s.Contains("["))
        {
            return s.Remove(s.IndexOf('['));
        }
        else
        {
            return s;
        }
    }
}



[System.Serializable]
public class Monster
{
    public string mName;
    public List<string> epithets = new List<string>();
    public string desc;
}
