using CMS.DocumentEngine;
using CMS.Helpers;
using Common;
using DocumentFormat.OpenXml.Presentation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZAGK13Export.Converters;

namespace ZAGK13Export.Services
{
    class SectionService
    {
        private readonly Dictionary<string, ISectionConverter> _sectionConverters;

        public SectionService(IEnumerable<ISectionConverter> converters)
        {
            _sectionConverters = converters.ToDictionary(c => c.Type, c => c);
        }

        public Task<Section> ConvertSection(string type, TreeNode page)
        {
            if (_sectionConverters.TryGetValue(type, out var converter))
            {
                return Task.FromResult(converter.Convert(page));
            }

            throw new ArgumentException($"No section converter found for type: {type}");
        }
    }
}
