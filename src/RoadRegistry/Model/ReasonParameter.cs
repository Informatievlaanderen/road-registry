 namespace RoadRegistry.Model
 {
     using System;

     public class ReasonParameter
     {
         private readonly string _name;
         private readonly string _value;

         public ReasonParameter(string name, string value)
         {
             _name = name ?? throw new ArgumentNullException(nameof(name));
             _value = value ?? throw new ArgumentNullException(nameof(value));
         }

         public Messages.ReasonParameter Translate() =>
             new Messages.ReasonParameter
             {
                 Name = _name, Value = _value
             };
     }
 }
