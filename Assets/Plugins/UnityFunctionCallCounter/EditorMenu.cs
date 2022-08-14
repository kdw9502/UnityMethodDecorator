using UnityEditor;

namespace UnityDecoratorAttribute
{
    public class EditorMenu
    {
        [MenuItem("UnityMethodCallCounter/Inject Dll")]
        public static void Inject()
        {
            ILInjector.InjectAll();        
        }
    }
}
