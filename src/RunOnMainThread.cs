// Copyright 2005 Sameer Morar <smorar@gmail.com>, Carl Hultquist <chultquist@gmail.com>
// This code is licensed under the GPLv2 license. Please see the COPYING file
// for more information

namespace bibliographer
{
public class RunOnMainThread {
   private object methodClass;
   private string methodName;
   private object[] arguments;

   public static void Run(object methodClass, string methodName, object[] arguments) {
       new RunOnMainThread(methodClass, methodName, arguments);
   }

   public RunOnMainThread(object methodClass, string methodName, object[] arguments) {
       this.methodClass = methodClass;
       this.methodName = methodName;
       this.arguments = arguments;
       GLib.Idle.Add(new GLib.IdleHandler(Go));
   }
   private bool Go() {
       methodClass.GetType().InvokeMember(methodName, System.Reflection.BindingFlags.Default | System.Reflection.BindingFlags.Public |
                                          System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.InvokeMethod, null,methodClass, arguments);
       return false;
   }
}
}