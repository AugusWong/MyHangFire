using CommonUtils;
using Hangfire.HttpJob.Support;
using Hangfire.Server;
using log4net;
using MailKit.Net.Smtp;
using MimeKit;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Hangfire.HttpJob.Server
{
    public class HttpJob
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(HttpJob));
        private static MimeMessage mimeMessage;
        public static HangfireHttpJobOptions HangfireHttpJobOptions;

        /// <summary>
        /// 发送邮件
        /// </summary>
        /// <param name="jobName">任务名称</param>
        /// <param name="url">地址</param>
        /// <param name="exception">异常信息</param>
        /// <returns></returns>
        public static bool SendEmail(string jobName, string url, string exception)
        {
            try
            {
                mimeMessage = new MimeMessage();
                mimeMessage.From.Add(MailboxAddress.Parse(HangfireHttpJobOptions.SendMailAddress));
                List<Emails> sendMailList = new List<Emails>();

                HangfireHttpJobOptions.SendToMailList.ForEach(f =>
                {
                    mimeMessage.To.Add(MailboxAddress.Parse(f));
                });

                mimeMessage.Subject = HangfireHttpJobOptions.SMTPSubject;
                var builder = new BodyBuilder
                {
                    HtmlBody = SendHtmlBody(jobName, url, $"执行出错，错误详情:{exception}")
                };

                mimeMessage.Body = builder.ToMessageBody();
                var client = new SmtpClient();
                client.Connect(HangfireHttpJobOptions.SMTPServerAddress, HangfireHttpJobOptions.SMTPPort);
                client.AuthenticationMechanisms.Remove("XOAUTH2");
                client.Authenticate(HangfireHttpJobOptions.SendMailAddress, HangfireHttpJobOptions.SMTPPwd); //验证账号密码
                client.Send(mimeMessage);
                client.Disconnect(true);
            }
            catch (Exception ex)
            {
                log.Error($"邮件服务异常，异常为：{ex}");
                return false;
            }
            return true;
        }

        public static string SendHtmlBody(string jobName, string url, string exception)
        {
            var title = HangfireHttpJobOptions.SMTPSubject;
            var htmlbody = $@"<h3 align='center'>{title}</h3>
                            <h3>执行时间：</h3>
                            <p>
                                {DateTime.Now}
                            </p>
                            <h3>
                                任务名称：<span> {jobName} </span><br/>
                            </h3>
                            <h3>
                                请求路径：{url}
                            </h3>
                            <h3><span></span> 
                                执行结果：<br/>
                            </h3>
                            <p>
                                {exception}
                            </p> ";
            return htmlbody;
        }

        /// <summary>
        /// 执行任务 ，delaysinseconds (重试时间间隔/单位秒)
        /// </summary>
        /// <param name="item"></param>
        /// <param name="jobName"></param>
        /// <param name="queueName"></param>
        /// <param name="isRetry"></param>
        /// <param name="context"></param>
        []
        []
        [DisplayName("Args : [JobName:{1}|QueueName:{2}|IsRetry:{3}]")]
        [JobFilter(timeoutInSeconds: 3600)]
        public static void Excute(HttpJobItem item, string jobName = null, string queueName = null,
            bool isRetry = true, PerformContext context = null)
        {

        }
    }
}
