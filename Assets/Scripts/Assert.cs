using System;
using System.Diagnostics;

public class Assert
{
    public static T NonNull<T>(
        T val,
        string errorMessage = "Assert.NonNull failed") where T : class
    {
        if(val == null)
            throw new Exception(errorMessage);
        return val;
    }

    public static void Condition(
        bool condition,
        string errorMessage = "Assert.Condition failed")
    {
        if(!condition)
            throw new Exception(errorMessage);
    }
}
