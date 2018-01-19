using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Services;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class upload : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {       
        var length = Request.ContentLength;
        var bytes = new byte[length];
        Request.InputStream.Read(bytes, 0, length);

        var fileName = generateName() + Request.Headers["X-File-Name"].Replace(' ', '_');
        var saveToFileLoc = string.Format("{0}\\{1}",
                                       Server.MapPath("/files"),
                                       fileName);

        // save the file.
        var fileStream = new FileStream(saveToFileLoc, FileMode.Create, FileAccess.ReadWrite);
        fileStream.Write(bytes, 0, length);
        fileStream.Close();                      

        Response.AddHeader("filename", fileName);
    }

    /// <summary>
    /// Генерация имени файла из 30 символов
    /// </summary>
    /// <returns></returns>
    private string generateName()
    {
        string val = "";
        Random rnd = new Random();
        for (int i = 0; i < 30; i++)
        {
            switch (rnd.Next(3))
            {
                case 0:
                    val += (char)rnd.Next(97, 123);
                    break;
                case 1:
                    val += (char)rnd.Next(65, 91);
                    break;
                case 2:
                    val += (char)rnd.Next(48, 58);
                    break;
            }

        }
        return val+"_";
    }

    [WebMethod()]
    public static void remFile(string filename)
    {

        filename = filename.Replace("_tchk_", ".");
        string path = HttpContext.Current.Server.MapPath("/files") + "/" + filename;
        File.Delete(path);
    }

}