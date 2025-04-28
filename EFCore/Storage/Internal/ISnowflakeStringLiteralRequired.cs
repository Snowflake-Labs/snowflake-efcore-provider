// <copyright file="ISnowflakeStringLiteralRequired.cs" company="Snowflake Inc">
//         Copyright (c) 2019-2023 Snowflake Inc. All rights reserved.
//  </copyright>

using System;
using System.Data;
using System.Globalization;
using System.Text;
using Snowflake.Data.Client;
using Snowflake.Data.Core;

namespace Snowflake.EntityFrameworkCore.Storage.Internal;

/// <summary>
/// Interface use to identify types which should be sent as literal values.
/// </summary>
public interface ISnowflakeStringLiteralRequired
{
}