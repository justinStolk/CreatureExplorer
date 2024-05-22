using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeviceBindingUtils : MonoBehaviour
{
    public static DeviceBindingUtils Instance;

    [SerializeField] private Binding[] actionBindings;
    [SerializeField] private static Binding[] bindings;
    [SerializeField] private BindingText[] bindingTextObjects;
    [SerializeField] private static BindingText[] bindingLabels;
    
    void Awake()
    {
        if (Instance == null)
        {
            bindingLabels = bindingTextObjects;
            bindings = actionBindings;
            Instance = this;
        }
        else
            Destroy(this);
    }

    public static void SwapBindingTexts(string controlScheme)
    {
        switch (controlScheme)
        {
            case "Keyboard":
                foreach(BindingText bindingText in bindingLabels)
                {
                    if (TryGetBinding(bindingText.BindingToLabel, out Binding binding))
                        bindingText.TextObject.text = binding.KeyboardInput + (bindingText.interactionText? " to interact": "");
                }
                break;
            case "Gamepad":
                foreach (BindingText bindingText in bindingLabels)
                {
                    if (TryGetBinding(bindingText.BindingToLabel, out Binding binding))
                        bindingText.TextObject.text = binding.GamepadInput + (bindingText.interactionText ? " to interact" : "");
                }
                break;
            default:
                foreach (BindingText bindingText in bindingLabels)
                {
                    if (TryGetBinding(bindingText.BindingToLabel, out Binding binding))
                        bindingText.TextObject.text = binding.KeyboardInput + (bindingText.interactionText ? " to interact" : "");
                }
                break;
        }
    }

    public static string BindingText(string controlScheme, InputActionName action)
    {            
        if (TryGetBinding(action, out Binding binding))
        {
            return controlScheme switch
            {
                "Keyboard" => binding.KeyboardInput,
                "Gamepad" => binding.GamepadInput,
                _ => binding.KeyboardInput,
            };
        }
        return null;
    }

    private static bool TryGetBinding(InputActionName actionName, out Binding binding)
    {
        binding = null;
        foreach (Binding b in bindings)
        {
            if (b.ActionName == actionName)
            {
                binding = b;
                return true;
            }
        }
        return false;
    }
}

[System.Serializable]
public class Binding
{
    [field: SerializeField] public InputActionName ActionName { get; private set; }
    [field: SerializeField] public string GamepadInput { get; private set; }
    [field: SerializeField] public string KeyboardInput { get; private set; }
}

[System.Serializable]
public class BindingText
{
    [field: SerializeField] public TMPro.TextMeshProUGUI TextObject { get; private set; }
    [field: SerializeField] public bool interactionText { get; private set; }
    [field: SerializeField] public InputActionName BindingToLabel { get; private set; }
}

public enum InputActionName
{
    Camera,
    Scrapbook,
    Continue,
    Interact
}
