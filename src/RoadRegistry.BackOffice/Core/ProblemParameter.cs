 namespace RoadRegistry.BackOffice.Core
 {
     using System;

     public class ProblemParameter
     {
         public ProblemParameter(string name, string value)
         {
             Name = name ?? throw new ArgumentNullException(nameof(name));
             Value = value ?? throw new ArgumentNullException(nameof(value));
         }

         public string Name { get; }

         public string Value { get; }

         public bool Equals(ProblemParameter other) => other != null
                                                       && string.Equals(Name, other.Name)
                                                       && string.Equals(Value, other.Value);

         public override bool Equals(object obj) => obj is ProblemParameter other && Equals(other);
         public override int GetHashCode() => Name.GetHashCode() ^ Value.GetHashCode();

         public Messages.ProblemParameter Translate() =>
             new Messages.ProblemParameter
             {
                 Name = Name, Value = Value
             };
     }
 }
