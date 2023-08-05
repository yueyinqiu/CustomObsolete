using System;

namespace CustomObsolete
{
    [AttributeUsage(
        AttributeTargets.Class |
        AttributeTargets.Struct | 
        AttributeTargets.Enum |
        AttributeTargets.Constructor | 
        AttributeTargets.Method | 
        AttributeTargets.Property | 
        AttributeTargets.Field |
        AttributeTargets.Event |
        AttributeTargets.Interface | 
        AttributeTargets.Delegate)]
    public class ObsoleteBase : Attribute
    {
        public ObsoleteBase() : this(null, false) { }
        public ObsoleteBase(string message) : this(message, false) { }
        public ObsoleteBase(string message, bool isError)
        {
            this.Message = message;
            this.IsError = isError;
        }
        public string Message { get; }
        public bool IsError { get; }
    }
}
