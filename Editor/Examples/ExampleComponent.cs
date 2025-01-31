#if UNITY_EDITOR
using StudioFortithri.AttributesAttachInspector;
using UnityEngine;

[CustomInspector]
public class ExampleComponent : MonoBehaviour
{
    public float value = 1;

    [ButtonLayout("Example Button")]
    public void Method() => value += 0.1f;
    [ButtonLayout(nameof(PrintValue))]
    public void PrintValue() => Debug.Log($"Value: {value}");
}
#endif
