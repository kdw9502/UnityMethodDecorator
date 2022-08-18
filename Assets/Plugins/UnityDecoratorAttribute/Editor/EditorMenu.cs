using UnityEditor;
#if UNITY_EDITOR
namespace UnityDecoratorAttribute
{
    public class EditorMenu
    {
        [MenuItem("UnityMethodCallCounter/Inject Dll")]
        public static void Inject()
        {
            ILInjector.ForceInjectAll();        
        }
    }
}

#endif