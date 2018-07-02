using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Http.Filters;
using System.Web.Mvc;
using X_Admin_API.Helper;

namespace X_Admin_API.Utils.Attribute
{
    public class ErrorLoggerAttribute : ExceptionFilterAttribute
    {
        public override void OnException(HttpActionExecutedContext context)
        {
            base.OnException(context);
            LogError(context);
        }
        public void LogError(HttpActionExecutedContext filterContext)
        {
            StringBuilder errorSB = new StringBuilder();
            errorSB
                .AppendLine($"****************** {DateTime.Now.ToString("yyyy-MM-dd HH':'mm':'ss")} *******************")
                .AppendFormat($"Source:\t {filterContext.Exception.Source}")
                .AppendLine()
                .AppendFormat($"Target:\t {filterContext.Exception.TargetSite}")
                .AppendLine()
                .AppendFormat($"Type:\t {filterContext.Exception.GetType().Name}")
                .AppendLine()
                .AppendFormat($"Message:\t {filterContext.Exception.Message}")
                .AppendLine()
                .AppendFormat($"Stack:\t {filterContext.Exception.StackTrace}");
            if(filterContext.Exception.InnerException != null)
            {
                errorSB
                .AppendLine()
                .AppendFormat($"-> ++++++++++ InnerException ++++++++++")
                .AppendLine()
                .AppendFormat($"Source:\t {filterContext.Exception.InnerException.Source}")
                .AppendLine()
                .AppendFormat($"Target:\t {filterContext.Exception.InnerException.TargetSite}")
                .AppendLine()
                .AppendFormat($"Type:\t {filterContext.Exception.InnerException.GetType().Name}")
                .AppendLine()
                .AppendFormat($"Message:\t {filterContext.Exception.InnerException.Message}")
                .AppendLine()
                .AppendFormat($"Stack:\t {filterContext.Exception.InnerException.StackTrace}");
            }
            errorSB
                .AppendLine()
                .AppendLine("======================================================================================================")
                .AppendLine()
                .AppendLine();

            string path = "";
            string year = DateTime.Now.Year.ToString();
            string month = DateTime.Now.Month > 9 ? DateTime.Now.Month.ToString() : "0" + DateTime.Now.Month;
            string day = DateTime.Now.Day > 9 ? DateTime.Now.Day.ToString() : "0" + DateTime.Now.Day;

            path = ConstantHelper.LOG_FOLDER + @"\" + year + @"\" + month;
            path = HttpContext.Current.Server.MapPath(@"~\" + path);
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            var fileName = day + ".log";
            using (StreamWriter writer = File.AppendText(path + @"\" + fileName))
            {
                writer.Write(errorSB.ToString());
                writer.Flush();
            }
        }
    }
}