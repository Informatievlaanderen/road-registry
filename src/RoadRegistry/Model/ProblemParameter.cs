 namespace RoadRegistry.Model
 {
     using System;

     public class ProblemParameter
     {
         private readonly string _name;
         private readonly string _value;

         public ProblemParameter(string name, string value)
         {
             _name = name ?? throw new ArgumentNullException(nameof(name));
             _value = value ?? throw new ArgumentNullException(nameof(value));
         }

         public Messages.ProblemParameter Translate() =>
             new Messages.ProblemParameter
             {
                 Name = _name, Value = _value
             };
     }
 }
