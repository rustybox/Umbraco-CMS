// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;

namespace Umbraco.Tests.Common.Builders.Interfaces
{
    public interface IWithUpdateDateBuilder
    {
        DateTime? UpdateDate { get; set; }
    }
}
