using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ZAGK13Export.Converters
{
    class FieldConverters
    {
        public ContentReference ConvertMediaItemReference(string mediaUrl)
        {
            Guid mediaGuid = Guid.NewGuid();
            string pattern = @"([a-f0-9]{8}-[a-f0-9]{4}-[a-f0-9]{4}-[a-f0-9]{4}-[a-f0-9]{12})";

            Match match = Regex.Match(mediaUrl, pattern);

            if (match.Success)
            {
                mediaGuid = Guid.Parse(match.Value);
            }

            return new ContentReference
            {
                OldGuid = mediaGuid
            };
        }
    }
}
