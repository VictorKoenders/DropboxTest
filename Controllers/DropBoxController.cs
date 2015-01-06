using System.Configuration;
using System.Linq;
using System.Web.Mvc;
using DropNet;
using DropNet.Models;
using Utils;

namespace DropboxProjects.Controllers
{
    public class DropBoxController : Controller
	{

		private static string ConsumerKey => ConfigurationManager.AppSettings["DropboxKey"];
		private static string ConsumerSecret => ConfigurationManager.AppSettings["DropboxSecret"];

		private readonly DropNetClient DropNetClient = new DropNetClient(ConsumerKey, ConsumerSecret);
        //
        // GET: /DropBox/
        public ActionResult Index()
		{
			if (Request["not_approved"] == "true")
			{
				return View("NoConnection");
			}
			if (SessionHelper.DropNetAccessToken != null)
			{
				return View();
			}
			if (SessionHelper.DropNetUserLogin != null)
			{
				try
				{
					DropNetClient.UserLogin = SessionHelper.DropNetUserLogin;
					SessionHelper.DropNetAccessToken = DropNetClient.GetAccessToken();

					return View();
				}
				catch
				{
				}
			}
			else
			{
				SessionHelper.DropNetUserLogin = DropNetClient.GetToken();
			}
			var url = DropNetClient.BuildAuthorizeUrl(Request.Url.ToString());

			return Redirect(url);
        }

	    public ActionResult IsAuthed()
	    {
		    return Json(SessionHelper.DropNetAccessToken != null, JsonRequestBehavior.AllowGet);
	    }

		public ActionResult Load(string folder = "/")
		{
			MetaData metaData;
			try
			{
				DropNetClient.UserLogin = SessionHelper.DropNetAccessToken;
				metaData = DropNetClient.GetMetaData(folder);
			}
			catch
			{
				SessionHelper.DropNetAccessToken = null;
				SessionHelper.DropNetUserLogin = null;
				return Json(new { Error = true }, JsonRequestBehavior.AllowGet);
			}

			dynamic result = new
			{
				Folders = metaData.Contents.Where(content => content.Is_Dir).Select(content => content.Name),
				Files = metaData.Contents.Where(content => !content.Is_Dir).Select(content => content.Name)
			};

			return Json(result, JsonRequestBehavior.AllowGet);
		}
	}
}