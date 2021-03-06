﻿using System;
using EPiServer.Shell.Composition;

namespace Knowit.NotFound.Core.Data
{
    public static class DataStoreFactory
    {
        public static EPiServer.Data.Dynamic.DynamicDataStore GetStore(Type t)
        {
            return EPiServer.Data.Dynamic.DynamicDataStoreFactory.Instance.GetOrCreateStore(t);
        }
    }
}
