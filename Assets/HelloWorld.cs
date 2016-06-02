using UnityEngine;
using ChakraHost.Hosting;
using System;
using System.Runtime.InteropServices;

public class HelloWorld : MonoBehaviour {

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

  private static JavaScriptValue CreateCube(JavaScriptValue callee, Boolean isConstructCall, JavaScriptValue[] arguments, ushort argumentCount, IntPtr callbackData) {
    int x = JSValueToInt(arguments[0]);
    int y = JSValueToInt(arguments[0]);
    int z = JSValueToInt(arguments[0]);

    GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
    cube.transform.position = new Vector3(x, y, z);

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

  // Use this for initialization
  void Start () {

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
          // Load the script
          //

          string script = "scene.createCube(0,0,0); console.log(\'Hello world\');";
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
	
	// Update is called once per frame
	void Update () {
	
	}
}
