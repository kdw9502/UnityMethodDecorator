using UnityEditor;

namespace FunctionCallCount
{
    public class EditorMenu
    {
        [MenuItem("UnityFunctionCallCounter/Inject Dll")]
        public static void Inject()
        {
            ILInjector.InjectAll();        
        }
    }
}
