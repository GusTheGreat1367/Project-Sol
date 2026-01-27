using System.Collections.Generic;
using UnityEngine;

public class ParrallaxTest : MonoBehaviour
{
    public List<GameObject> prefabs;
    public CameraMovement cam;
    public Vector2 camSpace;
    public List<List<GameObject>> starLayers = new List<List<GameObject>>();
    public float layerSpeedMultiplier = 1.0f;
    public Vector2 realCamSpace;
    public System.Random rnd = new System.Random();
    public float sizeMultiplier = 1.0f;
    public int starsPerLayer = 15;
    public int layersNum = 5;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Vector2 camPos = cam.gameObject.transform.position;
        if (cam.cam.orthographic)
        {
            float ySize = cam.cam.orthographicSize * 2f;
            float xSize = ySize * cam.cam.aspect;
            float padding = 5f;
            camSpace = new Vector2(xSize + padding, ySize + padding);
            realCamSpace = new Vector2(xSize, ySize);
        }
        else
        {
            Debug.LogError("Ortho Camera not detected!");
        }
        List<List<GameObject>> layers = new List<List<GameObject>>();
        for (int i = 0; i < layersNum; i++)
        {
            List<GameObject> layer = new List<GameObject>();
            for (int j = 0; j < starsPerLayer; j++)
            {
                layer.Add(Instantiate(prefabs[rnd.Next(0, prefabs.Count)], new Vector2(UnityEngine.Random.Range(Mathf.RoundToInt(camPos.x - (camSpace.x / 2)), Mathf.RoundToInt(camPos.x + (camSpace.x / 2))), UnityEngine.Random.Range(Mathf.RoundToInt(camPos.y - (camSpace.y / 2)), Mathf.RoundToInt(camPos.y + (camSpace.y / 2)))), Quaternion.identity));
                layer[j].transform.parent = cam.gameObject.transform;
                Vector2 currentObjLocalScale = layer[j].transform.localScale;
                layer[j].transform.localScale = new Vector2(Mathf.Clamp(currentObjLocalScale.x / (j + 1 * sizeMultiplier), 0.0001f, 1f), Mathf.Clamp(currentObjLocalScale.y / (j + 1 * sizeMultiplier), 0.0001f, 1f));
                SpriteRenderer spriteRenderer = layer[j].GetComponent<SpriteRenderer>();
                spriteRenderer.sortingOrder = layersNum - i - 1000;
            }
            layers.Add(layer);
        }
        starLayers = layers;
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 camPos = cam.gameObject.transform.position;
        if (cam.cam.orthographic)
        {
            float ySize = cam.cam.orthographicSize * 2f;
            float xSize = ySize * cam.cam.aspect;
            float padding = 5f;
            camSpace = new Vector2(xSize + padding, ySize + padding);
            realCamSpace = new Vector2(xSize, ySize);
        }
        else
        {
            Debug.LogError("Ortho Camera not detected!");
        }

    }
    public void Move(Vector2 speed)
    {
        Vector2 camPos = cam.gameObject.transform.position;
        List<GameObject> objsToDestroy = new List<GameObject>();
        for (int i = 0; i < starLayers.Count; i++)
        {
            float layerSpeed = (layerSpeedMultiplier * (starLayers.Count - i + 1));
            for (int j = 0; j < starLayers[i].Count; j++)
            {
                starLayers[i][j].transform.Translate(-speed * layerSpeed * Time.deltaTime);
                if (!DetectObstaclesInPosition.ContainsPoint(new Float2(camPos.x, camPos.y), new Float2(camSpace.x, camSpace.y), new Float2(starLayers[i][j].transform.position.x, starLayers[i][j].transform.position.y)).any)
                {
                    objsToDestroy.Add(starLayers[i][j]);
                }
            }
        }
        for (int i = 0; i < objsToDestroy.Count; i++)
        {
            for (int j = 0; j < starLayers.Count; j++)
            {
                if (starLayers[j].Contains(objsToDestroy[i]))
                {
                    ContainsPointReturn returnVal = DetectObstaclesInPosition.ContainsPoint(new Float2(camPos.x, camPos.y), new Float2(camSpace.x, camSpace.y), new Float2(objsToDestroy[i].transform.position.x, objsToDestroy[i].transform.position.y));
                    starLayers[j].Remove(objsToDestroy[i]);
                    Vector2 spawnPos = new Vector2();
                    int randomNum = rnd.Next(0, 2);
                    bool randomBool = false;
                    if (randomNum == 0)
                    {
                        randomBool = true;
                    }
                    else
                    {
                        randomBool = false;
                    }
                    if (returnVal.s1 && returnVal.s2)
                    {
                        if (randomBool)
                        {
                            spawnPos = new Vector2(UnityEngine.Random.Range(camPos.x + (camSpace.x / 2), camPos.x + camSpace.x), camPos.y - (camSpace.y / 2));
                        }
                        else
                        {
                            spawnPos = new Vector2(camPos.x + (camSpace.x / 2), UnityEngine.Random.Range(camPos.y - camSpace.y, camPos.y - (camSpace.y / 2)));
                        }

                    }
                    else if (returnVal.s2 && returnVal.s3)
                    {
                        if (randomBool)
                        {
                            spawnPos = new Vector2(UnityEngine.Random.Range(camPos.x - (camSpace.x / 2), camPos.x - camSpace.x), camPos.y - (camSpace.y / 2));
                        }
                        else
                        {
                            spawnPos = new Vector2(camPos.x - (camSpace.x / 2), UnityEngine.Random.Range(camPos.y - camSpace.y, camPos.y - (camSpace.y / 2)));
                        }
                    }
                    else if (returnVal.s3 && returnVal.s4)
                    {
                        if (randomBool)
                        {
                            spawnPos = new Vector2(UnityEngine.Random.Range(camPos.x - (camSpace.x / 2), camPos.x - camSpace.x), camPos.y + (camSpace.y / 2));
                        }
                        else
                        {
                            spawnPos = new Vector2(camPos.x - (camSpace.x / 2), UnityEngine.Random.Range(camPos.y + camSpace.y, camPos.y + (camSpace.y / 2)));
                        }
                    }
                    else if (returnVal.s4 && returnVal.s1)
                    {
                        if (randomBool)
                        {
                            spawnPos = new Vector2(UnityEngine.Random.Range(camPos.x + (camSpace.x / 2), camPos.x + camSpace.x), camPos.y + (camSpace.y / 2));
                        }
                        else
                        {
                            spawnPos = new Vector2(camPos.x + (camSpace.x / 2), UnityEngine.Random.Range(camPos.y + camSpace.y, camPos.y + (camSpace.y / 2)));
                        }
                    }
                    else
                    {
                        if (returnVal.s1)
                        {
                            spawnPos.x = camPos.x + (camSpace.x / 2);
                        }
                        else if (returnVal.s3)
                        {
                            spawnPos.x = camPos.x - (camSpace.x / 2);
                        }
                        else
                        {
                            spawnPos.x = UnityEngine.Random.Range(camPos.x - (realCamSpace.x / 2), camPos.x + (camSpace.x / 2));
                        }
                        if (returnVal.s2)
                        {
                            spawnPos.y = camPos.y - (camSpace.y / 2);
                        }
                        else if (returnVal.s4)
                        {
                            spawnPos.y = camPos.y + (camSpace.y / 2);
                        }
                        else
                        {
                            spawnPos.y = UnityEngine.Random.Range(camPos.y - (camSpace.y / 2), camPos.y + (camSpace.y / 2));
                        }
                    }
                    GameObject instantiatedParrallaxObj = Instantiate(prefabs[rnd.Next(0, prefabs.Count)], spawnPos, Quaternion.identity);
                    instantiatedParrallaxObj.transform.parent = cam.gameObject.transform;
                    Vector2 currentObjLocalScale = instantiatedParrallaxObj.transform.localScale;
                    instantiatedParrallaxObj.transform.localScale = new Vector2(Mathf.Clamp(currentObjLocalScale.x / (j + 1 * sizeMultiplier), 0.0001f, 1f), Mathf.Clamp(currentObjLocalScale.y / (j + 1 * sizeMultiplier), 0.0001f, 1f));
                    SpriteRenderer spriteRenderer = instantiatedParrallaxObj.GetComponent<SpriteRenderer>();
                    spriteRenderer.sortingOrder = starLayers.Count - j - 1000;
                    starLayers[j].Add(instantiatedParrallaxObj);

                    break;
                }
            }
            Destroy(objsToDestroy[i]);

        }
    }
}
