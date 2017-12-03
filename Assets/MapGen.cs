using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGen : MonoBehaviour {

    public GameManager gm;

    public GameObject nodePrefab;
    public GameObject linePrefab;

    public int minNrOfNodes = 10;
    public int maxNrOfNodes = 20;

    public float xRange = 50f;
    public float yRange = 50f;

    public int maxConnections = 3;

    public float lineThickness = 0.1f;

    public List<Node> mapnodes = new List<Node>();
    public List<GameObject> lines = new List<GameObject>();

    Node testOriginNode = null;

    public void Start()
    {
        //CreateMap();
    }

    public void Update()
    {

    }


    public void DeleteMap()
    {
        for (int i = 0; i < mapnodes.Count; i++)
        {
            Destroy(mapnodes[i].gameObject);
        }
        mapnodes.Clear();

        for (int i = 0; i < lines.Count; i++)
        {
            Destroy(lines[i]);
        }
        lines.Clear();

    }


    public void DetermineDifficulty(int knowledge)
    {
        //knowledge is counted as a number between 0 and 150 (know it can go higher, but clamp it.

        //so at 0, nodes should be 2-3.
        //at 100 they should be 10-15 or something.

        minNrOfNodes = (int)Mathf.Clamp(Util.Map(knowledge, 0, 60, 2, 5),2,10);
        maxNrOfNodes = (int)Mathf.Clamp(Util.Map(knowledge, 0, 100, 5, 10),5,20);

    }


    public void CreateMapCorrect(int knowledge, Monster startmonster = null)
    {
        
        CreateMap(knowledge, startmonster);
        testOriginNode = mapnodes[0];
        foreach(Node n in mapnodes)
        {
            if (!IsConnectedToDesignatedOriginNode(n))
            {
                print("is not connected! Connecting " + n.monster.mName + " to " + testOriginNode.monster.mName);
                n.connections.Add(testOriginNode);
                testOriginNode.connections.Add(n);

                //draw line 
                DrawLine(n, testOriginNode);
            }
        }
    }

    public bool IsConnectedToDesignatedOriginNode(Node n)
    {
       // print("-------------- testing " + n.monster.mName);
        if(n == testOriginNode)
        {
           // print("is origin");
            return true;
        }
        if (n.connections.Exists(x => x == testOriginNode))
        {
          //  print("is directly connected");

            return true;
        }
        for (int i = 0; i < n.connections.Count; i++)
        {
           if(n.connections[i].connections.Exists(x=>x == testOriginNode))
            {
             //   print("is secondarily connected");

                return true;
            }
            for (int j = 0; j < n.connections[i].connections.Count; j++)
            {
                if (n.connections[i].connections[j].connections.Exists(x => x == testOriginNode))
                {
                 //   print("is tertiarily connected");

                    return true;
                }
            }
        }
      //  print("is not connected");

        return false;
    }



    public void CreateMap(int knowledge, Monster startMonster = null)
    {
        List<Node> nodes = new List<Node>();
        //first place a bunch of cubes in random spots.


        DetermineDifficulty(knowledge);


        int r = Mathf.Clamp(Random.Range(minNrOfNodes, maxNrOfNodes), 0, gm.usableMonsterList.Count);



        if (startMonster != null)
        {
            r++; //plus one because that's the cur node
        }
        
        if(r > gm.usableMonsterList.Count)  //looks silly to double clamp. but for some reason, it's necessary?????? dunno
        {
            r = gm.usableMonsterList.Count; 
        }
        if(gm.usableMonsterList.Count == (r)) //include the last one if we hit an unlucky roll. because a level with one node is silly.
        {
            r++;
        }

        for (int i = 0; i < r; i++)
        {
            GameObject g = (GameObject)Instantiate(nodePrefab);
            g.transform.position = FindPosition(nodes);
            Node n = g.GetComponent<Node>();
            Monster m; 
            if (startMonster != null && i == 0)
            {
                m = startMonster;
            }
            else
            {
                m = gm.usableMonsterList[Random.Range(0, gm.usableMonsterList.Count)];

            }
            n.Init(m);
            gm.usableMonsterList.Remove(m);
            nodes.Add(n);
        }


        //then draw lines between some of them. Everyone should have at least 1 connection
        //for a random nr of times, at least 1, we make a connection with another node.
        //if a connection is already there, skip creating it again ( this is fine. only NEED one. ) Have to assign it To Both.
        foreach (Node n in nodes)
        {
            int connections = Random.Range(1, Mathf.Min(maxConnections,nodes.Count));

            for (int i = 0; i < connections; i++)
            {
                Node otherNode = FindAnotherNode(n, nodes);
                if (!n.connections.Contains(otherNode))
                {
                    n.connections.Add(otherNode);
                    otherNode.connections.Add(n);

                    //draw line
                    DrawLine(n, otherNode);
                }
            }
        }
        mapnodes = nodes;
    }

    public Node FindAnotherNode(Node me, List<Node> nodes)
    {
        int r = Random.Range(0, nodes.Count);
        while(nodes[r] == me)
        {
            r = Random.Range(0, nodes.Count);
        }
        return nodes[r];
    }


    public void DrawLine(Node start, Node end)
    {
        GameObject line = (GameObject)Instantiate(linePrefab);

        //draw vector between the two points. half of that is midpoint. that's where we place line.
        Vector3 throughNode = end.transform.position - start.transform.position;
        throughNode = throughNode.normalized * (Vector3.Distance(start.transform.position,end.transform.position)/2);
        line.transform.position = start.transform.position + throughNode;

        Quaternion newRot = Quaternion.LookRotation((end.transform.position - start.transform.position), Vector3.forward);
        newRot.x = 0;
        newRot.y = 0;
        line.transform.rotation = newRot;

        line.transform.localScale = new Vector3(lineThickness, throughNode.magnitude*2, 1);

        lines.Add(line);
    }


    public Vector3 FindPosition(List<Node> nodes)
    {
        bool yes = false;
        Vector3 pos = Vector3.zero;
        int i = 0;
        while (!yes)
        {
            
            pos = new Vector3(Random.Range(-xRange, xRange), Random.Range(-yRange, yRange), 0);
            if (TestPos(pos, nodes))
            {
                yes = true;
            }
            i++;
            if(i >= 50)
            {
                yes = true;
            }
        }

        return pos;
    }

    bool TestPos(Vector3 pos, List<Node> nodes)
    {
        foreach (Node n in nodes)
        {
            if (Vector3.Distance(n.gameObject.transform.position, pos) < 5)
            {
                return false;
            }

        } //Oh fuck, it can place itself correctly according to one, then go to another node check and then replace itself, but never returning to check that it has replaced itself correctly to the previous. oh god.
        return true;
    }
    //


}


//int i = 0;
//            //print(n.name+" "+Vector3.Distance(n.gameObject.transform.position, pos));





//            while(Vector3.Distance(n.gameObject.transform.position, pos) < 5)
//            {
//                //print(n.transform.position + " " + pos);
//                pos = new Vector3(Random.Range(-xRange, xRange), Random.Range(-yRange, yRange), 0);
//                i++;
//                //print(i);
//                if (i > 50)
//                {
//                    return pos;
//                }
//            }