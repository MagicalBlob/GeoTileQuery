using Newtonsoft.Json.Utilities;
using UnityEngine;

/// <summary>
/// https://github.com/jilleJr/Newtonsoft.Json-for-Unity/wiki/Fix-AOT-using-AotHelper
/// </summary>
public class AotTypeEnforcer : MonoBehaviour
{
    public void Awake()
    {
        AotHelper.EnsureList<Position>();
    }
}