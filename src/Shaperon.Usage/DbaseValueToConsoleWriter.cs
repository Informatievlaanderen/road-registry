using System;
using Shaperon;

namespace Usage
{
    partial class Program
    {
        private class DbaseValueToConsoleWriter : IDbaseValueInspector
        {
            public void Inspect(DbaseDateTime value)
            {
                if(value.Value.HasValue)
                {
                    Console.Write(value.Value.Value.ToString("yyyyMMddTHHmmss"));
                } else {
                    Console.Write("<null>");
                }
            }

            public void Inspect(DbaseDouble value)
            {
                if(value.Value.HasValue)
                {
                    Console.Write(value.Value.Value.ToString(""));
                } else {
                    Console.Write("<null>");
                }
            }

            public void Inspect(DbaseInt32 value)
            {
                if(value.Value.HasValue)
                {
                    Console.Write(value.Value.Value.ToString(""));
                } else {
                    Console.Write("<null>");
                }
            }

            public void Inspect(DbaseString value)
            {
                if(value.Value != null)
                {
                    Console.Write(value.Value);
                } else {
                    Console.Write("<null>");
                }
            }
        }
    }
}
