using System.Collections.Generic;
using UnityEngine;

public class ParrallaxTest : MonoBehaviour
{
    public GameObject starPrefab;
    public CameraMovement cam;
    public Vector2 camSpace;
    public List<List<GameObject>> starLayers = new List<List<GameObject>>();
    public float layerSpeedMultiplier = 1.0f;
    public Vector2 realCamSpace;
    public System.Random rnd = new System.Random();
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Vector2 camPos = cam.gameObject.transform.position;
        if (cam.cam.orthographic)
        {
            float ySize = cam.cam.orthographicSize * 2f;
            float xSize = ySize * cam.cam.aspect;
            float padding = 10f;
            camSpace = new Vector2(xSize + padding, ySize + padding);
            realCamSpace = new Vector2(xSize, ySize);
        }
        else
        {
            Debug.LogError("Ortho Camera not detected!");
        }
        List<List<GameObject>> layers = new List<List<GameObject>>();
        for (int i = 0; i < 3; i++)
        {
            List<GameObject> layer = new List<GameObject>();
            for (int j = 0; j < 15; j++)
            {
                layer.Add(Instantiate(starPrefab, new Vector2(UnityEngine.Random.Range(Mathf.RoundToInt(camPos.x - (camSpace.x / 2)), Mathf.RoundToInt(camPos.x + (camSpace.x / 2))), UnityEngine.Random.Range(Mathf.RoundToInt(camPos.y - (camSpace.y / 2)), Mathf.RoundToInt(camPos.y + (camSpace.y / 2)))), Quaternion.identity));
                layer[j].transform.parent = cam.gameObject.transform;
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
            float padding = 10f;
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
            float layerSpeed = (layerSpeedMultiplier * (i + 1));
            for (int j = 0; j < starLayers[i].Count; j++)
            {
                starLayers[i][j].transform.Translate(speed * layerSpeed * Time.deltaTime);
                if (!DetectObstaclesInPosition.ContainsPoint(camPos, new Vector2(camSpace.x * 2, camSpace.y * 2), starLayers[i][j].transform.position).any)
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
                    DetectObstaclesInPosition.ContainsPointReturn returnVal = DetectObstaclesInPosition.ContainsPoint(camPos, camSpace, objsToDestroy[i].transform.position);
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
                        if (randomNum == 0)
                        {

                        }
                        spawnPos = new Vector2(UnityEngine.Random.Range(camPos.x + (realCamSpace.x / 2), camPos.x + realCamSpace.x), UnityEngine.Random.Range(camPos.y - camSpace.y, camPos.y - (camSpace.y / 2)));
                    }
                    else if (returnVal.s2 && returnVal.s3)
                    {
                        spawnPos = new Vector2(UnityEngine.Random.Range(camPos.x - camSpace.x, camPos.x - (camSpace.x / 2)), UnityEngine.Random.Range(camPos.y - camSpace.y, camPos.y - (camSpace.y / 2)));
                    }
                    else if (returnVal.s3 && returnVal.s4)
                    {
                        spawnPos = new Vector2(UnityEngine.Random.Range(camPos.x - camSpace.x, camPos.x - (camSpace.x / 2)), UnityEngine.Random.Range(camPos.y + (camSpace.y / 2), camPos.y + camSpace.y));
                    }
                    else if (returnVal.s4 && returnVal.s1)
                    {
                        spawnPos = new Vector2(UnityEngine.Random.Range(camPos.x + (camSpace.x / 2), camPos.x + camSpace.x), UnityEngine.Random.Range(camPos.y + (camSpace.y / 2), camPos.y + camSpace.y));
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
                            spawnPos.x = UnityEngine.Random.Range(camPos.x - (camSpace.x / 2), camPos.x + (camSpace.x / 2));
                        }
                        if (returnVal.s2)
                        {
                            spawnPos.y = camPos.y + (camSpace.y / 2);
                        }
                        else if (returnVal.s4)
                        {
                            spawnPos.y = camPos.y - (camSpace.y / 2);
                        }
                        else
                        {
                            spawnPos.y = UnityEngine.Random.Range(camPos.y - (camSpace.y / 2), camPos.y + (camSpace.y / 2));
                        }
                    }
                    GameObject instantiatedParrallaxObj = Instantiate(starPrefab, spawnPos, Quaternion.identity);
                    instantiatedParrallaxObj.transform.parent = cam.gameObject.transform;
                    starLayers[j].Add(instantiatedParrallaxObj);

                    break;
                }
            }
            Destroy(objsToDestroy[i]);

        }
    }
}
