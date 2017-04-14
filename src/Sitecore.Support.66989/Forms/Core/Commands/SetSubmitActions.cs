using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Form.Core.ContentEditor.Data;
using Sitecore.Forms.Core.Data;
using Sitecore.Globalization;
using Sitecore.Text;
using Sitecore.Web;
using Sitecore.Web.UI.Sheer;
using Sitecore.WFFM.Abstractions.Dependencies;
using System;
using System.Web;

namespace Sitecore.Support.Forms.Core.Commands
{
  [Serializable]
  public class SetSubmitActions : Sitecore.Forms.Core.Commands.SetSubmitActions
  {
    protected new void Run(ClientPipelineArgs args)
    {
      Assert.ArgumentNotNull(args.Parameters["id"], "id");
      Assert.ArgumentNotNull(args.Parameters["db"], "db");
      ItemUri uri = new ItemUri(ID.Parse(args.Parameters["id"]), Language.Parse(args.Parameters["la"]), Data.Version.Latest, args.Parameters["db"]);
      Item innerItem = Database.GetItem(uri);
      if (SheerResponse.CheckModified() && (innerItem != null))
      {
        if (args.IsPostBack)
        {
          UrlString urlString = new UrlString(args.Parameters["url"]);
          UrlHandle handle = UrlHandle.Get(urlString);
          innerItem.Editing.BeginEdit();
          innerItem.Fields["__Tracking"].Value = handle["tracking"];
          if (args.HasResult)
          {
            ListDefinition definition = ListDefinition.Parse((args.Result == "-") ? string.Empty : args.Result);
            if (args.Parameters["mode"] == "save")
            {
              innerItem.Fields[Sitecore.Form.Core.Configuration.FieldIDs.SaveActionsID].Value = definition.ToXml();
            }
            else
            {
              innerItem.Fields[Sitecore.Form.Core.Configuration.FieldIDs.CheckActionsID].Value = definition.ToXml();
            }
          }
          innerItem.Editing.EndEdit();
        }
        else
        {
          string name = ID.NewID.ToString();
          UrlString str3 = new UrlString(UIUtil.GetUri("control:SubmitCommands.Editor"));
          FormItem item2 = new FormItem(innerItem);
          ListDefinition definition2 = ListDefinition.Parse((args.Parameters["mode"] == "save") ? item2.SaveActions : item2.CheckActions);
          HttpContext.Current.Session.Add(name, definition2);
          str3.Append("definition", name);
          str3.Add("id", args.Parameters["id"]);
          str3.Add("db", args.Parameters["db"]);
          str3.Add("la", args.Parameters["la"]);
          str3.Append("root", args.Parameters["root"]);
          str3.Append("system", args.Parameters["system"] ?? string.Empty);
          args.Parameters.Add("params", name);
          UrlHandle handle2 = new UrlHandle();
          handle2["title"] = DependenciesManager.ResourceManager.Localize((args.Parameters["mode"] == "save") ? "SELECT_SAVE_TITLE" : "SELECT_CHECK_TITLE");
          handle2["desc"] = DependenciesManager.ResourceManager.Localize((args.Parameters["mode"] == "save") ? "SELECT_SAVE_DESC" : "SELECT_CHECK_DESC");
          handle2["actions"] = DependenciesManager.ResourceManager.Localize((args.Parameters["mode"] == "save") ? "SAVE_ACTIONS" : "CHECK_ACTIONS");
          handle2["addedactions"] = DependenciesManager.ResourceManager.Localize((args.Parameters["mode"] == "save") ? "ADDED_SAVE_ACTIONS" : "ADDED_CHECK_ACTIONS");
          handle2["tracking"] = item2.Tracking.ToString();
          handle2.Add(str3);
          args.Parameters["url"] = str3.ToString();
          Context.ClientPage.ClientResponse.ShowModalDialog(str3.ToString(), true);
          args.WaitForPostBack();
        }
      }
    }
  }
}
