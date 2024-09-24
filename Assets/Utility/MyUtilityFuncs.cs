using UnityEngine;
namespace JossLib {
    public class MyUtilityFuncs
    {
        public static GameObject FindChildWithTag(GameObject parent, string tag) {
        GameObject child = null;

        foreach(Transform transform in parent.transform) {
            if(transform.CompareTag(tag)) {
                child = transform.gameObject;
                break;
            }
        }

        return child;
        }
    }
}
