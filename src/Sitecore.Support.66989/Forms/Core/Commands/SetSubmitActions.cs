using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Form.Core.Configuration;
using Sitecore.Form.Core.ContentEditor.Data;
using Sitecore.Forms.Core.Commands;
using Sitecore.Forms.Core.Data;
using Sitecore.Globalization;
using Sitecore.Text;
using Sitecore.Web;
using Sitecore.Web.UI.Sheer;
using Sitecore.WFFM.Core.Resources;
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
            #region Patch66989
            ItemUri uri = new ItemUri(ID.Parse(args.Parameters["id"]), Language.Parse(args.Parameters["la"]), Sitecore.Data.Version.Latest, args.Parameters["db"]);
            Item item = Database.GetItem(uri);
            if (SheerResponse.CheckModified() && item != null)
            {
                if (args.IsPostBack)
                {
                    UrlString urlString = new UrlString(args.Parameters["url"]);
                    UrlHandle urlHandle = UrlHandle.Get(urlString);
                    item.Editing.BeginEdit();
                    item.Fields["__Tracking"].Value = urlHandle["tracking"];
                    if (args.HasResult)
                    {
                        ListDefinition listDefinition = ListDefinition.Parse((args.Result == "-") ? string.Empty : args.Result);
                        if (args.Parameters["mode"] == "save")
                        {
                            item.Fields[Sitecore.Form.Core.Configuration.FieldIDs.SaveActionsID].Value = listDefinition.ToXml();
                        }
                        else
                        {
                            item.Fields[Sitecore.Form.Core.Configuration.FieldIDs.CheckActionsID].Value = listDefinition.ToXml();
                        }
                    }
                    item.Editing.EndEdit();
                    return;
                }
                string text = ID.NewID.ToString();
                UrlString urlString2 = new UrlString(UIUtil.GetUri("control:SubmitCommands.Editor"));
                FormItem formItem = new FormItem(item);
                ListDefinition listDefinition2 = ListDefinition.Parse((args.Parameters["mode"] == "save") ? formItem.SaveActions : formItem.CheckActions);
                if (listDefinition2.Groups.Count == 0)
                {
                    GroupDefinition item2 = new GroupDefinition
                    {
                        DisplayName = ResourceManager.Localize("SAVE_ACTIONS"),
                        ID = FormIDs.SaveActionsRootID.ToString()
                    };
                    listDefinition2.Groups.Add(item2);
                }
                HttpContext.Current.Session.Add(text, listDefinition2);
                #endregion
                urlString2.Append("definition", text);
                urlString2.Add("id", args.Parameters["id"]);
                urlString2.Add("db", args.Parameters["db"]);
                urlString2.Add("la", args.Parameters["la"]);
                urlString2.Append("root", args.Parameters["root"]);
                urlString2.Append("system", args.Parameters["system"] ?? string.Empty);
                args.Parameters.Add("params", text);
                UrlHandle urlHandle2 = new UrlHandle();
                urlHandle2["title"] = ResourceManager.Localize((args.Parameters["mode"] == "save") ? "SELECT_SAVE_TITLE" : "SELECT_CHECK_TITLE");
                urlHandle2["desc"] = ResourceManager.Localize((args.Parameters["mode"] == "save") ? "SELECT_SAVE_DESC" : "SELECT_CHECK_DESC");
                urlHandle2["actions"] = ResourceManager.Localize((args.Parameters["mode"] == "save") ? "SAVE_ACTIONS" : "CHECK_ACTIONS");
                urlHandle2["addedactions"] = ResourceManager.Localize((args.Parameters["mode"] == "save") ? "ADDED_SAVE_ACTIONS" : "ADDED_CHECK_ACTIONS");
                urlHandle2["tracking"] = formItem.Tracking.ToString();
                urlHandle2.Add(urlString2);
                args.Parameters["url"] = urlString2.ToString();
                Context.ClientPage.ClientResponse.ShowModalDialog(urlString2.ToString(), true);
                args.WaitForPostBack();
            }
        }
    }
}
