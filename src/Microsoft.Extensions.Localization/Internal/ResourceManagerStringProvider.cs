// Copyright (c) .NET Foundation. All rights reserved. 
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information. 

using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Resources;

namespace Microsoft.Extensions.Localization.Internal
{
    public class ResourceManagerStringProvider : IResourceStringProvider
    {
        private readonly IResourceNamesCache _resourceNamesCache;
        private readonly ResourceManager _resourceManager;
        private readonly Assembly _assembly;
        private readonly string _resourceBaseName;

        public ResourceManagerStringProvider(
            IResourceNamesCache resourceCache,
            ResourceManager resourceManager,
            Assembly assembly,
            string baseName)
        {
            _resourceManager = resourceManager;
            _resourceNamesCache = resourceCache;
            _assembly = assembly;
            _resourceBaseName = baseName;
        }

        private string GetResourceCacheKey(CultureInfo culture)
        {
            var resourceName = _resourceManager.BaseName;

            return $"Culture={culture.Name};resourceName={resourceName};Assembly={_assembly.FullName}";
        }

        private string GetResourceName(CultureInfo culture)
        {
            var resourceStreamName = _resourceBaseName;
            if (!string.IsNullOrEmpty(culture.Name))
            {
                resourceStreamName += "." + culture.Name;
            }
            resourceStreamName += ".resources";

            return resourceStreamName;
        }

        private IList<string> ThrowOrNull(CultureInfo culture, bool throwOnMissing)
        {
            if (throwOnMissing)
            {
                throw new MissingManifestResourceException(
                    Resources.FormatLocalization_MissingManifest(GetResourceName(culture)));
            }

            return null;
        }

        public IList<string> GetAllResourceStrings(CultureInfo culture, bool throwOnMissing)
        {
            var cacheKey = GetResourceCacheKey(culture);

            return _resourceNamesCache.GetOrAdd(cacheKey, _ =>
            {
                var resourceSet = _resourceManager.GetResourceSet(culture, true, false);

                if(resourceSet == null)
                {
                    return ThrowOrNull(culture, throwOnMissing);
                }


                var rs = resourceSet.GetEnumerator();

                var names = new List<string>();
                foreach(DictionaryEntry i in resourceSet)
                {
                    names.Add(i.Key.ToString());
                }

                return names;
            });
        }
    }
}
