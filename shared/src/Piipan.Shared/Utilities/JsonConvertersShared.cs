using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Text;

namespace Piipan.Shared.Utilities
{
    public class JsonConvertersShared
    {
        public class DateTimeConverter : IsoDateTimeConverter
        {
            public DateTimeConverter()
            {
                base.DateTimeFormat = "yyyy-MM-dd";
            }
        }
    }
}
