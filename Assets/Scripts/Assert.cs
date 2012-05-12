using System;
using System.Diagnostics;

public class Assert
{
    public static T NonNull<T>(T val) where T : class
    {
        return NonNull(val, "Assert.NonNull failed");
    }
    public static T NonNull<T>(T val, string errorMessage) where T : class
    {
        if(val == null)
            throw new Exception(errorMessage);
        return val;
    }

    public static void Condition(bool condition)
    {
        Condition(condition, "Assert.Condition failed");
    }
    public static void Condition(bool condition, string errorMessage)
    {
        if(!condition)
            throw new Exception(errorMessage);
    }
}
