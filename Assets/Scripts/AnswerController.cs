using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class AnswerController : MonoBehaviour {

    [SerializeField] Material startMaterial;

    [SerializeField] Cube startCube;

    [SerializeField] Plate startPlate;

    [SerializeField] Material[] materials;

    [SerializeField] Plate[] plates;

    [SerializeField] Transform cubeParent;

    [SerializeField] GameObject objOriginal;

    [SerializeField]
    Cube resultCube;

    [SerializeField]
    Plate resultPlate;

    int playCubeCount = 0;


    Action<DEFINE_APP.ANSWER_TYPE_ENUM> callback;
    

    public void Init(Action<DEFINE_APP.ANSWER_TYPE_ENUM> callback)
    {
        this.callback = callback;

        startCube.Init(CallbackFromCube, DEFINE_APP.ANSWER_TYPE_ENUM.START, -1, startMaterial);
        startPlate.Init(DEFINE_APP.ANSWER_TYPE_ENUM.START, -1, startMaterial);
        for (int i=0; i<plates.Length; i++)
        {
            plates[i].Init(DEFINE_APP.ANSWER_TYPE_ENUM.PLAY, i, materials[i]);
        }

        resultCube.Init(CallbackFromCube, DEFINE_APP.ANSWER_TYPE_ENUM.RESULT, -1, startMaterial);
        resultPlate.Init(DEFINE_APP.ANSWER_TYPE_ENUM.RESULT, -1, startMaterial);
    }


    void CallbackFromCube(Cube cube)
    {
        callback(cube.answerType);

        switch (cube.answerType) {
            case DEFINE_APP.ANSWER_TYPE_ENUM.PLAY:
                InstantiateNewCube();
                break;
            case DEFINE_APP.ANSWER_TYPE_ENUM.START:
                break;
            case DEFINE_APP.ANSWER_TYPE_ENUM.RESULT:
                break;
        }
        
    }


    public void InstantiateNewCube()
    {
        GameObject obj = Instantiate(objOriginal, cubeParent);
        obj.transform.localPosition = Vector3.zero;
        obj.SetActive(true);
        Cube cube = obj.GetComponent<Cube>();
        int num;
        Debug.Log(playCubeCount.ToString());
        if (playCubeCount < 8)
        {
            num = playCubeCount;
        }else
        {
            num =  UnityEngine.Random.Range(0, materials.Length);

        }
        cube.Init(CallbackFromCube, DEFINE_APP.ANSWER_TYPE_ENUM.PLAY, num, materials[num]);
        playCubeCount++;
    }
    
}
