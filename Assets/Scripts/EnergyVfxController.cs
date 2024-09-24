using UnityEngine;
using MyLib = JossLib.MyUtilityFuncs;

public class EnergyVfxController : MonoBehaviour
{
    private GameObject bodyObj;
    private Transform leftParent;
    private Transform rightParent;
    private Transform wayOut;
    public GameObject leftEmitterObj;
    public GameObject rightEmitterObj;
    public GameObject emitterTargetObj;
    private GameObject midPointObj;
    void Start()
    {
        bodyObj = gameObject.transform.parent.gameObject;
        leftParent = MyLib.FindChildWithTag(bodyObj, "LeftHand").transform;
        rightParent = MyLib.FindChildWithTag(bodyObj, "RightHand").transform;

        wayOut = GameObject.FindWithTag("OutHere").transform;
        if (!wayOut)
            Debug.LogError("No object found with tag 'OutHere'");

        midPointObj = new GameObject("Emitters Midpoint");
        midPointObj.AddComponent<Rigidbody>();
        midPointObj.GetComponent<Rigidbody>().useGravity = false;
        midPointObj.GetComponent<Rigidbody>().collisionDetectionMode = CollisionDetectionMode.Continuous;
        midPointObj.transform.parent = gameObject.transform;
    }


    [SerializeField]
    private float G = 1f;
    private float r;
    private float m1;
    private float m2;

    public float gravityStopDist = 1f;
    public float maxDist = 100f;
    public float massMult = 4000f;
    public float minMass = 0.25f;
    public float pushStopDist = 0.1f;
    void FixedUpdate()
    {
        SetControllerParents();

        midPointObj.transform.position = Vector3.Lerp(leftEmitterObj.transform.position, rightEmitterObj.transform.position, 0.5f);

        r = Vector3.Distance(midPointObj.transform.position, emitterTargetObj.transform.position);
        float adaptMass = Mathf.InverseLerp(1f, maxDist, r) * massMult;
        if (adaptMass < minMass) adaptMass = minMass;
        midPointObj.GetComponent<Rigidbody>().mass = adaptMass;

        m1 = emitterTargetObj.GetComponent<Rigidbody>().mass;
        m2 = midPointObj.GetComponent<Rigidbody>().mass;
        
        if (r > gravityStopDist) 
            emitterTargetObj.GetComponent<Rigidbody>().AddForce(
                (midPointObj.transform.position-emitterTargetObj.transform.position).normalized * 
                (G * (m1 * m2) / (r * r))
            );

        if (r > pushStopDist) 
            PushToMiddle();

        if((emitterTargetObj.transform.position-midPointObj.transform.position).magnitude<gravityStopDist && emitterTargetObj.GetComponent<Rigidbody>().velocity.magnitude<(gravityStopDist / 2f)){
            emitterTargetObj.GetComponent<Rigidbody>().velocity = Vector3.zero;
        }

        SetScale();
    }

    void SetControllerParents()
    {
        if (bodyObj.tag == "BothState")
        {
            leftEmitterObj.transform.parent = leftParent;
            rightEmitterObj.transform.parent = rightParent;
        }
        else if (bodyObj.tag == "LeftState")
        {
            leftEmitterObj.transform.parent = leftParent;
            rightEmitterObj.transform.parent = leftParent;

        }
        else if (bodyObj.tag == "RightState")
        {
            leftEmitterObj.transform.parent = rightParent;
            rightEmitterObj.transform.parent = rightParent;
        }
        else if (bodyObj.tag == "NoneState")
        {
            leftEmitterObj.transform.parent = wayOut;
            rightEmitterObj.transform.parent = wayOut;
        }
        else
        {
            Debug.LogError("Shouldn't be here! Body object tag not found.");
        }
        leftEmitterObj.transform.localPosition = Vector3.zero;
        rightEmitterObj.transform.localPosition = Vector3.zero;
    }


    public float pushSpeed = 8f;
    public AnimationCurve moveTowardsCurve;
    void PushToMiddle() {
        float loc = Vector3.Distance(midPointObj.transform.position, emitterTargetObj.transform.position);
        float dist = Vector3.Distance(leftEmitterObj.transform.position, rightEmitterObj.transform.position);
        dist = dist / 2f;
        
        float relativeDistance = Mathf.InverseLerp(0, dist, loc);
        relativeDistance = moveTowardsCurve.Evaluate(relativeDistance);

        float step = relativeDistance * Time.deltaTime * pushSpeed;
        
        emitterTargetObj.GetComponent<Rigidbody>().position = Vector3.MoveTowards(emitterTargetObj.transform.position, midPointObj.transform.position, step);
    }

    public float approxMaxVelocity = 50f;
    public float approxMaxSeparation = 20f;
    public float oscMagnitude = 0.15f;
    public float oscSpeed = 1f;
    public float overshootMagnitude = 0.5f;
    public float overshootSpeed = 0.33f;
    public AnimationCurve scaleCurve;
    public float minScale = 1f;
    public float maxScale = 3f;
    void SetScale()
    {
        float D = Vector3.Distance(leftEmitterObj.transform.position, rightEmitterObj.transform.position);
        float E = Mathf.InverseLerp(0f, approxMaxSeparation, D);
        float F = Mathf.Lerp(1f, maxScale, E);
        
        float V = Mathf.InverseLerp(0, approxMaxVelocity, emitterTargetObj.GetComponent<Rigidbody>().velocity.sqrMagnitude);
        float W = scaleCurve.Evaluate(V) * overshootSpeed;

        float Z = (0.5f * Mathf.Sin(Time.fixedUnscaledTime * oscSpeed + W * overshootMagnitude) + 0.5f) * F + minScale;
    
        emitterTargetObj.transform.localScale = Vector3.one * Z;
    }

    void ObjectFollowCursor() 
    {
        Vector3 point1 = new Vector3(leftParent.position.x,leftParent.position.y,0);
        Vector3 point2 = new Vector3(leftParent.position.x,leftParent.position.y,0);
    }

    
}
