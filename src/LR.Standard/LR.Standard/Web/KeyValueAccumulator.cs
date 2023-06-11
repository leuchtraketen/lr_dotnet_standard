// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.Extensions.Primitives;

namespace LR.Standard;

public struct KeyValueAccumulator
{
    private Dictionary<string, StringValues> _accumulator;
    private Dictionary<string, List<string>> _expandingAccumulator;

    public void Append(string key, string value)
    {
        if (this._accumulator == null)
        {
            this._accumulator = new Dictionary<string, StringValues>(StringComparer.OrdinalIgnoreCase);
        }

        StringValues values;
        if (this._accumulator.TryGetValue(key, out values))
        {
            if (values.Count == 0)
            {
                // Marker entry for this key to indicate entry already in expanding list dictionary
                this._expandingAccumulator[key].Add(value);
            }
            else if (values.Count == 1)
            {
                // Second value for this key
                this._accumulator[key] = new string[] { values[0], value };
            }
            else
            {
                // Third value for this key
                // Add zero count entry and move to data to expanding list dictionary
                this._accumulator[key] = default(StringValues);

                if (this._expandingAccumulator == null)
                {
                    this._expandingAccumulator = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
                }

                // Already 3 entries so use starting allocated as 8; then use List's expansion mechanism for more
                var list = new List<string>(8);
                var array = values.ToArray();

                list.Add(array[0]);
                list.Add(array[1]);
                list.Add(value);

                this._expandingAccumulator[key] = list;
            }
        }
        else
        {
            // First value for this key
            this._accumulator[key] = new StringValues(value);
        }

        this.ValueCount++;
    }

    public bool HasValues => this.ValueCount > 0;

    public int KeyCount => this._accumulator?.Count ?? 0;

    public int ValueCount { get; private set; }

    public Dictionary<string, StringValues> GetResults()
    {
        if (this._expandingAccumulator != null)
        {
            // Coalesce count 3+ multi-value entries into _accumulator dictionary
            foreach (var entry in this._expandingAccumulator)
            {
                this._accumulator[entry.Key] = new StringValues(entry.Value.ToArray());
            }
        }

        return this._accumulator ?? new Dictionary<string, StringValues>(0, StringComparer.OrdinalIgnoreCase);
    }
}
