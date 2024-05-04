using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshTester : MonoBehaviour
{
    
    // Start is called before the first frame update
    void Start()
    {
        Block.AIR.getBlockAttributes();

        Mesh m = new Mesh();
        MeshFilter mf = gameObject.GetComponent<MeshFilter>();
        MeshRenderer mr = gameObject.GetComponent<MeshRenderer>();
        mr.material.shader = Shader.Find("Particles/Standard Surface");//SetType("_Color", new Color(1,0,0));

        Vector3[] verts = new Vector3[]
        {
            new(0,0,0),
            new(0,0,1),
            new(1,0,1),
            new(1,0,0),

            new(1,0,1),
            new(1,0,2),
            new(2,0,2),
            new(2,0,1),


        };
        int[] quads = new int[]
        {
            0,1,2,3,
            4,5,6,7

        };
        Color[] cols = new Color[]
        {
            new(1,0,0),
            new(1,1,0),
            new(0,1,1),
            new(0,0,1),

            new(1,1,0),
            new(1,1,0),
            new(0,1,1),
            new(0,1,1)
        };
        m.vertices = verts;
        m.SetIndices(quads, MeshTopology.Quads,0);
        m.SetColors(cols);

        
        //m. = quads;

        m.RecalculateNormals();
        m.RecalculateTangents();
        mf.mesh = m;
    }
}
