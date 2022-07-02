using System.Collections.Generic;

public class CallCounter
{
    private readonly Dictionary<string, int> callCountDict = new();

    public int GetFunctionCallCount(string functionName)
    {
        return callCountDict[functionName];
    }

    public void IncreaseFunctionCallCount(string functionName)
    {
        if (!callCountDict.ContainsKey(functionName))
        {
            callCountDict[functionName] = 0;
        }

        callCountDict[functionName]++;
    }
    
}