using System;
using System.ComponentModel;
using System.Text;
using System.Text.Json.Serialization;
using System.Xml.Serialization;

using static Bphx.Cool.Functions;

namespace Bphx.Cool;

/// <summary>
/// A class representing a screen field.
/// </summary>
[Serializable]
public class ScreenField: INamed, ICloneable
{
	/// <summary>
	/// Creates a ScreenField instance.
	/// </summary>
	public ScreenField()
	{
		Intensity = Intensity.Normal;
    Highlighting = Highlighting.Normal;
	}

	/// <summary>
	/// Creates a ScreenField instance.
	/// </summary>
	/// <param name="that">an instance to copy from.</param>
	public ScreenField(ScreenField that)
	{
		Assign(that);
	}

  /// <summary>
  /// Creates a copy of this instance.
  /// </summary>
  /// <returns>a ScreenField instance.</returns>
  public ScreenField Clone() => new(this);

  /// <summary>
  /// Creates a copy of this instance.
  /// </summary>
  /// <returns>a ScreenField instance.</returns>
  object ICloneable.Clone()
  {
    return Clone();
  }

  /// <summary>
  /// Assigns field from other field.
  /// </summary>
  /// <param name="that">An instance to assign from.</param>
	public void Assign(ScreenField that)
	{
    this.Color = that.Color;
    this.Protected = that.Protected;
    this.Intensity = that.Intensity;
    this.Highlighting = that.Highlighting;
    this.Focused = that.Focused;
    this.Error = that.Error;
	}

  /// <summary>
  /// Indicates whether all fields (except possibly name) have default 
  /// values.
  /// </summary>
  /// <returns>
  /// true if all fields have default values, and false otherwise.
  /// </returns>
  public bool IsDefault()
  {
    return IsEmpty(Color) &&
      (Protected != true) &&
      (Intensity == Intensity.Normal) &&
      (Highlighting == Highlighting.Normal) &&
      !Focused &&
      (Error == false);
  }

  /// <summary>
  /// Returns string representation of the instance used 
  /// for debug purposes.
  /// </summary>
  /// <returns>A string representation of the instance.</returns>
  public override string ToString()
  {
    var result = new StringBuilder();
    var flag = false;

    result.Append('{');

    if (Name != null)
    {
      result.Append("name: ").Append(Name);
      flag = true;
    }

    if (Focused)
    {
      if (flag)
      {
        result.Append(", ");
      }

      flag = true;

      result.Append("focused: true");
    }

    if (Error)
    {
      if (flag)
      {
        result.Append(", ");
      }

      flag = true;
      result.Append("error: true");
    }

    if (Color != null)
    {
      if (flag)
      {
        result.Append(", ");
      }

      flag = true;
      result.Append("color: ").Append(Color);
    }

    if (Protected != null)
    {
      if (flag)
      {
        result.Append(", ");
      }

      flag = true;
      result.Append("protected: ").Append(Protected == true ? "true" : "false");
    }

    if (flag)
    {
      result.Append(", ");
    }

    if (Intensity != Intensity.Normal)
    {
      result.Append("intensity: ").Append(Intensity.ToString()).Append(", ");
    }

    if (Highlighting != Highlighting.Normal)
    {
      result.Append("highlight: ").Append(Highlighting.ToString()).Append('}');
    }

    return result.ToString();
  }

  /// <summary>
  /// Reference to shared state, if any.
  /// </summary>
  [XmlIgnore, JsonIgnore]
  public ScreenFields ScreenFields { get; set; }

  /// <summary>
  /// A field name.
  /// </summary>
  public string Name { get; set; }

	/// <summary>
  /// Gets and sets a color of the screen field.
	/// </summary>
	public string Color { get; set; }

  /// <summary>
  ///  <para>Gets and sets a field protection status.</para>
  ///  <para>
  ///   <b>Note: </b> Protection is a three state value.
  ///   null means value is unspecified.
  ///  </para>
  /// </summary>
	public bool? Protected { get; set; }

  /// <summary>
  /// Gets and sets an intensity of the screen field.
  /// </summary>
  [DefaultValue(Intensity.Normal)]
  public Intensity Intensity { get; set; }

  /// <summary>
  /// Gest and sets an error status for the screen field.
  /// </summary>
  [DefaultValue(false)]
  public bool Error { get; set; }

  /// <summary>
  /// Gets and sets a focused status for the screen field.
  /// </summary>
  [DefaultValue(false)]
  public bool Focused 
  {
    get => ScreenFields != null ? ScreenFields.Focused == this : focused;
    set
    {
      if (ScreenFields != null)
      {
        if (value)
        {
          if ((ScreenFields.Focused != null) && (ScreenFields.Focused != this))
          {
            ScreenFields.Focused.Focused = false;
          }

          ScreenFields.Focused = this;
        }
        else
        {
          if (ScreenFields.Focused == this)
          {
            ScreenFields.Focused = null;
          }
        }
      }

      focused = value;
    } 
  }

  /// <summary>
  /// Gets and sets a highlight value for the screen field.
  /// </summary>
  [DefaultValue(Highlighting.Normal)]
  public Highlighting Highlighting { get; set; }

  /// <summary>
  /// Focused indicator.
  /// </summary>
  private bool focused;
}
