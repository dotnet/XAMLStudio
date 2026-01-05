// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace XamlStudio.Toolkit.Models;

/// <summary>
/// Record of a Binding/Conversion Occuring
/// </summary>
public class ConversionRecord
{
    public DateTime TimeStamp { get; } = DateTime.Now;

    public object Value { get; private set; }

    public object Result { get; private set; }

    public object ResultOrValue { get { return Result ?? Value; } }

    public bool HasResult { get; private set; }

    public bool IsSuccessful { get; private set; }

    public Exception ExceptionObject { get; private set; }

    public XamlBindingInfo Parent { get; private set; }

    /// <summary>
    /// If there was no converter, then it's just a value that was passed thru.
    /// </summary>
    /// <param name="value"></param>
    public ConversionRecord(XamlBindingInfo parent, object value)
    {
        Parent = parent;
        Value = value;
        IsSuccessful = true;
    }

    /// <summary>
    /// Value was successfully converted to the specified Result.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="result"></param>
    public ConversionRecord(XamlBindingInfo parent, object value, object result)
    {
        Parent = parent;
        Value = value;
        Result = result;
        HasResult = true;
        IsSuccessful = true;
    }

    /// <summary>
    /// There was an error converting the given value.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="error"></param>
    public ConversionRecord(XamlBindingInfo parent, object value, Exception error)
    {
        Parent = parent;
        Value = value;
        ExceptionObject = error;
    }

    public override string ToString()
    {
        var start = string.IsNullOrWhiteSpace(Parent.ElementName) ? Parent.ElementTypeName : Parent.ElementName + "[" + Parent.ElementTypeName + "]";

        return start + "." + Parent.PropertyName + " = " + ResultOrValue;
    }
}
