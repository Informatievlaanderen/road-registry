using System;

namespace Shaperon
{
    public class AnonymousDbaseRecord : DbaseRecord
    {
        public AnonymousDbaseRecord(DbaseField[] fields)
        {
            if (fields == null)
            {
                throw new ArgumentNullException(nameof(fields));
            }

            IsDeleted = false;
            Values = Array.ConvertAll(fields, field => field.CreateFieldValue());
        }
    }
}
