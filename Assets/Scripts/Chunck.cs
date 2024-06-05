﻿//using System.Collections;
//using System.Collections.Generic;
using UnityEngine;
using System.Collections.Generic;

public class Chunck : MonoBehaviour
{
    public Mesh chunk;

    public int xSize, zSize;

    public float seedX,seedZ;

    public float MountainDensity;

    public Gradient biomes;

    public MeshFilter filter;

    public GameObject trees;
    public GameObject stones;
    public GameObject foodSource;
    public GameObject City;
    public GameObject OceanTemple;

    public Generator home;
    public string key;

    public bool Modified;

    //function variables
    public float a, b;
    public void StartChunk()
    {
        chunk = new Mesh();

        filter.mesh = chunk;
    }

    public void GenerateChunk()
    {
        Vector3[] verts = new Vector3[(xSize + 1) * (zSize + 1)];
        Color[] colors = new Color[verts.Length];

        bool e = true;

        for (int i = 0, z = 0; z <=zSize; z++)
        {
            for (int x = 0; x <= xSize; x++)
            {
                //ocean map
                float oceans = Mathf.PerlinNoise((x + seedX) * .003f, (z + seedZ) * .003f);

                float perlin = Mathf.PerlinNoise((x + seedX) * .03f, (z + seedZ) * .03f) * oceans;

                float Nx = Mathf.Floor(perlin * a);
                float Dx = (Nx + 1) / a - perlin;

                float rezult = Nx / a;

                if(Dx<=b)
                {
                    float Kx = 1.0f / a - Dx / (a * b);
                    rezult += Kx;
                }

                if (perlin >= MountainDensity)
                {
                    float delta = perlin - MountainDensity;
                    float d2 = (6.0f * delta * delta) / (1.0f - MountainDensity);

                    rezult *= 1.0f + d2 + Mathf.PerlinNoise((x + seedX) * .3f, (z + seedZ) * .3f) * d2;
                }

                verts[i] = new Vector3(x / 2.0f, rezult * 10.0f, z / 2.0f);

                //structure
                bool struc = false;
                if (Nx == 2 && perlin <= 0.3f) 
                {
                    if (x % 35 == 0 && z % 45 == 0 && x >= 35 && z >= 45) 
                    {
                        if (IsStructure(x, z)) 
                        {
                            Vector3 pos = new Vector3(x / 2.0f, rezult * 10.0f, z / 2.0f) + gameObject.transform.position;
                            GameObject t = Instantiate(City, pos, Quaternion.Euler(0, Random.Range(0, 180), 0));
                            t.transform.parent = gameObject.transform;
                            struc = true;
                            t.name = x.ToString() + "_" + z.ToString();
                        }
                    }
                }

                if (Nx == 0 && perlin <= 0.1f && e)  
                {
                    if (x % 25 == 0 && z % 25 == 0 && x >= 50 && z >= 75)
                    {
                        if (IsStructure(x, z))
                        {
                            Vector3 pos = new Vector3(x / 2.0f, rezult * 10.0f, z / 2.0f) + gameObject.transform.position;
                            GameObject t = Instantiate(OceanTemple, pos, Quaternion.Euler(0, Random.Range(0, 180), 0));
                            t.transform.parent = gameObject.transform;
                            struc = true; e = false;
                            t.name = x.ToString() + "_" + z.ToString();
                        }
                    }
                }

                //natural resources
                if (Nx == 2 && perlin >= 0.37f && !struc)
                {
                    if (x % 10 == 0 && z % 10 == 0)
                    {
                        if (x <= xSize - 10 && z >= 10)
                        {
                            if (IsStructure(x, z))
                            {
                                float add = Mathf.PerlinNoise((x + seedX) * .5f, (z + seedZ) * .5f);
                                Vector3 pos = new Vector3(x / 2.0f - add, rezult * 10.0f, z / 2.0f + add) + gameObject.transform.position;

                                if (perlin <= 0.42f)
                                {
                                    GameObject t = Instantiate(trees, pos, Quaternion.Euler(0, Random.Range(0, 180), 0));
                                    t.transform.localScale = new Vector3(1, Random.Range(0.8f, 1.2f), 1);
                                    t.transform.parent = gameObject.transform;
                                    t.name = x.ToString() + "_" + z.ToString();
                                }
                                else
                                {
                                    GameObject t = Instantiate(stones, pos, Quaternion.Euler(0, Random.Range(0, 180), 0));
                                    t.transform.parent = gameObject.transform;
                                    t.name = x.ToString() + "_" + z.ToString();
                                }
                            }
                        }
                    }
                    else
                    {
                        if (x % 15 == 0 && z % 15 == 0 && perlin <= 0.42f && x <= xSize - 15 && z >= 15)
                        {
                            if (IsStructure(x, z))
                            {
                                float add = Mathf.PerlinNoise((x + seedX) * .5f, (z + seedZ) * .5f);
                                Vector3 pos = new Vector3(x / 2.0f - add, rezult * 10.0f, z / 2.0f + add) + gameObject.transform.position;

                                GameObject t = Instantiate(foodSource, pos, Quaternion.Euler(0, Random.Range(0, 180), 0));
                                t.transform.parent = gameObject.transform;
                                t.name = x.ToString() + "_" + z.ToString();
                            }
                        }
                    }
                }

                colors[i] = biomes.Evaluate(rezult);

                i++;
            }
        }

        if (home.Buildings.ContainsKey(key)) 
        {
            for (int i = 0; i < home.Buildings[key].Count; i++)
            {
                int id = (int)home.Buildings[key][i].w;
                GameObject newBuild = Instantiate(home.Manager.builds[id].Object, new Vector3(home.Buildings[key][i].x, home.Buildings[key][i].y, home.Buildings[key][i].z), Quaternion.identity);
                newBuild.transform.SetParent(gameObject.transform);
                newBuild.name = id.ToString() + "build";
            }
        }

        //chunk.Clear();

        chunk.vertices = verts;
        chunk.colors = colors;
    }

    bool IsStructure(int x, int y)
    {
        if(Modified)
        {
            if (home.Modifications[key].Contains(x.ToString() + "_" + y.ToString())) 
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        else
        {
            return true;
        }
    }
}
