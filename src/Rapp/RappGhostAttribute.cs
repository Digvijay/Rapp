using System;

namespace Rapp
{
    /// <summary>
    /// Opt-in attribute to generate a Zero-Copy "Ghost Reader" (ref struct) for this class.
    /// The generated reader allows accessing properties directly from the raw binary buffer without object allocation.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class RappGhostAttribute : Attribute 
    { 
    }
}
