using UnityEngine;

public class GetParents : MonoBehaviour
{
    void Start()
    {
        Transform t = this.transform;
        while (t != null)
        {
            Logger.Log($"GetParents > {t.name} | {t.gameObject.activeInHierarchy} | {t.gameObject.activeSelf} | {t.gameObject.transform.position} | {t.gameObject.transform.localPosition} | {t.gameObject.transform.localScale}");
            t = t.parent;
        }
    }
}
