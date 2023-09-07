/*using PostSharp.Aspects;
using PostSharp.Serialization;
using System;
using System.Collections.Generic;
using System.Text;

namespace Backend.Utils
{
    public class ExceptionAspect
    {
        [PSerializable]
        public sealed class HandleExceptionsAttribute : OnExceptionAspect
        {
            public override void OnException(MethodExecutionArgs args)
            {
                RXLogger.Print($" { args.Method.ReflectedType.Name } { args.Method.Name } : { args.Exception }");

                args.FlowBehavior = FlowBehavior.Return;
                args.ReturnValue = -1;
            }
        }
    }
}
*/