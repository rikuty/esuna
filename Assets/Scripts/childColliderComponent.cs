using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class childColliderComponent : MonoBehaviour {

    public Nest nest {get; private set;}

    public void Init(Nest nest)
    {
        this.nest = nest;
    }

}
