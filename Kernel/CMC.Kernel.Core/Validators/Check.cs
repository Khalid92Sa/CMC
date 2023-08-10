using System;
using System.Collections.Generic;
using System.Text;

namespace CMC.Kernel.Core.Validators
{
    public static class Check
    {
        public static void NotNull(object value)
        {
            That<NullReferenceException>(value != null);
        }

        public static void That(bool condition)
        {
            That<Exception>(condition);
        }

        public static void That<TException>(bool condition) where TException : Exception, new()
        {
            if (!condition)
                throw new TException();
        }
    }
}
