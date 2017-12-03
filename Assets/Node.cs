using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Node : MonoBehaviour {

    public List<Node> connections = new List<Node>();
    public Monster monster;
    public TextMeshPro visText;
    public Renderer rend;
    public ParticleSystem particles;
    public ParticleSystem.MainModule particleMain;

    public bool travelled = false; 

    public void Init(Monster mons)
    {
        monster = mons;
        visText.text = monster.mName;
        visText.gameObject.SetActive(false);
        rend = particles.GetComponent<Renderer>();
        rend.material.color = Util.blue;
        particleMain = particles.main;
        particleMain.startColor = Util.blue;
    }

    public void SetTravelled(bool t)
    {
        travelled = t;
        rend.material.color = Util.green;
        particleMain.startColor = Util.green;
    }


}
