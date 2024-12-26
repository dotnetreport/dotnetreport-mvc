﻿using Newtonsoft.Json;
using Quartz;
using Quartz.Impl;
using ReportBuilder.Web.Controllers;
using ReportBuilder.Web.Models;
using System.Net.Mail;

namespace ReportBuilder.Web.Jobs
{
    public class ReportSchedule
    {
        public int Id { get; set; } = 0;
        public string Schedule { get; set; }
        public string EmailTo { get; set; }
        public string LastRun { get; set; }
        public DateTime? NextRun { get; set; }
        public string UserId { get; set; }
        public string Format { get; set; }
        public string DataFilters { get; set; }
        public string TimeZone { get; set; }
    }
    public class ReportWithSchedule
    {
        public int Id { get; set; }
        public int ReportId { get; set; }
        public int DashboardId { get; set; }
        public List<int> DashboardReports { get; set; } = new List<int>();
        public string Name { get; set; }
        public string Description { get; set; }
        public string DataConnectName { get; set; }
        public List<ReportSchedule> Schedules { get; set; }

    }

    public class JobScheduler
    {
        public static string WebAppRootUrl = "";
        public static async void Start()
        {
            var schedulerFactory = new StdSchedulerFactory();
            var scheduler = await schedulerFactory.GetScheduler();
            await scheduler.Start();

            IJobDetail job = JobBuilder.Create<DotNetReportJob>()
                                       .WithIdentity("DotNetReportJob")
                                       .StoreDurably()
                                       .Build();

            ITrigger trigger = TriggerBuilder.Create()
                .WithIdentity("DotNetReportJobTrigger")
                .StartNow()
                .WithSimpleSchedule(s => s.WithIntervalInSeconds(60).RepeatForever())
                .Build();

            await scheduler.ScheduleJob(job, trigger);

        }
    }

    public class DotNetReportJob : IJob
    {
        private readonly IConfigurationRoot _configuration;
        public readonly static string _configFileName = "appsettings.dotnetreport.json";

        public DotNetReportJob()
        {
            var builder = new ConfigurationBuilder()
           .SetBasePath(Directory.GetCurrentDirectory())
           .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            _configuration = builder.Build();
        }
        async Task IJob.Execute(IJobExecutionContext context)
        {
            var apiUrl = _configuration.GetValue<string>("dotNetReport:apiUrl");
            var accountApiKey = _configuration.GetValue<string>("dotNetReport:accountApiToken");
            var databaseApiKey = _configuration.GetValue<string>("dotNetReport:dataconnectApiToken");

            var appsetting = DotNetReportHelper.GetAppSettings();
            var fromEmail = appsetting.emailAddress;
            var fromName = appsetting.emailName;
            var mailServer = appsetting.emailServer;
            var mailUserName =appsetting.emailUserName;
            var mailPassword = appsetting.emailPassword;
            var emailport = appsetting.emailPort;

            var clientId = ""; // you can specify client id here if needed

            // Get all reports with schedule and run the ones that are due
            using (var client = new HttpClient())
            {
                var response = await client.GetAsync($"{apiUrl}/ReportApi/GetScheduledReportsAndDashboards?account={accountApiKey}&dataConnect={databaseApiKey}&clientId={clientId}");

                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var reports = JsonConvert.DeserializeObject<List<ReportWithSchedule>>(content);

                foreach (var report in reports)
                {
                    foreach (var schedule in report.Schedules)
                    {
                        try
                        {
                            var chron = new CronExpression(schedule.Schedule);
                            var lastRun = !String.IsNullOrEmpty(schedule.LastRun) ? Convert.ToDateTime(schedule.LastRun) : DateTimeOffset.UtcNow.AddMinutes(-10);
                            var nextRun = chron.GetTimeAfter(lastRun);

                            if (!String.IsNullOrEmpty(schedule.TimeZone))
                            {
                                TimeZoneInfo timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(schedule.TimeZone);
                                // Convert last run to user's local time zone
                                lastRun = TimeZoneInfo.ConvertTime(lastRun, timeZoneInfo);
                                nextRun = chron.GetTimeAfter(lastRun);
                                // Get current time in user's time zone
                                DateTime currentTime = TimeZoneInfo.ConvertTime(DateTime.UtcNow, timeZoneInfo);
                            }
                            schedule.NextRun = (nextRun.HasValue ? nextRun.Value.ToLocalTime().DateTime : (DateTime?)null);

                            if (schedule.NextRun.HasValue && DateTime.Now >= schedule.NextRun && (!String.IsNullOrEmpty(schedule.LastRun) || lastRun <= schedule.NextRun))
                            {
                                var isDashboard = report.DashboardId > 0;
                                var itemId = isDashboard ? report.DashboardId : report.ReportId;

                                response = await client.GetAsync($"{apiUrl}/ReportApi/RunScheduledItem?account={accountApiKey}&dataConnect={databaseApiKey}&scheduleId={schedule.Id}&id={itemId}&localRunTime={schedule.NextRun.Value:yyyy-MM-ddTHH:mm:ss}&isDashboard={isDashboard}&clientId={clientId}&dataFilters={schedule.DataFilters}");
                                response.EnsureSuccessStatusCode();

                                content = await response.Content.ReadAsStringAsync();

                                DotNetReportScheduleModel reportToRun = null;
                                List<DotNetReportScheduleModel> reportsToRun = null;

                                if (isDashboard)
                                {
                                    reportsToRun = JsonConvert.DeserializeObject<List<DotNetReportScheduleModel>>(content);
                                }
                                else
                                {
                                    reportToRun = JsonConvert.DeserializeObject<DotNetReportScheduleModel>(content);
                                }

                                var files = new List<byte[]>();
                                byte[] fileData;
                                string fileExt = "";
                                string imageData = "";
                                switch ((schedule.Format ?? "Excel").ToUpper())
                                {
                                    case "PDF":
                                        if (report.DashboardId > 0)
                                        {
                                            foreach (var r in reportsToRun)
                                            {
                                                fileData = await DotNetReportHelper.GetPdfFile(JobScheduler.WebAppRootUrl + "/Report/ReportPrint", r.ReportId, r.ReportSql, r.ConnectKey, r.ReportName, schedule.UserId, clientId, JsonConvert.SerializeObject(schedule.DataFilters));

                                                files.Add(fileData);
                                            }

                                            fileData = DotNetReportHelper.GetCombinePdfFile(files);
                                        }
                                        else
                                        {
                                            fileData = await DotNetReportHelper.GetPdfFile(JobScheduler.WebAppRootUrl + "/Report/ReportPrint", reportToRun.ReportId, reportToRun.ReportSql, reportToRun.ConnectKey, reportToRun.ReportName, schedule.UserId, clientId, JsonConvert.SerializeObject(schedule.DataFilters));
                                        }
                                        fileExt = ".pdf"; 
                                        break;

                                    case "CSV": 
                                        fileExt = ".csv";
                                        fileData = await DotNetReportHelper.GetCSVFile(reportToRun.ReportSql, reportToRun.ConnectKey, reportToRun.Columns, reportToRun.IncludeSubTotals);
                                        break;

                                    case "WORD":
                                        if (report.DashboardId > 0)
                                        {
                                            foreach (var r in reportsToRun)
                                            {
                                                imageData = await DotNetReportHelper.GetChartImage(JobScheduler.WebAppRootUrl + "/Report/ReportPrint", r.ReportId, r.ConnectKey, r.ReportSql);
                                                fileData = await DotNetReportHelper.GetWordFile(r.ReportSql, r.ConnectKey, r.ReportName, columns: r.Columns, includeSubtotal: r.IncludeSubTotals, pivot: r.ReportType == "Pivot", chartData: imageData);
                                                files.Add(fileData);
                                            }

                                            fileData = DotNetReportHelper.GetCombineWordFile(files);
                                        }
                                        else
                                        {
                                            fileExt = ".docx";
                                            imageData = await DotNetReportHelper.GetChartImage(JobScheduler.WebAppRootUrl + "/Report/ReportPrint", reportToRun.ReportId, reportToRun.ConnectKey, reportToRun.ReportSql);
                                            fileData = await DotNetReportHelper.GetWordFile(reportToRun.ReportSql, reportToRun.ConnectKey, reportToRun.ReportName, columns: reportToRun.Columns, includeSubtotal: reportToRun.IncludeSubTotals, pivot: reportToRun.ReportType == "Pivot", chartData: imageData);
                                        }
                                        break;

                                    case "EXCEL-SUB":
                                        fileData = await DotNetReportHelper.GetExcelFile(reportToRun.ReportSql, reportToRun.ConnectKey, reportToRun.ReportName, columns: reportToRun.Columns, allExpanded: true, expandSqls: reportToRun.ReportData, includeSubtotal: reportToRun.IncludeSubTotals, pivot: reportToRun.ReportType == "Pivot");
                                        fileExt = ".xlsx";
                                        break;
                                    
                                    case "EXCEL":
                                    default:
                                        if (report.DashboardId > 0)
                                        {
                                            foreach (var r in reportsToRun)
                                            {
                                                imageData = await DotNetReportHelper.GetChartImage(JobScheduler.WebAppRootUrl + "/Report/ReportPrint", r.ReportId, r.ConnectKey, r.ReportSql);
                                                fileData = await DotNetReportHelper.GetExcelFile(r.ReportSql, r.ConnectKey, r.ReportName, columns: r.Columns, includeSubtotal: r.IncludeSubTotals, pivot: r.ReportType == "Pivot", chartData: imageData);
                                                files.Add(fileData);
                                            }

                                            fileData = DotNetReportHelper.GetCombineExcelFile(files, reportsToRun.Select(r => r.ReportName).ToList());
                                        }
                                        else
                                        {
                                            imageData = await DotNetReportHelper.GetChartImage(JobScheduler.WebAppRootUrl + "/Report/ReportPrint", reportToRun.ReportId, reportToRun.ConnectKey, reportToRun.ReportSql);
                                            fileData = await DotNetReportHelper.GetExcelFile(reportToRun.ReportSql, reportToRun.ConnectKey, reportToRun.ReportName, columns: reportToRun.Columns, includeSubtotal: reportToRun.IncludeSubTotals, pivot: reportToRun.ReportType == "Pivot", chartData: imageData);
                                            fileExt = ".xlsx";
                                        }
                                        break;
                                }

                                // send email
                                var mail = new MailMessage
                                {
                                    From = new MailAddress(fromEmail, fromName),
                                    Subject = report.Name,
                                    Body = $"Your scheduled report is attached.<br><br>{report.Description}",
                                    IsBodyHtml = true
                                };
                                mail.To.Add(schedule.EmailTo);


                                if (schedule.Format == "Link")
                                {
                                    mail.Body = $"Please click on the link below to Run your Report:<br><br><a href=\"{JobScheduler.WebAppRootUrl}/DotnetReport/Report?linkedreport=true&noparent=true&reportId={reportToRun.ReportId}\">{report.Description}</a>";
                                }
                                else
                                {
                                    var attachment = new Attachment(new MemoryStream(fileData), report.Name + fileExt);
                                    mail.Attachments.Add(attachment);
                                }

                                using (var smtpServer = new SmtpClient(mailServer))
                                {
                                    smtpServer.Port = Convert.ToInt32(emailport);// 587
                                    smtpServer.Credentials = new System.Net.NetworkCredential(mailUserName, mailPassword);
                                    //smtpServer.EnableSsl = true;
                                    smtpServer.Send(mail);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            // could not run, ignore error
                        }
                    }
                }
            }
        }
    }
}