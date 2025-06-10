// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2025 MindPort GmbH

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using VRBuilder.Core;

namespace VRBuilder.Core.Editor.Tabs
{
    /// <summary>
    /// Tabs group which stores the selected tab in the parent object metadata.
    /// </summary>
    [DataContract]
    internal class TabsGroup : ITabsGroup
    {
        public const string SelectedKey = "selected";

        Metadata ParentMetadata { get; }

        /// <inheritdoc />
        public int Selected
        {
            get
            {
                var value = ParentMetadata.GetMetadata(GetType())[SelectedKey];
                return Convert.ToInt32(value);
            }

            set
            {
                ParentMetadata.SetMetadata(GetType(), SelectedKey, value);
            }
        }

        /// <inheritdoc />
        public IList<ITab> Tabs { get; }

        public TabsGroup(Metadata parentMetadata, params ITab[] tabs)
        {
            ParentMetadata = parentMetadata;
            if (ParentMetadata.GetMetadata(GetType()).ContainsKey(SelectedKey) == false)
            {
                Selected = 0;
            }
            Tabs = tabs;
        }
    }
}
