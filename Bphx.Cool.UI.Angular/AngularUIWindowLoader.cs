using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using AngleSharp.Html.Dom;

using static Bphx.Cool.Functions;

namespace Bphx.Cool.UI;

/// <summary>
/// Implements <see cref="IUIWindowLoader"/> interface for Angular pages.
/// </summary>
public class AngularUIWindowLoader: IUIWindowLoader
{
  /// <summary>
  /// Creates <see cref="AngularUIWindowLoader"/> instance.
  /// </summary>
  /// <param name="resourceResolver">
  /// A window resource stream resolver by a procedure and a window name.
  /// </param>
  /// <param name="controlResolver">
  /// An UI control resolver for <see cref="UIWindow"/> 
  /// by control type and <see cref="IElement"/>.
  /// </param>
  public AngularUIWindowLoader(
    Func<IProcedure, string, Stream> resourceResolver,
    Func<UIWindow, string, IElement, UIControl> controlResolver = null)
  {
    this.resourceResolver = resourceResolver ??
      throw new ArgumentNullException(nameof(resourceResolver));
    this.controlResolver = controlResolver ??
      ((window, type, elemenet) => new());
  }

  /// <summary>
  /// Loads window definition.
  /// </summary>
  /// <param name="procedure">A <see cref="IProcedure"/> instance.</param>
  /// <param name="window">A window name.</param>
  /// <returns><see cref="UIWindow"/> instance.</returns>
  public UIWindow Load(IProcedure procedure, string window)
  {
    var parser = new HtmlParser();
    var document = null as IHtmlDocument;

    using(var stream =
      resourceResolver(procedure, window) ??
      throw new InvalidOperationException(
        @$"No page definition is available for procedure: {procedure.Name
        } and window: {window}."))
    {
      document = parser.ParseDocument(stream);
    }

    var result = Load(document);

    result.PrimaryWindow = !IsEmpty(result.Name) && 
      Equal(result.Name, procedure.PrimaryWindow);
    result.Procedure = procedure;

    if (result.Name == procedure.PrimaryWindow)
    {
      result.PrimaryWindow = true;
    }

    return result;
  }

  /// <summary>
  /// Loads the specified resource stream and returns UIWindow instance.
  /// </summary>
  /// <param name="path">an input resource to parse.</param>
  /// <returns>an UIWindow instance or null.</returns>
  protected virtual UIWindow Load(IHtmlDocument document)
  {
    var prompts = new Dictionary<string, string>();
    var elements = document.QuerySelectorAll("[coolFor]");

    foreach(var prompt in elements)
    {
      var parent = prompt.ParentElement;

      if (parent.GetAttribute("coolHeader") != null)
      {
        // skip coolHeader
        continue;
      }

      var name = prompt.GetAttribute("coolFor");
      var text = prompt.TextContent;

      if (!string.IsNullOrEmpty(text))
      {
        prompts.Add(name, text);
      }
    }

    elements = document.QuerySelectorAll("[coolType]");

    var window = new UIWindow();
    var controls = new List<UIControl>();
    var radioGroups = new Dictionary<string, UIRadioButtonGroup>();

    foreach(var element in elements)
    {
      var parent = element.ParentElement;
      var type = element.GetAttribute("coolType");
      var model = element.GetAttribute("[(ngModel)]");
      var modelElement = IsEmpty(model) ? null : element;

      if (modelElement == null)
      {
        modelElement = element.Children.
          FirstOrDefault(
            c =>
              c.HasAttribute("[(ngModel)]") &&
              !c.HasAttribute("coolType"));
        model = modelElement?.GetAttribute("[(ngModel)]");
      }

      var name = element.GetAttribute("coolName");

      if (IsEmpty(name))
      {
        name = modelElement?.GetAttribute("name") ??
          element.GetAttribute("name");
      }

      var control = null as UIControl;

      if (string.Compare("WINPRIME", type, true) == 0 ||
        (string.Compare("DLGBOX", type, true) == 0) ||
        (string.Compare("WINSEC", type, true) == 0))
      {
        window.Caption = element.GetAttribute("coolTitle");
        window.DisplayExitStateMessage = document.
          QuerySelector("[coolDisplayErrorMessage]") != null;

        var left = element.GetAttribute("coolLeft");
        var top = element.GetAttribute("coolTop");
        var width = element.GetAttribute("coolWidth");
        var height = element.GetAttribute("coolHeight");
        var alignment = element.GetAttribute("coolWindowAlignment");

        if (!IsEmpty(left))
        {
          window.LeftValue = left;
        }

        if (!IsEmpty(top))
        {
          window.TopValue = top;
        }
        if (!IsEmpty(width))
        {
          window.WidthValue = width;
        }
        if (!IsEmpty(height))
        {
          window.HeightValue = height;
        }

        if (string.Compare("system", alignment, true) == 0)
        {
          window.Position = InitialPosition.System;
        }
        else if (string.Compare("mouse", alignment, true) == 0)
        {
          window.Position = InitialPosition.Mouse;
        }
        // else designed position

        window.Modal = !element.HasAttribute("coolModeless");
        window.Resizable = element.HasAttribute("coolResizable");

        control = window;
      }
      else if (string.Compare("WNGROUP", type, true) == 0)
      {
        control = new UIGroupBox
        {
          Caption = element.QuerySelector("fieldset > legend")?.TextContent
        };
      }
      else if (string.Compare("TOOLBAR", type, true) == 0)
      {
        control = new UIToolBar();
      }
      else if (string.Compare("STATSBAR", type, true) == 0)
      {
        control = new UIStatusBar();
      }
      else if ((string.Compare("SNGLNFLD", type, true) == 0) ||
        (string.Compare("error-message", type, true) == 0))
      {
        control = new UIField();
      }
      else if (string.Compare("MLTLNFLD", type, true) == 0)
      {
        control = new UIField { Multiline = true };
      }
      else if (string.Compare("WINLIT", type, true) == 0)
      {
        control = new UILiteral { Caption = element.TextContent };
      }
      else if (string.Compare("PUSHBTN", type, true) == 0)
      {
        control = new UIButton { Caption = element.GetAttribute("value") };
      }
      else if (string.Compare("CHKBOX", type, true) == 0)
      {
        control = new UICheckBox { Prompt = element.TextContent };

      }
      else if (string.Compare("MENUITEM", type, true) == 0)
      {
        var link = element.QuerySelector("a");

        control = new UIMenu() { Caption = link?.TextContent };
      }
      else if (string.Compare("STNDLST", type, true) == 0)
      {
        var listBox = new UIListBox();

        var columns = parent.Children.
          FirstOrDefault(c => c.HasAttribute("coolHeader")).
          Children.
          Where(c => c.HasAttribute("coolPrompt"));

        foreach(var column in columns)
        {
          var item = new UIListItem
          {
            Prompt = column.TextContent,
            Visible = !column.ClassList.Contains("[coolShow=false]")
          };

          item.SetDefault();
          listBox.ListItems.List.Add(item);
        }

        control = listBox;
      }
      else if (string.Compare("DRPDWNN", type, true) == 0)
      {
        control = new UIDropDownList();
      }
      else if (string.Compare("DRPDWNE", type, true) == 0)
      {
        control = new UIEnterableDropDownList();
      }
      else if (string.Compare("COMBGRP", type, true) == 0)
      {
        control = new UIEnterableDropDownList();
      }
      else if (string.Compare("RDBTNOC", type, true) == 0)
      {
        var radioButton = new UIRadioButton
        {
          Name = name,
          Prompt = element.TextContent,
          Value = modelElement?.GetAttribute("value"),
          Visible = !element.ClassList.Contains("[coolShow=false]")
        };

        name = modelElement?.GetAttribute("name") ?? name;

        var group = null as UIRadioButtonGroup;

        if (!radioGroups.TryGetValue(name, out group))
        {
          control = radioGroups[name] = group = new();
        }

        CheckDefaultBinding(radioButton, modelElement, model);
        radioButton.SetDefault();
        controls.Add(radioButton);
        group.RadioButtons.List.Add(radioButton);
      }
      else if (string.Compare("TABSET", type, true) == 0)
      {
        var tabset = new UITabsetControl();

        foreach(var uibTab in
          element.Children.
            FirstOrDefault(
              c => string.Compare(c.TagName, "uib-tabset", true) == 0)?.
            Children.
            Where(c => string.Compare(c.TagName, "uib-tab", true) == 0))
        {
          var index = uibTab.GetAttribute("index");

          var tab = new UITabControl()
          {
            Caption = uibTab.Children.
              FirstOrDefault(
                c => string.Compare(c.TagName, "uib-tab-heading", true) == 0)?.
              TextContent
          };

          if (!string.IsNullOrEmpty(index))
          {
            if ((index[0] == '\'') || (index[0] == '\"'))
            {
              index = index[1..^1];
            }

            tab.Index = Convert.ToInt32(index);
          }

          tab.SetDefault();
          tabset.TabControls.List.Add(tab);
        }

        control = tabset;
      }
      else if (string.Compare("COMMDLG", type, true) == 0)
      {
        control = new UICommonDialog();
      }
      else if ((string.Compare("OCXCNTL", type, true) == 0) ||
        (string.Compare("OCXSRC", type, true) == 0) ||
        (string.Compare("OCXFIELD", type, true) == 0))
      {
        control = new UIOleControl();
      }
      else if (string.Compare("OLEAREA", type, true) == 0)
      {
        control = new UIOleArea();
      }
      else if (string.Compare("BITMAP", type, true) == 0)
      {
        control = new UIPicture();
      }
      else
      {
        control = controlResolver(window, type, element);
      }

      if (control != null)
      {
        control.Name = name;

        if ((control is UIInputControl inputControl) &&
          prompts.TryGetValue(name, out var prompt))
        {
          inputControl.Prompt = prompt;
        }

        if (control is not UIRadioButtonGroup)
        {
          control.Visible = !element.ClassList.Contains("[coolShow=false]");
        }

        var color = element.GetAttribute("coolBackground");

        if (!string.IsNullOrEmpty(color))
        {
          control.BackgroundColor = FromColor(color);
        }

        color = element.GetAttribute("coolColor");

        if (!string.IsNullOrEmpty(color))
        {
          control.ForegroundColor = FromColor(color);
        }

        if (window != control)
        {
          control.ReadOnly = element.HasAttribute("coolReadonly");

          var command = element.GetAttribute("coolEventsCommand");

          if (!string.IsNullOrEmpty(command))
          {
            control.Command = command;
          }
          else
          {
            command = element.GetAttribute("coolCommand");

            if (!string.IsNullOrEmpty(command))
            {
              control.Command = command;
            }
          }

          CheckDefaultBinding(control, modelElement, model);
          controls.Add(control);
        }

        control.SetDefault();
      }
    }

    controls.Reverse();
    window.Controls = controls;

    return window;
  }

  /// <summary>
  /// Checks default binding.
  /// </summary>
  /// <param name="control">An <see cref="UIControl"/>.</param>
  /// <param name="element">A model element, if any.</param>
  /// <param name="model">A model expression.</param>
  protected virtual void CheckDefaultBinding(
    UIControl control,
    IElement element,
    string model)
  {
    if(control.SupportsBinding &&
      (element != null) &&
      model.StartsWith("in."))
    {
      var defaultValue = element.GetAttribute("coolDefaultValue");

      if(!IsEmpty(defaultValue))
      {
        control.Binding = model["in.".Length..];
        control.DefaultValue = defaultValue;
      }
    }
  }

  /// <summary>
  /// A window resource stream resolver.
  /// </summary>
  private readonly Func<IProcedure, string, Stream> resourceResolver;

  /// <summary>
  /// An UI control resolver.
  /// </summary>
  private readonly Func<UIWindow, string, IElement, UIControl> controlResolver;
}
