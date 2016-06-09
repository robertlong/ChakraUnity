using UnityEngine;
using ChakraHost.Hosting;
using System;
using System.Runtime.InteropServices;
using System.Collections;

public class JSRunner : MonoBehaviour {

  public string url = "http://localhost:3000/bundle.js";
  
  private static JavaScriptSourceContext currentSourceContext = JavaScriptSourceContext.FromIntPtr(IntPtr.Zero);
  private static readonly JavaScriptNativeFunction consoleLogDelegate = ConsoleLog;
  private static readonly JavaScriptNativeFunction createCubeDelegate = CreateCube;

  private static void PrintScriptException(JavaScriptValue exception) {
    JavaScriptPropertyId messageName = JavaScriptPropertyId.FromString("message");
    JavaScriptValue messageValue = exception.GetProperty(messageName);
    string message = messageValue.ToString();

    Debug.LogErrorFormat("chakrahost: exception: {0}", message);
  }

  private static void SetCallback(JavaScriptValue obj, string propertyName, JavaScriptNativeFunction callback, IntPtr callbackState) {
    JavaScriptPropertyId propertyId = JavaScriptPropertyId.FromString(propertyName);
    JavaScriptValue function = JavaScriptValue.CreateFunction(callback, callbackState);

    obj.SetProperty(propertyId, function, true);
  }

  private static void SetProperty(JavaScriptValue obj, string propertyName, JavaScriptValue value) {
    JavaScriptPropertyId propertyId = JavaScriptPropertyId.FromString(propertyName);
    obj.SetProperty(propertyId, value, true);
  }

  private static string JSValueToString(JavaScriptValue value) {
    JavaScriptValue strVal;
    Native.JsConvertValueToString(value, out strVal);
    IntPtr strPtr;
    UIntPtr stringLength;
    Native.JsStringToPointer(strVal, out strPtr, out stringLength);
    return Marshal.PtrToStringUni(strPtr);
  }

  private static int JSValueToInt(JavaScriptValue value) {
    JavaScriptValue intVal;
    Native.JsConvertValueToNumber(value, out intVal);
    int intResult;
    Native.JsNumberToInt(intVal, out intResult);
    return intResult;
  }

  private static float JSValueToFloat(JavaScriptValue value) {
    JavaScriptValue floatVal;
    Native.JsConvertValueToNumber(value, out floatVal);
    double doubleResult;
    Native.JsNumberToDouble(floatVal, out doubleResult);
    return (float) doubleResult;
  }

  private static JavaScriptValue CreateCube(JavaScriptValue callee, Boolean isConstructCall, JavaScriptValue[] arguments, ushort argumentCount, IntPtr callbackData) {
    int x = JSValueToInt(arguments[1]);
    int y = JSValueToInt(arguments[2]);
    int z = JSValueToInt(arguments[3]);
    float r = JSValueToFloat(arguments[4]);
    float g = JSValueToFloat(arguments[5]);
    float b = JSValueToFloat(arguments[6]);
    float a = JSValueToFloat(arguments[7]);

    GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
    cube.transform.position = new Vector3(x, y, z);
    cube.GetComponent<Renderer>().material.color = new Color(r / 255, g / 255, b / 255, a / 255);

    return JavaScriptValue.Invalid;
  }

  private static JavaScriptValue ConsoleLog(JavaScriptValue callee, Boolean isConstructCall, JavaScriptValue[] arguments, ushort argumentCount, IntPtr callbackData) {
    string output = "";

    foreach (var argument in arguments) {
      output += JSValueToString(argument);
    }

    Debug.Log(output);

    return JavaScriptValue.Invalid;
  }

  private static JavaScriptContext CreateContext(JavaScriptRuntime runtime) {
    JavaScriptContext context = runtime.CreateContext();

    using (new JavaScriptContext.Scope(context)) {

      // Create the global javascript object
      JavaScriptValue globalObject = JavaScriptValue.GlobalObject;

      // Add the console object
      JavaScriptValue console;
      Native.JsCreateObject(out console);

      // Add the console.log function
      SetProperty(globalObject, "console", console);
      SetCallback(console, "log", consoleLogDelegate, IntPtr.Zero);

      // Add the scene object
      JavaScriptValue scene;
      Native.JsCreateObject(out scene);

      // Add the console.log function
      SetProperty(globalObject, "scene", scene);
      SetCallback(scene, "createCube", createCubeDelegate, IntPtr.Zero);
    }

    return context;
  }

  void LoadScript(string script) {

    try {
      //
      // Create the runtime. We're only going to use one runtime for this host.
      //

      using (JavaScriptRuntime runtime = JavaScriptRuntime.Create()) {
        //
        // Similarly, create a single execution context. Note that we're putting it on the stack here,
        // so it will stay alive through the entire run.
        //

        JavaScriptContext context = CreateContext(runtime);

        // Int
        // Native.JsSetRuntimeMemoryAllocationCallback(runtime, , )

        //
        // Now set the execution context as being the current one on this thread.
        //

        using (new JavaScriptContext.Scope(context)) {
          //
          // Run the script.
          //

          JavaScriptValue result;
          try {
            result = JavaScriptContext.RunScript(script, currentSourceContext++, "");
          } catch (JavaScriptScriptException e) {
            PrintScriptException(e.Error);
            return;
          } catch (Exception e) {
            Console.Error.WriteLine("chakrahost: failed to run script: {0}", e.Message);
            return;
          }
        }
      }
    } catch (Exception e) {
      Debug.LogErrorFormat("chakrahost: fatal error: internal error: {0}.", e.Message);
    }
  }

  // Use this for initialization
  IEnumerator Start () {
    WWW www = new WWW(url);
    yield return www;
    LoadScript(www.text);
  }
	
	// Update is called once per frame
	void Update () {
	
	}
}
