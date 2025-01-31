using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public static class InputBindingHelper
{
    [Serializable]
    public class InputBindings
    {
        public List<string> Gamepad;
        public List<string> Keyboard;
        public List<string> Mouse;
    }
    
    private static string filePath => Path.Combine(Application.dataPath, "inputPaths.json");
    
    public static List<string> GetAllPossibleBindingPaths()
    {
        if (!File.Exists(filePath) || new FileInfo(filePath).Length == 0)
            DumpDefaultBindings();
        
        string jsonContent = File.ReadAllText(filePath);
        List<string> bindingPaths = new List<string>();
        
        InputBindings loadedBindings = JsonUtility.FromJson<InputBindings>(jsonContent);

        bindingPaths.AddRange(ProcessControls(loadedBindings.Gamepad, "Gamepad"));
        bindingPaths.AddRange(ProcessControls(loadedBindings.Keyboard, "Keyboard"));
        bindingPaths.AddRange(ProcessControls(loadedBindings.Mouse, "Mouse"));

        return bindingPaths;
    }

    private static IEnumerable<string> ProcessControls(List<string> controls, string deviceType)
    {
        List<string> formattedPaths = new List<string>();

        foreach (var control in controls)
        {
            if (!string.IsNullOrEmpty(control))
            {
                string controlName = control.Substring(control.LastIndexOf('/'));
                string formattedPath = $"<{deviceType}>{controlName}";
                formattedPaths.Add(formattedPath);
            }
        }

        return formattedPaths;
    }
    
    public static void DumpDefaultBindings()
    {
        var inputPaths = new JObject();
        var layouts = InputSystem.ListLayouts().Where(layout => InputSystem.LoadLayout(layout).isDeviceLayout);
        
        foreach (var layout in layouts)
        {
            try
            {
                var device = InputSystem.AddDevice(layout);
                
                if (device != null)
                {
                    inputPaths[device.layout] = new JArray();

                    foreach (var control in device.allControls)
                    {
                        JArray arr = (JArray)inputPaths[device.layout];
                        arr.Add(control.path);
                    }

                    InputSystem.RemoveDevice(device);
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning(e);
            }
        }

        File.WriteAllText(filePath, inputPaths.ToString(), Encoding.UTF8);
    }
}
