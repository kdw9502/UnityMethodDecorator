using UnityEditor;

public class EditorMenu
{
    [MenuItem("UnityFunctionCallCounter/Inject All Dll")]
    public static void Inject()
    {
        ILInjector.InjectAll();        
    }
    [MenuItem("UnityFunctionCallCounter/Inject Selected Dll")]
    public static void InjectDll()
    {
        ILInjector.Inject();        
    }
    
}