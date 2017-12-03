using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathCloud : MonoBehaviour {

    public GameManager gm;

    public ParticleSystem particles;
    public ParticleSystem.ShapeModule particleShape;
    public ParticleSystem.EmissionModule emission;
    public ParticleSystem.MainModule partmain;
    public float startrate = 40f;
    public float sizeStart = 0.3f;
    public float growingrate = 0f;

    public float growRate = 0.01f;

    public Node originNode;


    public void BeginGrow(GameManager g, Node on)
    {
        originNode = on;
        gm = g;
        particleShape = particles.shape;
        emission = particles.emission;
        partmain = particles.main;

        transform.localScale = new Vector3(0.25f, 0.25f, 0.1f);
        particleShape.radius = 0.5f;


        StartCoroutine("Grow");
    }


    IEnumerator Grow()
    {
        while (true)
        {
            transform.localScale += new Vector3(growRate, growRate, 0);
            //particles.transform.localScale += new Vector3(growRate, growRate, 0);
            particleShape.radius += growRate / 2f;
            growingrate += growRate * 7f;
            partmain.startSize = new ParticleSystem.MinMaxCurve(Mathf.Clamp(sizeStart + (growingrate / 35f),0,4));
            emission.rateOverTime = new ParticleSystem.MinMaxCurve(startrate+growingrate);
            
            yield return new WaitForSeconds(0.01f);
        }
    }


    //public void OnTriggerEnter(Collider other)
    //{
    //    if (other.CompareTag("Node"))
    //    {
    //        Node n = other.gameObject.GetComponent<Node>();
    //        if(n != originNode)
    //        {
    //            print(originNode.monster.mName + " hit " + n.monster.mName);
    //            //n.SetTravelled(true);
    //            gm.insanity++;
    //            // print("collided with node!");

    //            gm.DoDoneCheck();
    //        }
    //    }
    //}





}
