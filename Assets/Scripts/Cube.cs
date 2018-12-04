using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cube : MonoBehaviour {


    [SerializeField] Material[] materials;

    [SerializeField] MeshRenderer cubeRenderer;

    [SerializeField] Transform cubeParent;

    string[] plateNames = {"Plate1", "Plate2", "Plate3", "Plate4", "Plate5", "Plate6", "Plate7", "Plate8"};


    float TIME_STAY = 1f;

    bool enter = false;

    float deltaTime = 0f;

    string correctName = "";


    private void Awake()
    {
        int num = Random.Range(0, materials.Length);
        cubeRenderer.material = materials[num];
        correctName = plateNames[num];

    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if (!enter) return;
        deltaTime += Time.deltaTime;
        if(deltaTime> TIME_STAY)
        {
            enter = false;
            deltaTime = 0f;
            GameObject obj = Instantiate(this.gameObject, cubeParent);
            obj.transform.localPosition = Vector3.zero;
            this.gameObject.active = false;
        
        }

	}

    void OnTriggerEnter(Collider collider)
    {

        if (collider.gameObject.name == correctName)
        {
            enter = true;
        }
    }
}
