using System;
using System.Linq;
using System.Collections;
using System.ComponentModel;
using System.Text;
using System.Diagnostics;
using Bphx.Cool.Xml;
using Bphx.Cool.UI;

namespace Bphx.Cool;

/// <summary>
/// Utility class that specifies how to clone, copy and set/get ordinary 
/// data according to a data mapping.
/// </summary>
public sealed class DataUtils
{
  /// <summary>
  /// Splits a binding path into components. 
  /// </summary>
  /// <param name="value">a binding path.</param>
  /// <returns>an array of components.</returns>
  public static string[] Split(string value)
  {
    return string.IsNullOrWhiteSpace(value) ? Empty : value.Split(SplitChars);
  }

  /// <summary>
  /// <para>
  /// Copies source object into the target, 
  /// matching properties using MemberAttribute layout.
  /// </para>
  /// <para>
  /// This method is used for beans generated from different models, 
  /// which match structurally only.
  /// </para>
  /// </summary>
  /// <typeparam name="S">a type of a source instance.</typeparam>
  /// <typeparam name="T">a type of a target instance.</typeparam>
  /// <param name="source">a source instance.</param>
  /// <param name="target">a target instance.</param>
  /// <param name="errorOnMismatch">
  /// determines whether to throw an exception in case of layout mismatch (true)
  /// or write a warning message to trace.
  /// </param>
  /// <returns>a target instance.</returns>
  public static T Copy<S, T>(S source, T target, bool errorOnMismatch)
    where S : class
    where T : class
  {
    if (source == null)
    {
      throw new ArgumentNullException(nameof(source));
    }

    if (target == null)
    {
      throw new ArgumentNullException(nameof(target));
    }

    Copy(source, null, target, null, errorOnMismatch);

    return target;
  }

  /// <summary>
  ///   <para>Creates a deep clone of a specified object.</para>
  ///   <para>
  ///     Note that call of this method potentially may result in an
  ///     infinitive recursion, thus it should be used with care.
  ///   </para>
  /// </summary>
  /// <param name="instance">an object to clone.</param>
  /// <returns>the cloned Object.</returns>
  public static object Clone(object instance)
  {
    if (instance == null)
    {
      return null;
    }

    if ((instance is string) ||
      instance.GetType().IsValueType ||
      (instance is UIObject))
    {
      return instance;
    }

    if (instance is ICloneable cloneable)
    {
      return cloneable.Clone();
    }

    var clone = TypeDescriptor.CreateInstance(
      instance as System.IServiceProvider,
      instance.GetType(),
      null,
      null);

    Copy(instance, clone, null);

    return clone;
  }

  /// <summary>
  /// Copy import object to the export one according to 
  /// the specified data mapping.
  /// </summary>
  /// <param name="export">an export object.</param>
  /// <param name="import">an import object.</param>
  /// <param name="dataMappings">
  /// an array of data mappings of export to import.
  /// </param>
  /// <param name="direction">the copy direction to use.</param>
  /// <returns>
  /// <c>true</c>  if value is change, and <c>false</c> otherwise.
  /// </returns>
  public static bool Copy(
    object export,
    object import,
    Map[] dataMappings,
    CopyDirection direction)
  {
    if (dataMappings == null)
    {
      return false;
    }

    object source;
    object target;

    if (direction == CopyDirection.ExportToImport)
    {
      source = export;
      target = import;
    }
    else
    {
      source = import;
      target = export;
    }

    var changed = false;

    // copy data
    foreach(var dataMapping in dataMappings)
    {
      string[] sourceDataMapping = Split(dataMapping.From);
      string[] targetDataMapping = Split(dataMapping.To);

      if (direction != CopyDirection.ExportToImport)
      {
        var mapping = sourceDataMapping;

        sourceDataMapping = targetDataMapping;
        targetDataMapping = mapping;
      }

      try
      {
        changed |=
          Copy(source, target, sourceDataMapping, 0, targetDataMapping, 0);
      }
      catch(Exception e)
      {
        var builder = new StringBuilder();

        builder.Append("Cannot copy ");

        for(int i = 0; i < sourceDataMapping.Length; i++)
        {
          if (i > 0)
          {
            builder.Append('.');
          }

          builder.Append(sourceDataMapping[i]);
        }

        builder.Append(" to ");

        for(int i = 0; i < targetDataMapping.Length; i++)
        {
          if (i > 0)
          {
            builder.Append('.');
          }

          builder.Append(targetDataMapping[i]);
        }

        builder.Append(" due to the following error: ");
        builder.Append(
          e.InnerException != null ? e.InnerException.Message : e.Message);

        throw new InvalidOperationException(builder.ToString(), e);
      }
    }

    return changed;
  }

  /// <summary>
  /// Sets an object's property value according with the 
  /// specified data binding.
  /// </summary>
  /// <param name="instance">the object instance.</param>
  /// <param name="dataBinding">the data binding.</param>
  /// <param name="value">the value to set.</param>
  /// <returns>
  /// <c>true</c>  if value is change, and <c>false</c> otherwise.
  /// </returns>
  public static bool Set(object instance, string[] dataBinding, object value)
  {
    int size = dataBinding.Length - 1;

    for(int i = 0; i < size; i++)
    {
      instance = Get(instance, dataBinding[i]);
    }

    return Set(instance, dataBinding[size], value);
  }

  /// <summary>
  /// Gets an object's property value according with 
  /// the specified data binding.
  /// </summary>
  /// <param name="instance">the object instance.</param>
  /// <param name="dataBinding">the data binding.</param>
  /// <returns>a value of the specified object's property.</returns>    
  public static object Get(object instance, string[] dataBinding)
  {
    foreach(string name in dataBinding)
    {
      instance = Get(instance, name);
    }

    return instance;
  }

  /// <summary>
  /// Calls an action over each view member of the instance.
  /// </summary>
  /// <param name="instance">An instance to traverse.</param>
  /// <param name="viewAction">
  /// <para>An action to call on the view.</para>
  /// <para>Action is invoked with arguments:</para>
  /// <list type="bullet">
  ///   <item>An instance whose member is <see cref="View"/>.</item>
  ///   <item>A property name in the instance.</item>
  ///   <item><see cref="View"/> a view to act upon.</item>
  /// </list>
  /// </param>
  /// <param name="arrayAction">
  /// <para>An action to call on the list.</para>
  /// <para>Action is invoked with arguments:</para>
  /// <list type="bullet">
  ///   <item>An instance whose member is <see cref="IList"/>.</item>
  ///   <item>A property name in the instance.</item>
  ///   <item><see cref="IList"/> a view to act upon.</item>
  /// </list>
  /// </param>
  public static void ForEachView(
    object instance,
    Action<View> viewAction,
    Action<IArray> arrayAction)
  {
    if (instance == null)
    {
      return;
    }

    if (instance is IList list)
    {
      if (list is IArray array)
      {
        arrayAction?.Invoke(array);
      }

      foreach(var item in list)
      {
        ForEachView(item, viewAction, arrayAction);
      }

      return;
    }

    if (instance is View view)
    {
      viewAction?.Invoke(view);
    }

    if ((instance is string) || (instance is ValueType))
    {
      return;
    }

    foreach(PropertyDescriptor property in
      TypeDescriptor.GetProperties(instance))
    {
      if (property.Attributes[typeof(ComputedAttribute)] == null)
      {
        ForEachView(property.GetValue(instance), viewAction, arrayAction);
      }
    }
  }

  /// <summary>
  /// Prints a byte array to a writer.
  /// </summary>
  /// <param name="builder">A string builder to print the dump.</param>
  /// <param name="data">A data to dump.</param>
  /// <param name="encoding">String encoding.</param>
  public static void Dump(
    StringBuilder builder,
    Span<byte> data,
    Encoding encoding)
  {
    const int line = 16;
    Span<char> chars = stackalloc char[line];

    builder.AppendLine("--- Start of dump ---");

    for(var position = 0; position < data.Length; position += line)
    {
      builder.Append(position.ToString("X4"));
      builder.Append(": ");

      for(var j = 0; j < line; j++)
      {
        if (position + j < data.Length)
        {
          var b = data[position + j];

          builder.Append(b.ToString("X2"));
          builder.Append(' ');
        }
        else
        {
          builder.Append("   ");
        }

        if (j == line / 2 - 1)
        {
          builder.Append(' ');
        }
      }

      builder.Append("| ");

      var count = encoding.GetChars(
        data.Slice(position, Math.Min(line, data.Length - position)),
        chars);

      for(var j = 0; j < count; j++)
      {
        char c = chars[j];

        builder.Append(c < ' ' ? '.' : c);
      }

      builder.AppendLine();
    }

    builder.AppendLine("--- End of dump ---");
    builder.AppendLine();
  }

  #region Auxiliary methods
  /// <summary>
  /// Copies properties from source to target.
  /// </summary>
  /// <param name="source">Source instance.</param>
  /// <param name="target">Target instance.</param>
  /// <param name="properties">Optional properties collection.</param>
  private static void Copy(
    object source,
    object target,
    PropertyDescriptorCollection properties)
  {
    if (properties == null)
    {
      properties = TypeDescriptor.GetProperties(source);
    }

    foreach(PropertyDescriptor property in properties)
    {
      if (property.Attributes[typeof(ComputedAttribute)] != null)
      {
        continue;
      }

      var sourceValue = property.GetValue(source);
      var sourceList = sourceValue as IList;

      if (sourceList != null)
      {
        var targetList = (IList)property.GetValue(target);

        if (targetList == null)
        {
          if (targetList.IsReadOnly)
          {
            continue;
          }

          targetList = (IList)TypeDescriptor.CreateInstance(
            null,
            sourceList.GetType(),
            null,
            null);

          property.SetValue(target, targetList);
        }

        for(int i = 0, c = sourceList.Count; i < c; i++)
        {
          targetList.Add(Clone(sourceList[i]));
        }
      }
      else if (sourceValue == null)
      {
        if (property.IsReadOnly)
        {
          continue;
        }

        property.SetValue(target, null);
      }
      else
      {
        if (!property.IsReadOnly)
        {
          if ((sourceValue is string) || sourceValue.GetType().IsValueType)
          {
            property.SetValue(target, sourceValue);

            continue;
          }

          if (source is ICloneable cloneable)
          {
            property.SetValue(target, cloneable.Clone());

            continue;
          }
        }

        object targetValue = property.GetValue(target);

        if (targetValue == null)
        {
          if (property.IsReadOnly)
          {
            continue;
          }

          targetValue = TypeDescriptor.CreateInstance(
            sourceList as System.IServiceProvider,
            sourceList.GetType(),
            null,
            null);

          property.SetValue(target, targetValue);
        }

        Copy(
          sourceValue,
          targetValue,
          property.GetChildProperties());
      }
    }
  }

  /// <summary>
  /// Gets an object property's value according with the specified name 
  /// or index.
  /// </summary>
  /// <param name="instance">the object.</param>
  /// <param name="name">the property name or index.</param>
  /// <returns>a value of the specified property.</returns>
  public static object Get(object instance, string name)
  {
    if (instance == null)
    {
      return null;
    }

    if (instance is IList list)
    {
      return list[Convert.ToInt32(name)];
    }

    var property = TypeDescriptor.GetProperties(instance).Find(name, true);

    return property.GetValue(instance);
  }

  /// <summary>
  /// Sets a value to the specified property name.
  /// </summary>
  /// <param name="instance">an object instance.</param>
  /// <param name="name">the property name.</param>
  /// <param name="value">the value to set.</param>
  /// <returns>
  /// <c>true</c> if value is change, and <c>false</c> otherwise.
  /// </returns>
  public static bool Set(object instance, string name, object value)
  {
    if (instance == null)
    {
      return false;
    }

    if (instance is IList list)
    {
      list[Convert.ToInt32(name)] = value;

      return true;
    }
    else if (value is IList)
    {
      return Copy(
        value,
        instance,
        new[] { name },
        0,
        Empty,
        0);
    }
    else
    {
      var changed = false;
      var set = true;
      var property =
        TypeDescriptor.GetProperties(instance).Find(name, false);

      if ((property == null) ||
        (property.Attributes[typeof(ComputedAttribute)] != null))
      {
        return false;
      }

      var propertyMember =
        property.Attributes[typeof(MemberAttribute)] as MemberAttribute;
      var propertyMembers = propertyMember?.Members;
      var targetValue = property.GetValue(instance);
      var heap = value as Heap;
      var view = value as View;

      if (((value != null) &&
        (heap == null) &&
        (value is not string) &&
        !property.PropertyType.IsValueType) ||
        (propertyMembers?.Length > 0) ||
        ((view != null) &&
          (targetValue != null) &&
          (view.GetType() != targetValue.GetType())))
      {
        if ((property.Converter != null) &&
          property.Converter.CanConvertFrom(value.GetType()))
        {
          value = property.Converter.ConvertFrom(value);
        }
        else
        {
          set = false;

          if (targetValue != null)
          {
            var sourceProperties = TypeDescriptor.GetProperties(value);
            var targetAllProperties = TypeDescriptor.GetProperties(targetValue);

            foreach(var targetProperty in
              propertyMembers?.Length > 0 ?
                propertyMember.Members.
                  Select(name => targetAllProperties.Find(name, true)) :
                targetAllProperties.OfType<PropertyDescriptor>())
            {
              if ((targetProperty != null) &&
                (targetProperty.Attributes[typeof(ComputedAttribute)] ==
                  null))
              {
                var sourceProperty =
                  sourceProperties.Find(targetProperty.Name, true);

                if (sourceProperty != null)
                {
                  changed |= Set(
                    targetValue,
                    targetProperty.Name,
                    sourceProperty.GetValue(value));
                }
              }
            }
          }
        }
      }

      if (set && !property.IsReadOnly)
      {
        var oldHeap = targetValue as Heap;

        changed = (targetValue != value) &&
          ((heap == null) || (targetValue == null) || !oldHeap.IsEquivalent(heap)) &&
          ((value == null) || (targetValue == null) || !targetValue.Equals(value));

        if (changed)
        {
          var copy = Clone(value);

          if ((copy is View copyView) && (targetValue is View targetView))
          {
            copyView.AssignScreenFields(targetView);
          }

          property.SetValue(instance, copy);
        }
      }

      return changed;
    }
  }

  /// <summary>
  /// Copies recursively source object to target according with lists 
  /// of properties.
  /// </summary>
  /// <param name="source">the source object.</param>
  /// <param name="target">the target object.</param>
  /// <param name="sourceDataBinding">
  /// an array of properties that defines what to be copied.
  /// </param>
  /// <param name="sourceIndex">
  /// defines a start index in the sourceDataBinding array.
  /// </param>
  /// <param name="targetDataBinding">
  /// an array of properties that defines where to copy.
  /// </param>
  /// <param name="targetIndex">
  /// defines a start index in the targetDataBinding array.
  /// </param>
  /// <returns>
  /// <c>true</c>  if value is change, and <c>false</c> otherwise.
  /// </returns>
  private static bool Copy(
    object source,
    object target,
    string[] sourceDataBinding,
    int sourceIndex,
    string[] targetDataBinding,
    int targetIndex)
  {
    if ((source == null) || (target == null))
    {
      return false;
    }

    for(var c = sourceDataBinding.Length; sourceIndex < c;)
    {
      source = Get(source, sourceDataBinding[sourceIndex++]);

      if (source is IList)
      {
        break;
      }
    }

    for(var c = targetDataBinding.Length - 1; targetIndex < c;)
    {
      target = Get(target, targetDataBinding[targetIndex++]);

      if (target is IList)
      {
        break;
      }
    }

    var sourceList = source as IList;
    var targetList = target as IList;

    if ((sourceList == null) && (targetList == null))
    {
      var name = targetDataBinding[targetIndex];

      return Set(target, name, source);
    }
    else if ((sourceList != null) && (targetList != null))
    {
      var targetArray = targetList as IArray;
      var changed = sourceList.Count != targetList.Count;

      targetArray.Clear();

      var size = sourceList.Count;
      var capacity = targetArray != null ? targetArray.Capacity : size;

      if (size > capacity)
      {
        var message = new StringBuilder();

        message.Append("A capacity (");
        message.Append(capacity);
        message.Append(") of target array: ");

        for(var i = 0; i < targetIndex; ++i)
        {
          if (i > 0)
          {
            message.Append('.');
          }

          message.Append(targetDataBinding[i]);
        }

        message.Append(" is less than size (");
        message.Append(size);
        message.Append(") of source array: ");

        for(var i = 0; i < sourceIndex; ++i)
        {
          if (i > 0)
          {
            message.Append('.');
          }

          message.Append(sourceDataBinding[i]);
        }

        message.Append(
          ". Elements with index above target capacity are not copied.");

        Trace.TraceWarning(message.ToString());

        size = capacity;
      }

      for(var i = 0; i < size; i++)
      {
        changed |= Copy(
          sourceList[i],
          targetList[i],
          sourceDataBinding,
          sourceIndex,
          targetDataBinding,
          targetIndex);
      }

      return changed;
    }
    else if ((sourceList == null) && (targetList != null))
    {
      throw new InvalidOperationException("Cannot copy an object to an array.");
    }
    else
    {
      throw new InvalidOperationException("Cannot copy an array to an object.");
    }
  }

  /// <summary>
  /// Implementation method for the structural instance copying.
  /// </summary>
  /// <param name="source">a source instance.</param>
  /// <param name="sourceProperties">a list of source properties.</param>
  /// <param name="target">a target instance.</param>
  /// <param name="targetProperties">a list of target properties</param>
  /// <param name="errorOnMismatch">
  /// determines whether to throw an exception in case of layout mismatch (true)
  /// or write a warning message to trace.
  /// </param>
  private static void Copy(
    object source,
    string[] sourceProperties,
    object target,
    string[] targetProperties,
    bool errorOnMismatch)
  {
    var index = 0;
    var hasSourceProperties = sourceProperties?.Length > 0;
    var hasTargetProperties = targetProperties?.Length > 0;
    var sourceDescriptorsProperties = hasSourceProperties ? null :
      MemberAttribute.GetProperties(source.GetType());
    var targetDescriptorsProperties = hasTargetProperties ? null :
      MemberAttribute.GetProperties(target.GetType());
    var sourceDescriptors = !hasSourceProperties ? null :
      TypeDescriptor.GetProperties(source.GetType());
    var targetDescriptors = !hasTargetProperties ? null :
      TypeDescriptor.GetProperties(target.GetType());

    var sourceView = source as View;
    var sourceScreenFields = sourceView?.ScreenFields;
    var targetView = target as View;
    var targetScreenFields = targetView?.ScreenFields;

    while(true)
    {
      var sourceDescriptor = hasSourceProperties ?
        sourceDescriptors.Find(sourceProperties[index], true) :
        sourceDescriptorsProperties[index];
      var targetDescriptor = hasTargetProperties ?
        targetDescriptors.Find(sourceProperties[index], true) :
        targetDescriptorsProperties[index];

      if ((sourceDescriptor == null) != (targetDescriptor == null))
      {
        goto Error;
      }

      if (sourceDescriptor == null)
      {
        return;
      }

      var sourceMember =
        sourceDescriptor.Attributes[typeof(MemberAttribute)] as
          MemberAttribute;
      var targetMember =
        targetDescriptor.Attributes[typeof(MemberAttribute)] as
          MemberAttribute;

      if ((sourceMember == null) != (targetMember == null))
      {
        goto Error;
      }

      if (sourceMember == null)
      {
        return;
      }

      if (targetDescriptor.Attributes[typeof(ComputedAttribute)] != null)
      {
        ++index;

        continue;
      }

      var sourceMemberType = sourceMember.Type;
      var targetMemberType = targetMember.Type;

      if (targetView != null)
      {
        if (sourceScreenFields.
          TryGetValue(sourceDescriptor.Name, out ScreenField screenField))
        {
          targetScreenFields[targetDescriptor.Name] = screenField.Clone();
        }
        else
        {
          targetScreenFields.Remove(targetDescriptor.Name);
        }
      }

      switch(sourceMemberType)
      {
        case MemberType.Object:
        {
          if (targetMemberType != MemberType.Object)
          {
            goto Error;
          }

          var sourceType = sourceDescriptor.PropertyType;
          var targetType = targetDescriptor.PropertyType;
          var sourceMemberProperties = sourceMember.Members;
          var targetMemberProperties = targetMember.Members;
          var sourceValue = sourceDescriptor.GetValue(source);
          var targetValue = targetDescriptor.GetValue(target);
          var toSet = targetValue == null;

          if (toSet)
          {
            if (targetDescriptor.IsReadOnly)
            {
              goto Error;
            }

            targetValue = Activator.CreateInstance(targetType);
          }

          if (sourceValue == null)
          {
            sourceValue = Activator.CreateInstance(sourceType);
          }

          var sourceArray = sourceValue as IArray;
          var targetArray = targetValue as IArray;

          if ((sourceArray == null) != (targetArray == null))
          {
            goto Error;
          }

          if (sourceArray != null)
          {
            var size = sourceArray.Count;

            targetArray.Count = size;

            for(int i = 0; i < size; ++i)
            {
              Copy(
                sourceArray[i],
                sourceMemberProperties,
                targetArray[i],
                targetMemberProperties,
                errorOnMismatch);
            }
          }
          else
          {
            Copy(
              sourceValue,
              sourceMemberProperties,
              targetValue,
              targetMemberProperties,
              errorOnMismatch);
          }

          if (toSet)
          {
            targetDescriptor.SetValue(target, targetValue);
          }

          break;
        }
        case MemberType.Char:
        case MemberType.Varchar:
        {
          if ((targetMemberType != MemberType.Char) &&
            (targetMemberType != MemberType.Varchar))
          {
            goto Error;
          }

          targetDescriptor.SetValue(
            target,
            sourceDescriptor.GetValue(source));

          break;
        }
        case MemberType.Binary:
        case MemberType.Varbinary:
        {
          if ((targetMemberType != MemberType.Binary) &&
            (targetMemberType != MemberType.Varbinary))
          {
            goto Error;
          }

          targetDescriptor.SetValue(
            target,
            sourceDescriptor.GetValue(source));

          break;
        }
        case MemberType.Date:
        case MemberType.Time:
        case MemberType.Timestamp:
        {
          if ((targetMemberType != MemberType.Date) &&
            (targetMemberType != MemberType.Time) &&
            (targetMemberType != MemberType.Timestamp))
          {
            goto Error;
          }

          targetDescriptor.SetValue(
            target,
            sourceDescriptor.GetValue(source));

          break;
        }
        case MemberType.BinaryNumber:
        case MemberType.PackedDecimal:
        case MemberType.Number:
        {
          if ((targetMemberType != MemberType.Number) &&
            (targetMemberType != MemberType.BinaryNumber) &&
            (targetMemberType != MemberType.PackedDecimal))
          {
            goto Error;
          }

          targetDescriptor.SetValue(
            target,
            sourceDescriptor.GetValue(source));

          break;
        }
        default:
        {
          goto Error;
        }
      }

      ++index;
    }

    Error:
    var message = new StringBuilder("Layout mismatch on property index: ");

    message.Append(index);
    message.Append(",\nsource: ");
    message.Append(source.GetType().FullName);

    if ((sourceProperties != null) && (sourceProperties.Length > 0))
    {
      message.Append('(');

      for(var i = 0; i < sourceProperties.Length; ++i)
      {
        if (i > 0)
        {
          message.Append(", ");
        }

        message.Append(sourceProperties[i]);
      }

      message.Append(')');
    }

    message.Append(", and\ntarget: ");
    message.Append(target.GetType().FullName);

    if ((targetProperties != null) && (targetProperties.Length > 0))
    {
      message.Append('(');

      for(int i = 0; i < targetProperties.Length; ++i)
      {
        if (i > 0)
        {
          message.Append(", ");
        }

        message.Append(targetProperties[i]);
      }

      message.Append(')');
    }

    message.Append('.');

    if (errorOnMismatch)
    {
      throw new InvalidOperationException(message.ToString());
    }

    Trace.TraceWarning(message.ToString());
  }
  #endregion

  /// <summary>
  /// Empty list.
  /// </summary>
  private static readonly string[] Empty = Array.Empty<string>();

  /// <summary>
  /// Split characters.
  /// </summary>
  private static readonly char[] SplitChars = { '.', '[', ']' };
}

/// <summary>
/// Defines a copy direction.
/// </summary>
public enum CopyDirection
{
  /// <summary>
  /// Copy import to export.
  /// </summary>
  ImportToExport,

  /// <summary>
  /// Copy export to import.
  /// </summary>
  ExportToImport
}
