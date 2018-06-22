using System;

namespace Shaperon
{
    public class AnonymousDbaseSchema : DbaseSchema
    {
        public AnonymousDbaseSchema(DbaseField[] fields)
        {
            if (fields == null)
            {
                throw new ArgumentNullException(nameof(fields));
            }

            Fields = fields;
        }
    }
}
