using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Windows.Kinect;
using Joint = Windows.Kinect.Joint;

public class BodyVfxConnect : MonoBehaviour
{
    private BodySourceManager bodySourceManager;
    public GameObject bodyPrefab;
    public GameObject jointPrefab;
    public GameObject vfxControllerPrefab;
    private Dictionary<ulong, GameObject> bodies = new();
    private List<JointType> joints = new()
    {
        JointType.HandLeft,
        JointType.HandRight,
    };

    public bool drawSkeleton;
    public bool customColors;
    private Color lastColor;

    private Dictionary<JointType, JointType> boneMap = new()
    {
        // { JointType.FootLeft, JointType.AnkleLeft },
        // { JointType.AnkleLeft, JointType.KneeLeft },
        // { JointType.KneeLeft, JointType.HipLeft },
        // { JointType.HipLeft, JointType.SpineBase },
        
        // { JointType.FootRight, JointType.AnkleRight },
        // { JointType.AnkleRight, JointType.KneeRight },
        // { JointType.KneeRight, JointType.HipRight },
        // { JointType.HipRight, JointType.SpineBase },
        
        // { JointType.HandTipLeft, JointType.HandLeft },
        // { JointType.ThumbLeft, JointType.HandLeft },
        { JointType.HandLeft, JointType.WristLeft },
        { JointType.WristLeft, JointType.ElbowLeft },
        { JointType.ElbowLeft, JointType.ShoulderLeft },
        { JointType.ShoulderLeft, JointType.SpineShoulder },
        
        // { JointType.HandTipRight, JointType.HandRight },
        // { JointType.ThumbRight, JointType.HandRight },
        { JointType.HandRight, JointType.WristRight },
        { JointType.WristRight, JointType.ElbowRight },
        { JointType.ElbowRight, JointType.ShoulderRight },
        { JointType.ShoulderRight, JointType.SpineShoulder },
        
        // { JointType.SpineBase, JointType.SpineMid },
        // { JointType.SpineMid, JointType.SpineShoulder },
        // { JointType.SpineShoulder, JointType.Neck },
        // { JointType.Neck, JointType.Head },
    };

    public Material BoneMaterial;

    public Color[] colors = new Color[]
    {
        Color.blue,
        Color.cyan,
        Color.green,
        Color.magenta,
        Color.red,
        Color.white,
        Color.yellow
    };

    private Dictionary<ulong, Color> bodyColors = new Dictionary<ulong, Color>();

    // private InstructionsController instructionsController;
    // float instructionsClosedAt;
    // float dontCloseSignal;
    // bool coroutineTriggered;
    // public float instructionsDelay = 12f;

    void Start()
    {
        Cursor.visible = false;
        bodySourceManager = GameObject.Find("BodyManager").GetComponent<BodySourceManager>();
        // instructionsController = GameObject.FindWithTag("Canvas").GetComponent<InstructionsController>();
    }

    void Update()
    {
        #region Get Kinect Data
        Body[] bodyData = bodySourceManager.GetData();
        if (bodyData == null)
            return;
        
        List<ulong> trackedIds = new List<ulong>();
        foreach(var body in bodyData)
        {
            if (body == null)
                continue;
                
            if(body.IsTracked)
                trackedIds.Add (body.TrackingId);
        }
        #endregion

        #region Delete Kinect Bodies
        List<ulong> knownIds = new List<ulong>(bodies.Keys);
        foreach(ulong trackingId in knownIds)
        {
            if(!trackedIds.Contains(trackingId))
            {
                Destroy(bodies[trackingId]);
                bodies.Remove(trackingId);
            }
        }

        if (Input.GetMouseButtonDown(0))
            DeleteAllBodies(knownIds);
        else if (Input.GetMouseButtonDown(1))
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        #endregion
            
        // if (Time.fixedTime > dontCloseSignal + instructionsDelay)
        // {
        //     instructionsController.SetVisibility(true);
        // }

        #region Create Kinect Bodies
        foreach(var body in bodyData)
        {
            if (body == null)
                continue;
            
            if(body.IsTracked)
            {
                // dontCloseSignal = Time.fixedTime;
                if(!bodies.ContainsKey(body.TrackingId)) {                    
                    bodies[body.TrackingId] = CreateBodyObject(body.TrackingId);
                    
                    GameObject bodyVfx = Instantiate(vfxControllerPrefab);
                    bodyVfx.name = "VFX Controller:" + body.TrackingId;
                    bodyVfx.transform.parent = bodies[body.TrackingId].transform;
                }
                
                UpdateBodyObject(body, bodies[body.TrackingId]);

                if (drawSkeleton) 
                    UpdateSkeleton(body, bodies[body.TrackingId]);
            }
        }
        #endregion
    }

    private GameObject CreateBodyObject(ulong id)
    {
        GameObject body = Instantiate(bodyPrefab);
        body.name = "Body:" + id;

        for (JointType joint = JointType.SpineBase; joint <= JointType.ThumbRight; joint++)
        {
            GameObject newJoint;
            if (drawSkeleton) 
            {
                // newJoint = GameObject.CreatePrimitive(PrimitiveType.Cube);
                // newJoint.GetComponent<MeshRenderer>().material = BoneMaterial;
                newJoint = Instantiate(jointPrefab);
            
                LineRenderer lr = newJoint.AddComponent<LineRenderer>();
                lr.positionCount = 2;
                lr.material = BoneMaterial;
                lr.startWidth = 0.1f;
                lr.endWidth = 0.1f;
            }
            else
            {
                newJoint = Instantiate(jointPrefab);
            }

            if (joint == JointType.HandLeft) {
                newJoint.tag = "LeftHand";
            }
            else if (joint == JointType.HandRight) {
                newJoint.tag = "RightHand";
            }

            newJoint.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
            newJoint.name = joint.ToString();
            newJoint.transform.parent = body.transform;
        }

        Color bodyColor = colors[Random.Range(0, colors.Length)];
        while (bodyColor == lastColor)
        {
            bodyColor = colors[Random.Range(0, colors.Length)];
        }
        lastColor = bodyColor;
        bodyColors[id] = bodyColor;
        
        return body;
    }

    private void UpdateBodyObject(Body body, GameObject bodyObject)
    {
        if (!body.IsRestricted) {
            foreach (JointType joint in joints) {
                Joint sourceJoint = body.Joints[joint];
                Vector3 targetPosition = GetVector3FromJoint(sourceJoint);

                Transform jointObject = bodyObject.transform.Find(joint.ToString());
                jointObject.position = targetPosition;
            }
            // if (body.HandLeftConfidence > 0 && body.HandRightConfidence > 0){
                if ((body.HandLeftState == HandState.Open || body.HandLeftState == HandState.Lasso || body.HandLeftState == HandState.Unknown) && (body.HandRightState == HandState.Open || body.HandRightState == HandState.Lasso || body.HandRightState == HandState.Unknown))
                    bodyObject.tag = "BothState";
                else if ((body.HandLeftState == HandState.Open || body.HandLeftState == HandState.Lasso || body.HandLeftState == HandState.Unknown) && body.HandRightState != HandState.Open && body.HandRightState != HandState.Lasso && body.HandRightState != HandState.Unknown)
                    bodyObject.tag = "LeftState";
                else if (body.HandLeftState != HandState.Open && body.HandLeftState != HandState.Lasso && body.HandLeftState != HandState.Unknown && (body.HandRightState == HandState.Open || body.HandRightState == HandState.Lasso || body.HandRightState == HandState.Unknown))
                    bodyObject.tag = "RightState";
                else
                    bodyObject.tag = "NoneState";
            // }
        }
        // else {
        //     Destroy(bodies[body.TrackingId]);
        //     bodies.Remove(body.TrackingId);
        // }
    }

    private static Vector3 GetVector3FromJoint(Joint joint)
    {
        return new Vector3(joint.Position.X * 10, joint.Position.Y * 10, joint.Position.Z * 10);
    }

    private void DeleteAllBodies(List<ulong> ids)
    {
        foreach(ulong trackingId in ids)
        {
            Destroy(bodies[trackingId]);
            bodies.Remove(trackingId);
        }
    }

    private void UpdateSkeleton(Body body, GameObject bodyObject)
    {
        for (JointType joint = JointType.SpineBase; joint <= JointType.ThumbRight; joint++)
        {
            Joint sourceJoint = body.Joints[joint];
            Joint? targetJoint = null;
            
            if(boneMap.ContainsKey(joint))
            {
                targetJoint = body.Joints[boneMap[joint]];
            }
            
            Transform jointObj = bodyObject.transform.Find(joint.ToString());
            jointObj.localPosition = GetVector3FromJoint(sourceJoint);
            
            LineRenderer lr = jointObj.GetComponent<LineRenderer>();
            if(targetJoint.HasValue)
            {
                lr.SetPosition(0, jointObj.localPosition);
                lr.SetPosition(1, GetVector3FromJoint(targetJoint.Value));
                if (customColors)
                {
                    lr.startColor = bodyColors[body.TrackingId];
                    lr.endColor = bodyColors[body.TrackingId];
                }
                else
                {
                    lr.startColor = ColorSkeleton (sourceJoint.TrackingState);
                    lr.endColor = ColorSkeleton (targetJoint.Value.TrackingState);
                }
                
            }
            else
            {
                lr.enabled = false;
            }
        }
    }

    private static Color ColorSkeleton(TrackingState state)
    {
        switch (state)
        {
        case TrackingState.Tracked:
            return Color.white;

        case TrackingState.Inferred:
            return Color.grey;

        default:
            return Color.black;
        }
    }
}
