//using PostSharp.Extensibility;
//using PostSharp.Patterns.Diagnostics;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
//using static Backend.Utils.ExceptionAspect;

namespace Backend.Utils.Extensions
{
    //[Log(AttributeTargetMemberAttributes = MulticastAttributes.Public)]
    internal static class ClientTaskExtensions
    {
        //[HandleExceptions]
        public static System.Threading.Tasks.Task<T> RunReturnAsync<T>(this GTANetworkMethods.Task task, System.Func<T> func)
        {
            var taskCompletionSource = new System.Threading.Tasks.TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);
            task.Run(() =>
            {
                var result = func();
                taskCompletionSource.SetResult(result);
            });
            return taskCompletionSource.Task;
        }

        //[HandleExceptions]
        public static T RunReturn<T>(this GTANetworkMethods.Task task, System.Func<T> func)
        {
            var taskCompletionSource = new System.Threading.Tasks.TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);
            task.Run(() =>
            {
                var result = func();
                taskCompletionSource.SetResult(result);
            });
            return taskCompletionSource.Task.Result;
        }

        //[HandleExceptions]
        public static System.Threading.Tasks.Task RunAsync(this GTANetworkMethods.Task task, Action func)
        {
            task.Run(() =>
            {
                func();
            });
            return Task.CompletedTask;
        }
    }
}
