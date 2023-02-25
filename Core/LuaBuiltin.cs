using System;
using Microsoft.Xna.Framework.Input;
using NLua;
using Teuria;

namespace Pixxa;

public static class LuaBuiltin 
{
    public static void RegisterFunctions(Lua luaState) 
    {
        luaState.RegisterFunction("keyPressed", typeof(LuaBuiltin).GetMethod("KeyPressed"));
        luaState.RegisterFunction("keyJustPressed", typeof(LuaBuiltin).GetMethod("KeyJustPressed"));
    }        

    public static bool KeyPressed(string key) 
    {
        var keys = (Keys)Enum.Parse(typeof(Keys), key);
        if (TInput.Keyboard.Pressed(keys)) 
        {
            return true;
        }
        return false;
    }

    public static bool KeyJustPressed(string key) 
    {
        var keys = (Keys)Enum.Parse(typeof(Keys), key);
        if (TInput.Keyboard.JustPressed(keys)) 
        {
            return true;
        }
        return false;
    }
}