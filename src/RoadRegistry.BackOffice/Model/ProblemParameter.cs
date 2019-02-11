 namespace RoadRegistry.BackOffice.Model
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

         public bool Equals(ProblemParameter other) => other != null
                                                       && string.Equals(_name, other._name)
                                                       && string.Equals(_value, other._value);

         public override bool Equals(object obj) => obj is ProblemParameter other && Equals(other);
         public override int GetHashCode() => _name.GetHashCode() ^ _value.GetHashCode();

         public Messages.ProblemParameter Translate() =>
             new Messages.ProblemParameter
             {
                 Name = _name, Value = _value
             };
     }
 }
