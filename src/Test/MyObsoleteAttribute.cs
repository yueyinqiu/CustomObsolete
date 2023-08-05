using CustomObsolete;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test;

[AttributeUsage(AttributeTargets.Method)]
public sealed class MyObsoleteAttribute : Attribute, ICustomObsoleteAttribute
{
    public string Message => "My!";
    public bool IsError => false;
}
