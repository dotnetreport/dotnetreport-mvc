﻿using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OfficeOpenXml;
using PdfSharp;
using PdfSharp.Drawing;
using PdfSharp.Drawing.Layout;
using PdfSharp.Pdf;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.OleDb;
using System.Drawing;
using System.Net;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;
using A = DocumentFormat.OpenXml.Drawing;
using DW = DocumentFormat.OpenXml.Drawing.Wordprocessing;
using PIC = DocumentFormat.OpenXml.Drawing.Pictures;
using System.Data.SqlClient;

namespace ReportBuilder.Web.Models
{
    public class DotNetReportModel
    {
        public int ReportId { get; set; }
        public string ReportName { get; set; }
        public string ReportDescription { get; set; }
        public string ReportSql { get; set; }

        public bool IncludeSubTotals { get; set; }
        public bool ShowUniqueRecords { get; set; }
        public string ReportFilter { get; set; }
        public string ReportType { get; set; }
        public bool ShowDataWithGraph { get; set; }

        public string ConnectKey { get; set; }

        public string ChartData { get; set; }
        public bool IsDashboard { get; set; }
        public int SelectedFolder { get; set; }

        public string ReportSeries { get; set; }
        public bool ExpandAll { get; set; }
        public string ReportData { get; set; }
    }

    public class DotNetReportPrintModel : DotNetReportModel
    {
        public string ClientId { get; set; }
        public string UserId { get; set; }
        public string CurrentUserRoles { get; set; }
        public string DataFilters { get; set; }
        public string ReportData { get; set; }
    }

    public class DotNetReportResultModel
    {
        public string ReportSql { get; set; }
        public DotNetReportDataModel ReportData { get; set; }
        public DotNetReportPagerModel Pager { get; set; }

        public bool HasError { get; set; }
        public string Exception { get; set; }
        public string Warnings { get; set; }

        public bool ReportDebug { get; set; }
    }

    public class DotNetReportPagerModel
    {
        public int CurrentPage { get; set; }
        public int TotalRecords { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

    public class DotNetReportDataColumnModel
    {
        public string SqlField { get; set; }
        public string ColumnName { get; set; }
        public string DataType { get; set; }
        public bool IsNumeric { get; set; }
        public string FormatType { get; set; }
    }

    public class DotNetReportDataRowItemModel
    {
        public string Value { get; set; }
        public string FormattedValue { get; set; }
        public string LabelValue { get; set; }
        public double? NumericValue { get; set; }
        public DotNetReportDataColumnModel Column { get; set; }
    }

    public class DotNetReportDataRowModel
    {
        public DotNetReportDataRowItemModel[] Items { get; set; }
    }

    public class DotNetReportDataModel
    {
        public List<DotNetReportDataRowModel> Rows { get; set; }
        public List<DotNetReportDataColumnModel> Columns { get; set; }
    }

    public class TableViewModel
    {
        public int Id { get; set; }
        public string SchemaName { get; set; }
        public string TableName { get; set; }
        public string DisplayName { get; set; }
        public bool Selected { get; set; }
        public bool IsView { get; set; }
        public int DisplayOrder { get; set; }
        public string AccountIdField { get; set; }

        public List<ColumnViewModel> Columns { get; set; }

        public DataTable dataTable { get; set; }
        public List<ParameterViewModel> Parameters { get; set; }
        public List<string> AllowedRoles { get; set; }
        public bool? DoNotDisplay { get; set; }

        public bool CustomTable { get; set; }
        public string CustomTableSql { get; set; }
    }

    public class ParameterViewModel
    {
        public int Id { get; set; }
        public string ParameterName { get; set; }
        public string DisplayName { get; set; }
        public string ParameterValue { get; set; }
        public string ParameterDataTypeString { get; set; }
        public Type ParameterDataTypeCLR { get; set; }
        public OleDbType ParamterDataTypeOleDbType { get; set; }
        public int ParamterDataTypeOleDbTypeInteger { get; set; }
        public bool Required { get; set; }
        public bool ForeignKey { get; set; }
        public string ForeignTable { get; set; }
        public string ForeignJoin { get; set; }
        public string ForeignKeyField { get; set; }
        public string ForeignValueField { get; set; }
        public bool Hidden { get; set; }
    }

    public class RelationModel
    {
        public int Id { get; set; }
        public int TableId { get; set; }
        public int JoinedTableId { get; set; }
        public string JoinType { get; set; }
        public string FieldName { get; set; }
        public string JoinFieldName { get; set; }
    }


    public class CustomFunctionModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string DisplayName { get; set; } = "";
        public string Description { get; set; } = "";
        public int DataConnectionId { get; set; } 
        public int DisplayOrder { get; set; } 

        public string FunctionType { get; set; } = "";
        public string References { get; set; } = "";
        public string ResultDataType { get; set; } = "";
        public string Code { get; set; } = "";
        public List<CustomFunctionParameterModel> Parameters { get; set; } = new List<CustomFunctionParameterModel>();
        public List<string> AllowedRoles { get; set; } = new List<string>();
    }

    public class CustomFunctionParameterModel
    {
        public int Id { get; set; }
        public int CustomFunctionId { get; set; }
        public string ParameterName { get; set; } = "";
        public string DisplayName { get; set; } = "";
  
        public string Description { get; set; } = "";

        public bool Required { get; set; }
        public string DefaultValue { get; set; } = "";
    }


    public enum FieldTypes
    {
        Boolean,
        DateTime,
        Varchar,
        Money,
        Int,
        Double
    }

    public enum JoinTypes
    {
        Inner,
        Left,
        Right
    }

    public enum DbTypes
    {
        MS_SQL,
        MySql,
        Postgre_Sql
    }

    public class ColumnViewModel
    {
        public int Id { get; set; }
        public string ColumnName { get; set; }
        public string DisplayName { get; set; }
        public bool Selected { get; set; }
        public int DisplayOrder { get; set; }
        public string FieldType { get; set; }
        public bool PrimaryKey { get; set; }
        public bool ForeignKey { get; set; }
        public bool AccountIdField { get; set; }
        public string ForeignTable { get; set; }
        public string ForeignJoin { get; set; }
        public string ForeignKeyField { get; set; }
        public string ForeignValueField { get; set; }
        public bool DoNotDisplay { get; set; }
        public bool ForceFilter { get; set; }
        public bool ForceFilterForTable { get; set; }
        public string RestrictedDateRange { get; set; }
        public DateTime? RestrictedStartDate { get; set; }
        public DateTime? RestrictedEndDate { get; set; }
        public List<string> AllowedRoles { get; set; }
        public bool ForeignParentKey { get; set; }
        public string ForeignParentTable { get; set; }
        public string ForeignParentApplyTo { get; set; }
        public string ForeignParentKeyField { get; set; }
        public string ForeignParentValueField { get; set; }
        public bool ForeignParentRequired { get; set; }
        public string JsonStructure { get; set; }
    }

    public class ConnectViewModel
    {
        public string Provider { get; set; }
        public string ServerName { get; set; }
        public string InitialCatalog { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public bool IntegratedSecurity { get; set; }
        public string ApiUrl { get; set; }
        public string AccountApiKey { get; set; }
        public string DatabaseApiKey { get; set; }
    }

    public class ManageViewModel
    {
        public string ApiUrl { get; set; }
        public string AccountApiKey { get; set; }
        public string DatabaseApiKey { get; set; }

        public List<TableViewModel> Tables { get; set; }
        public List<TableViewModel> Procedures { get; set; }
        public List<CustomFunctionModel> Functions { get; set; }

        public dynamic DbConfig { get; set; }
        public UserRolesConfig UserAndRolesConfig { get; set; }

    }
    public class UserRolesConfig
    {
        public bool RequireLogin { get; set; }
        public bool UsersSource { get; set; }
        public bool UserRolesSource { get; set; }
        public string SelectedUserConfig { get; set; }
    }
    public class DotNetReportApiCall
    {
        public string Method { get; set; }
        public bool SaveReport { get; set; }
        public string ReportJson { get; set; }
        public bool adminMode { get; set; }
        public bool SubTotalMode { get; set; }
    }

    public class DotNetDasboardReportModel : DotNetReportModel
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public bool IsWidget { get; set; }
        public string WidgetSettings { get; set; }    
    }

    public class DotNetDashboardModel
    {
        public List<dynamic> Dashboards { get; set; }
        public List<DotNetDasboardReportModel> Reports { get; set; }
    }

    public class DotNetReportSettings
    {
        /// <summary>
        /// dotnet Report Service Api Url
        /// </summary>
        public string ApiUrl { get; set; }

        /// <summary>
        /// Your dotnet Report Account Key
        /// </summary>
        public string AccountApiToken { get; set; }

        /// <summary>
        /// Your dotnet Report Data Connection Key
        /// </summary>
        public string DataConnectApiToken { get; set; }

        /// <summary>
        /// Current Client Id if using Multi-tenant
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        /// Current User Id if using Authentication
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// If you want to use user id for schedule but not for authentication, use this property
        /// </summary>
        public string UserIdForSchedule { get; set; }


        /// <summary>
        /// If you want to use user id for filter but not for authentication, use this property
        /// </summary>
        public string UserIdForFilter { get; set; }

        /// <summary>
        /// Current User name to display
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// List of Current User's Roles if using Authentication
        /// </summary>
        public List<string> CurrentUserRole { get; set; } = new List<string>();

        /// <summary>
        /// List of all User Ids in your Application
        /// </summary>
        public List<dynamic> Users { get; set; } = new List<dynamic>();

        /// <summary>
        /// List of all User Roles in your Application
        /// </summary>
        public List<string> UserRoles { get; set; } = new List<string>();

        /// <summary>
        /// A list of Global Data filters using format { Column1: 'val1, val2, ...', Column2: '1,2,3,...', ...}
        /// </summary>
        public dynamic DataFilters { get; set; }

        /// <summary>
        /// Set true if the current user can enter Admin Mode
        /// </summary>
        public bool CanUseAdminMode { get; set; }
    }

    public class ReportHeaderColumn
    {
        public string fieldName { get; set; }
        public string fieldLabel { get; set; }
        public string customfieldLabel { get; set; }
        public bool hideStoredProcColumn { get; set; }
        public int? decimalPlacesDigit { get; set; }
        public string fieldAlign { get; set; }
        public string fieldFormating { get; set; }
        public bool dontSubTotal { get; set; }
        public string currencySymbol { get; set; }
        public bool isNumeric { get; set; }
        public bool isCurrency { get; set; }
        public bool isJsonColumn { get; set; }
    }

    public interface IDnrDataConnection
    {
        string DbConnection { get; set; }
        string ConnectKey { get; set; }

        DataTable ExecuteSql(string sql);
    }

    public class MsSqlDnrDataConnection : IDnrDataConnection
    {
        public string DbConnection { get; set; }
        public string ConnectKey { get; set; }

        public MsSqlDnrDataConnection(string connectKey)
        {
            //DbConnection = dbConnection;
            ConnectKey = connectKey;
        }

        public DataTable ExecuteSql(string sql)
        {
            var dt = new DataTable();
            using (var conn = new SqlConnection(DbConnection))
            {
                conn.Open();
                var command = new SqlCommand(sql, conn);
                var adapter = new SqlDataAdapter(command);

                adapter.Fill(dt);
            }

            return dt;
        }
    }


    public static class DotNetReportHelper
    {
        private readonly static string _configFileName = "appsettings.dotnetreport.json";

        public static string GetConnectionString(string key, bool addOledbProvider = true)
        {
            var connString = ConfigurationManager.ConnectionStrings[key].ConnectionString;
            if (connString == null)
            {
                return "";
            }

            connString = connString.Replace("Trusted_Connection=True", "");

            if (!connString.ToLower().StartsWith("provider") && addOledbProvider)
            {
                connString = "Provider=sqloledb;" + connString;
            }

            return connString;
        }

        public static ConnectViewModel GetConnection(string databaseApiKey = "")
        {
            return new ConnectViewModel
            {
                ApiUrl = ConfigurationManager.AppSettings["dotNetReport.apiUrl"],
                AccountApiKey = ConfigurationManager.AppSettings["dotNetReport.accountApiToken"], // Your Account Api Token from your http://dotnetreport.com Account
                DatabaseApiKey = string.IsNullOrEmpty(databaseApiKey) ? ConfigurationManager.AppSettings["dotNetReport.dataconnectApiToken"] : databaseApiKey
            };
        }

        public static async Task<string> GetConnectionString(ConnectViewModel connect, bool addOledbProvider = true)
        {
            if (connect.AccountApiKey == "Your Account API Key" || string.IsNullOrEmpty(connect.AccountApiKey) || string.IsNullOrEmpty(connect.DatabaseApiKey))
                return "";

            using (var client = new HttpClient())
            {
                var response = await client.GetAsync(String.Format("{0}/ReportApi/GetDataConnectKey?account={1}&dataConnect={2}", connect.ApiUrl, connect.AccountApiKey, connect.DatabaseApiKey));

                if (!response.IsSuccessStatusCode)
                {
                    return "";
                }

                var content = await response.Content.ReadAsStringAsync();
                return DotNetReportHelper.GetConnectionString(content.Replace("\"", ""), addOledbProvider);
            }

        }

        public static bool IsNumericType(Type type)
        {

            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.UInt32:
                case TypeCode.Int64:
                case TypeCode.UInt64:
                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Decimal:
                    return true;

                case TypeCode.Boolean:
                case TypeCode.DateTime:
                case TypeCode.String:
                default:
                    return false;
            }
        }

        public static string GetLabelValue(DataColumn col, DataRow row)
        {
            if (@row[col] != null && row[col] != DBNull.Value)
            {
                switch (Type.GetTypeCode(col.DataType))
                {
                    case TypeCode.Int16:
                    case TypeCode.UInt16:
                    case TypeCode.Int32:
                    case TypeCode.UInt32:
                    case TypeCode.Int64:
                    case TypeCode.UInt64:
                    case TypeCode.Single:
                        return row[col].ToString();

                    case TypeCode.Double:
                    case TypeCode.Decimal:
                        return @row[col].ToString();// "'" + (Convert.ToDouble(@row[col].ToString()).ToString("C")) + "'";

                    case TypeCode.Boolean:
                        return (Convert.ToBoolean(@row[col]) ? "Yes" : "No");

                    case TypeCode.DateTime:
                        try
                        {
                            return "'" + @Convert.ToDateTime(@row[col]).ToShortDateString() + "'";
                        }
                        catch
                        {
                            return "'" + @row[col] + "'";
                        }

                    case TypeCode.String:
                    default:
                        return "'" + @row[col].ToString().Replace("'", "") + "'";
                }
            }

            return "";
        }


        static string ParseJsonValue(JToken json, string columnToExtract, bool asTable = true)
        {
            if (!string.IsNullOrEmpty(columnToExtract))
                return json.Value<dynamic>(columnToExtract)?.ToString();

            StringBuilder sb = new StringBuilder();
            ParseJson(json, sb, "", asTable);

            return asTable ? $"<table>{sb}</table>" : sb.ToString();
        }

        static void ParseJson(JToken token, StringBuilder sb, string prefix, bool asTable = true)
        {
            if (token.Type == JTokenType.Object)
            {
                foreach (JProperty prop in token.Children<JProperty>())
                {
                    string propName = prop.Name;
                    if (!string.IsNullOrEmpty(prefix))
                    {
                        propName = prefix + " > " + propName;
                    }

                    ParseJson(prop.Value, sb, propName, asTable);
                }
            }
            else if (token.Type == JTokenType.Array)
            {
                int index = 0;
                foreach (JToken child in token.Children())
                {
                    ParseJson(child, sb, prefix + " - " + index + 1, asTable);
                    index++;
                }
            }
            else
            {
                string value = token.ToString();
                if (!string.IsNullOrEmpty(prefix))
                {
                    if (asTable)
                        sb.AppendFormat($"<tr><td>{prefix}</td><td>{value}</td></tr>\n");
                    else
                        sb.AppendFormat($"{prefix} is {value}\n");
                }
            }
        }

        static bool IsValidJson(string json)
        {
            try
            {
                JToken.Parse(json);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public static bool IsNumeric(string value)
        {
            double result;
            return double.TryParse(value, out result);
        }


        public static string GetFormattedValue(DataColumn col, DataRow row, string formatType, bool jsonAsTable = true)
        {
            if (row[col] != null && row[col] != DBNull.Value && !string.IsNullOrEmpty(row[col].ToString()))
            {
                var val = row[col].ToString().Trim();
                if ((val.StartsWith("[") || val.StartsWith("{")) && IsValidJson(val) && !IsNumeric(val))
                {
                    JToken json = JToken.Parse(val);
                    return ParseJsonValue(json, formatType == "Json" ? col.ColumnName : "", jsonAsTable);
                }

                switch (Type.GetTypeCode(col.DataType))
                {
                    case TypeCode.Int16:
                    case TypeCode.UInt16:
                    case TypeCode.Int32:
                    case TypeCode.UInt32:
                    case TypeCode.Int64:
                    case TypeCode.UInt64:
                    case TypeCode.Single:
                        return row[col].ToString();


                    case TypeCode.Double:
                    case TypeCode.Decimal:
                        return col.ColumnName.Contains("%")
                            ? (Convert.ToDouble(row[col].ToString()) / 100).ToString("P2")
                            : Convert.ToDouble(row[col].ToString()).ToString("C");


                    case TypeCode.Boolean:
                        return (Convert.ToBoolean(row[col]) ? "Yes" : "No");


                    case TypeCode.DateTime:
                        try
                        {
                            return Convert.ToDateTime(row[col]).ToShortDateString();
                        }
                        catch
                        {
                            return row[col] != null ? row[col].ToString() : null;
                        }

                    case TypeCode.String:
                    default:
                        if (row[col].ToString() == "System.Byte[]")
                        {

                            return "<img src=\"data:image/png;base64," + Convert.ToBase64String((byte[])row[col], 0, ((byte[])row[col]).Length) + "\" style=\"max-width: 200px;\" />";
                        }
                        else
                        {
                            return row[col].ToString();
                        }

                }
            }
            return "";
        }

        private static void FormatExcelSheet(DataTable dt, ExcelWorksheet ws, int rowstart, int colstart, List<ReportHeaderColumn> columns = null, bool includeSubtotal = false, bool loadHeader = true, string chartData = null)
        {
            ws.Cells[rowstart, colstart].LoadFromDataTable(dt, loadHeader);
            if (loadHeader) ws.Cells[rowstart, colstart, rowstart, colstart + dt.Columns.Count -1].Style.Font.Bold = true;
            if (!string.IsNullOrEmpty(chartData) && chartData != "undefined")
            {
                byte[] imageBytes = Convert.FromBase64String(chartData.Substring(chartData.LastIndexOf(',') + 1));
                using (MemoryStream ms = new MemoryStream(imageBytes))
                {
                    Image image = Image.FromStream(ms);
                    // Add the image to the worksheet
                    var picture = ws.Drawings.AddPicture("ChartImage", image);
                    picture.SetPosition(1, 0, dt.Columns.Count + 1, 0); // Set the position of the image
                    picture.SetSize(400, 300); // Set the size of the image in pixels (width, height)
                }
            }
            int i = colstart; var isNumeric = false;
            foreach (DataColumn dc in dt.Columns)
            {
                var formatColumn = columns?.FirstOrDefault(x => dc.ColumnName.StartsWith(x.fieldName)) ?? new ReportHeaderColumn();
                string decimalFormat = new string('0', formatColumn.decimalPlacesDigit.GetValueOrDefault());
                isNumeric = dc.DataType.Name.StartsWith("Int") || dc.DataType.Name == "Double" || dc.DataType.Name == "Decimal";
                if (dc.DataType == typeof(decimal) || (formatColumn != null && formatColumn.fieldFormating=="Decimal"))
                {
                    if (formatColumn != null && formatColumn.decimalPlacesDigit != null)
                    {
                        ws.Column(i).Style.Numberformat.Format = "###,###,##0." + decimalFormat;
                    }
                    else
                    {
                        ws.Column(i).Style.Numberformat.Format = "###,###,##0.00";
                    }
                    isNumeric = true;
                }
                if (dc.DataType == typeof(DateTime))
                    ws.Column(i).Style.Numberformat.Format = "mm/dd/yyyy";

                if (formatColumn != null && formatColumn.fieldFormating == "Currency")
                {
                    if (formatColumn.currencySymbol != null && formatColumn.decimalPlacesDigit != null)
                    {
                        ws.Column(i).Style.Numberformat.Format = formatColumn.currencySymbol + "###,###,##0." + decimalFormat;
                    }
                    else if (formatColumn.currencySymbol != null)
                    {
                        ws.Column(i).Style.Numberformat.Format = formatColumn.currencySymbol + "###,###,##0.00";
                    }
                    else
                    {
                        ws.Column(i).Style.Numberformat.Format = "$###,###,##0.00";
                    }
                    isNumeric = true;
                }
                if (formatColumn != null && formatColumn.isJsonColumn)
                {
                    ws.Column(i).Style.Numberformat.Format = "@";
                    ws.Column(i).Style.WrapText = true;
                }
                if (formatColumn != null)
                {
                    ws.Column(i).Style.HorizontalAlignment = formatColumn.fieldAlign == "Right" || (isNumeric && (formatColumn.fieldAlign == "Auto" || string.IsNullOrEmpty(formatColumn.fieldAlign))) ? OfficeOpenXml.Style.ExcelHorizontalAlignment.Right : formatColumn.fieldAlign == "Center" ? OfficeOpenXml.Style.ExcelHorizontalAlignment.Center : OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                }

                if (includeSubtotal)
                {
                    if (isNumeric && !(formatColumn?.dontSubTotal ?? false))
                    {
                        ws.Cells[dt.Rows.Count + rowstart + 1, i].Formula = $"=SUM({ws.Cells[rowstart, i].Address}:{ws.Cells[dt.Rows.Count + rowstart, i].Address})";
                        ws.Cells[dt.Rows.Count + rowstart + 1, i].Style.Font.Bold = true;
                    }
                }

                i++;
            }

            ws.Cells[ws.Dimension.Address].AutoFitColumns();
        }

        public static DataTable Transpose(DataTable dt)
        {
            DataTable dtNew = new DataTable();

            for (int i = 0; i <= dt.Rows.Count; i++)
            {
                dtNew.Columns.Add(i.ToString());
            }

            dtNew.Columns[0].ColumnName = " ";

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                dtNew.Columns[i + 1].ColumnName = dt.Rows[i].ItemArray[0].ToString();
            }

            for (int k = 1; k < dt.Columns.Count; k++)
            {
                DataRow r = dtNew.NewRow();
                r[0] = dt.Columns[k].ToString();
                for (int j = 1; j <= dt.Rows.Count; j++)
                    r[j] = dt.Rows[j - 1][k];
                dtNew.Rows.Add(r);
            }

            return dtNew;
        }

        public static string RunReportApiCall(string postData)
        {
            using (var client = new HttpClient())
            {
                var settings = new DotNetReportSettings
                {
                    ApiUrl = ConfigurationManager.AppSettings["dotNetReport.apiUrl"],
                    AccountApiToken = ConfigurationManager.AppSettings["dotNetReport.accountApiToken"], // Your Account Api Token from your http://dotnetreport.com Account
                    DataConnectApiToken = ConfigurationManager.AppSettings["dotNetReport.dataconnectApiToken"] // Your Data Connect Api Token from your http://dotnetreport.com Account
                };
                var keyvalues = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("Account", settings.AccountApiToken),
                    new KeyValuePair<string, string>("DataConnect", settings.DataConnectApiToken),
                    new KeyValuePair<string, string>("ClientId", ""),
                    new KeyValuePair<string, string>("UserId", ""),
                    new KeyValuePair<string, string>("SubTotalMode", "false"),
                    new KeyValuePair<string, string>("AdminMode", "false"),
                    new KeyValuePair<string, string>("UserIdForSchedule", ""),
                    new KeyValuePair<string, string>("ReportJson", postData),
                    new KeyValuePair<string, string>("SaveReport", "false"),
                };

                var encodedItems = keyvalues.Select(i => WebUtility.UrlEncode(i.Key) + "=" + WebUtility.UrlEncode(i.Value));
                var encodedContent = new StringContent(String.Join("&", encodedItems), null, "application/x-www-form-urlencoded");

                var response = client.PostAsync(new Uri(settings.ApiUrl + "/ReportApi/RunDrillDownReport"), encodedContent).Result;
                var stringContent = response.Content.ReadAsStringAsync().Result;
                var sql = "";
                if (stringContent.Contains("\"sql\":"))
                {
                    var sqlQuery = JsonConvert.DeserializeObject<Dictionary<string, object>>(stringContent);
                    object value;
                    var keyValuePair = sqlQuery.TryGetValue("sql", out value);
                    sql = Decrypt(value.ToString());
                }

                return sql;
            }
        }

        public static int FindFromIndex(string sql)
        {
            int parenthesesCount = 0;

            for (int i = 0; i < sql.Length - 4; i++)  // -4 because "FROM" has 4 characters
            {
                if (sql[i] == '(')
                {
                    parenthesesCount++;
                }
                else if (sql[i] == ')')
                {
                    parenthesesCount--;
                }
                else if (parenthesesCount == 0 && sql.Substring(i, 4).Equals("FROM", StringComparison.OrdinalIgnoreCase))
                {
                    return i;
                }
            }

            return -1;
        }

        public static List<string> SplitSqlColumns(string sql)
        {
            if (sql.StartsWith("EXEC")) return new List<string>();
            var fromIndex = FindFromIndex(sql);
            var sqlSplit = sql.Substring(0, fromIndex).Replace("SELECT", "").Trim();
            var sqlFields = Regex.Split(sqlSplit, "], (?![^\\(]*?\\))").Where(x => x != "CONVERT(VARCHAR(3)")
                .Select(x => x.EndsWith("]") ? x : x + "]")
                .Select(x => x.StartsWith("DISTINCT ") ? x.Replace("DISTINCT ", "") : x)
                .Select(x => x.StartsWith("TOP ") ? Regex.Replace(x, @"TOP\s+\d+", "") : x)
                .Where(x => x.Contains(" AS "))
                .ToList();

            return sqlFields;
        }

        public static DotNetReportDataModel DataTableToDotNetReportDataModel(DataTable dt, List<string> sqlFields, bool jsonAsTable = true)
        {
            var model = new DotNetReportDataModel
            {
                Columns = new List<DotNetReportDataColumnModel>(),
                Rows = new List<DotNetReportDataRowModel>()
            };

            if (!sqlFields.Any())
            {
                foreach (DataColumn c in dt.Columns) { sqlFields.Add($"{c.ColumnName} AS {c.ColumnName}"); }
            }

            int i = 0;
            foreach (DataColumn col in dt.Columns)
            {
                var sqlField = sqlFields[i++];
                if (col.ColumnName.Contains("__prm__")) continue;
                model.Columns.Add(new DotNetReportDataColumnModel
                {
                    SqlField = sqlField.Contains(" FROM ") ? col.ColumnName : sqlField.Substring(0, sqlField.LastIndexOf(" AS ")).Trim().Replace("__jsonc__", ""),
                    ColumnName = col.ColumnName,
                    DataType = col.DataType.ToString(),
                    IsNumeric = IsNumericType(col.DataType),
                    FormatType = sqlField.Contains("__jsonc__") ? "Json" : (sqlField.Contains(" FROM ") ? "Csv" : "")
                });

                col.ColumnName = col.ColumnName.Replace("__jsonc__", "");
            }

            foreach (DataRow row in dt.Rows)
            {
                i = 0;
                var items = new List<DotNetReportDataRowItemModel>();

                foreach (DataColumn col in dt.Columns)
                {
                    if (!col.ColumnName.Contains("__prm__"))
                    {
                        var item = new DotNetReportDataRowItemModel
                        {
                            Column = model.Columns[i],
                            Value = row[col] != null ? row[col].ToString() : null,
                            FormattedValue = GetFormattedValue(col, row, model.Columns[i].FormatType, jsonAsTable),
                            LabelValue = GetLabelValue(col, row)
                        };

                        items.Add(item);

                        try
                        {
                            row[col] = item.FormattedValue;
                        }
                        catch (Exception ex)
                        {
                            // ignore
                        }
                    }
                    i += 1;
                }

                model.Rows.Add(new DotNetReportDataRowModel
                {
                    Items = items.ToArray()
                });
            }

            return model;
        }

        private static string GetWhereClause(string sql)
        {

            int whereIndex = sql.IndexOf("WHERE ", StringComparison.OrdinalIgnoreCase);
            int nextClauseIndex = sql.IndexOf("GROUP BY ", whereIndex, StringComparison.OrdinalIgnoreCase);
            if (nextClauseIndex < 0)
            {
                nextClauseIndex = sql.IndexOf("ORDER BY ", whereIndex, StringComparison.OrdinalIgnoreCase);
            }
            nextClauseIndex = nextClauseIndex < 0 ? sql.Length : nextClauseIndex;
            var modifiedSql = sql.Substring(0, whereIndex) + sql.Substring(nextClauseIndex);
            var whereClause = sql.Substring(whereIndex, nextClauseIndex - whereIndex);

            return whereClause;
        }

        private static string ReplaceWhereClause(string sql, string where)
        {
            int whereIndex = sql.IndexOf("WHERE ", StringComparison.OrdinalIgnoreCase);
            int nextClauseIndex = sql.IndexOf("ORDER BY ", whereIndex, StringComparison.OrdinalIgnoreCase);
            nextClauseIndex = nextClauseIndex < 0 ? sql.Length : nextClauseIndex;
            var modifiedSql = sql.Substring(0, whereIndex) + where + sql.Substring(nextClauseIndex);
            return modifiedSql;
        }

        public static DataSet GetDrillDownData(OleDbConnection conn, DataTable dt, List<string> sqlFields, string reportDataJson)
        {
            var drilldownRow = new List<string>();
            var dr = dt.Rows[0];
            int i = 0;
            foreach (DataColumn dc in dt.Columns)
            {
                var col = sqlFields[i++]; //columns.FirstOrDefault(x => x.fieldName == dc.ColumnName) ?? new ReportHeaderColumn();
                drilldownRow.Add($@"
                                    {{
                                        ""Value"":""{dr[dc]}"",
                                        ""FormattedValue"":""{dr[dc]}"",
                                        ""LabelValue"":""'{dr[dc]}'"",
                                        ""NumericValue"":null,
                                        ""Column"":{{
                                            ""SqlField"":""{col.Substring(0, col.LastIndexOf(" AS "))}"",
                                            ""ColumnName"":""{dc.ColumnName}"",
                                            ""DataType"":""{dc.DataType.ToString()}"",
                                            ""IsNumeric"":{(dc.DataType.Name.StartsWith("Int") || dc.DataType.Name == "Double" || dc.DataType.Name == "Decimal" ? "true" : "false")},
                                            ""FormatType"":""""
                                        }}
                                     }}
                                ");
            }

            var reportData = reportDataJson.Replace("\"DrillDownRow\":[]", $"\"DrillDownRow\": [{string.Join(",", drilldownRow)}]").Replace("\"IsAggregateReport\":true", "\"IsAggregateReport\":false");
            var drilldownSql = RunReportApiCall(reportData);

            var dts = new DataSet();
            var combinedSqls = "";
            if (!string.IsNullOrEmpty(drilldownSql))
            {
                foreach (DataRow ddr in dt.Rows)
                {
                    i = 0;
                    var filteredSql = drilldownSql;
                    foreach (DataColumn dc in dt.Columns)
                    {
                        var value = ddr[dc].ToString().Replace("'", "''");
                        filteredSql = filteredSql.Replace($"<{dc.ColumnName}>", value);
                    }

                    combinedSqls += filteredSql += ";\n";
                }

                using (var cmd = new OleDbCommand(combinedSqls, conn))
                using (var adp = new OleDbDataAdapter(cmd))
                {
                    adp.Fill(dts);
                }
            }

            return dts;
        }

        public static DataSet GetDrillDownData(IDatabaseConnection databaseConnection, string connectionString, DataTable dt, List<string> sqlFields, string reportDataJson)
        {
            var drilldownRow = new List<string>();
            var dr = dt.Rows[0];
            int i = 0;
            foreach (DataColumn dc in dt.Columns)
            {
                var col = sqlFields[i++]; //columns.FirstOrDefault(x => x.fieldName == dc.ColumnName) ?? new ReportHeaderColumn();
                drilldownRow.Add($@"
                                    {{
                                        ""Value"":""{dr[dc]}"",
                                        ""FormattedValue"":""{dr[dc]}"",
                                        ""LabelValue"":""'{dr[dc]}'"",
                                        ""NumericValue"":null,
                                        ""Column"":{{
                                            ""SqlField"":""{col.Substring(0, col.LastIndexOf(" AS "))}"",
                                            ""ColumnName"":""{dc.ColumnName}"",
                                            ""DataType"":""{dc.DataType.ToString()}"",
                                            ""IsNumeric"":{(dc.DataType.Name.StartsWith("Int") || dc.DataType.Name == "Double" || dc.DataType.Name == "Decimal" ? "true" : "false")},
                                            ""FormatType"":""""
                                        }}
                                     }}
                                ");
            }

            var reportData = reportDataJson.Replace("\"DrillDownRow\":[]", $"\"DrillDownRow\": [{string.Join(",", drilldownRow)}]").Replace("\"IsAggregateReport\":true", "\"IsAggregateReport\":false");
            var drilldownSql = RunReportApiCall(reportData);

            var dts = new DataSet();
            var combinedSqls = "";
            if (!string.IsNullOrEmpty(drilldownSql))
            {
                foreach (DataRow ddr in dt.Rows)
                {
                    i = 0;
                    var filteredSql = drilldownSql;
                    foreach (DataColumn dc in dt.Columns)
                    {
                        var value = ddr[dc].ToString().Replace("'", "''");
                        filteredSql = filteredSql.Replace($"<{dc.ColumnName}>", value);
                    }

                    combinedSqls += filteredSql += ";\n";
                }

                dts = databaseConnection.ExecuteDataSetQuery(connectionString, combinedSqls);
            }

            return dts;
        }

        private static int CompareValues(object value1, object value2, Type dtType)
        {
            bool isInt = dtType == typeof(int) || dtType == typeof(long) || dtType == typeof(Int16) || dtType == typeof(Int32);
            bool isDecimal = dtType == typeof(decimal) || dtType == typeof(double) || dtType == typeof(float);

            if (value2 == null) { return 1; }
            if (value1 == null) { return -1; }

            if (dtType == typeof(DateTime))
            {
                DateTime date1 = Convert.ToDateTime(value1);
                DateTime date2 = Convert.ToDateTime(value2);
                return DateTime.Compare(date1, date2);
            }
            else if (isInt || isDecimal)
            {
                double dblValue1 = Convert.ToDouble(value1);
                double dblValue2 = Convert.ToDouble(value2);
                return dblValue1.CompareTo(dblValue2);
            }
            return value1.ToString().CompareTo(value2.ToString());
        }

        public static async Task<DataTable> ExecuteCustomFunction(DataTable dataTable, string sql)
        {
            if (!sql.Contains("/*|")) return dataTable;

            Regex regex = new Regex(@"/\*\|(.*?)\|\*/");
            var matches = regex.Matches(sql);

            foreach (Match match in matches)
            {
                string functionCall = match.Groups[1].Value;
                int columnIndex = -1;

                // Find the column that contains the function call to replace it later
                foreach (DataColumn column in dataTable.Columns)
                {
                    if (column.Expression.Contains(match.Value))
                    {
                        columnIndex = column.Ordinal;
                        break;
                    }
                }

                var functionCalls = new List<string>();
                foreach (DataRow row in dataTable.Rows)
                {
                    string modifiedFunctionCall = functionCall;

                    // Iterate over all columns that end with "__prm__"
                    foreach (DataColumn column in dataTable.Columns.Cast<DataColumn>().Where(c => c.ColumnName.EndsWith("__prm__")))
                    {
                        string paramName = "{" + column.ColumnName.Replace("__prm__", "") + "}";

                        if (modifiedFunctionCall.Contains(paramName))
                        {
                            string valueReplacement = row[column].ToString();
                            // Check if the datatype is numeric
                            if (column.DataType == typeof(int) || column.DataType == typeof(decimal) || column.DataType == typeof(double) || column.DataType == typeof(long))
                            {
                                modifiedFunctionCall = modifiedFunctionCall.Replace(paramName, valueReplacement);
                            }
                            else
                            {
                                modifiedFunctionCall = modifiedFunctionCall.Replace(paramName, "\"" + valueReplacement + "\"");
                            }
                        }
                    }

                    //var result = await DynamicCodeRunner.RunCode(modifiedFunctionCall + ";");
                    //if (columnIndex != -1)
                    //{
                    //    row[columnIndex] = result;
                    //}
                }
            }

            foreach (var column in dataTable.Columns.Cast<DataColumn>().Where(c => c.ColumnName.EndsWith("__prm__")).ToList())
            {
                dataTable.Columns.Remove(column);
            }
            return dataTable;
        }

        public static DataTable PushDatasetIntoDataTable(DataTable tbl, DataSet dts, string pivotColumnName, string pivotFunction)
        {
            var dt = tbl.Copy();
            
            foreach (DataRow row in dt.Rows)
            {
                int rowIndex = dt.Rows.IndexOf(row);
                DataTable dtsTable = dts.Tables[rowIndex];
                var distinctValues = new Dictionary<string, HashSet<object>>();
                var maxValues = new Dictionary<string, object>();

                if (dtsTable.Columns.Contains(pivotColumnName))
                {
                    int pivotColumnIndex = dtsTable.Columns[pivotColumnName].Ordinal;
                    int dtColumnIndex = (pivotColumnIndex + 1 < dtsTable.Columns.Count) ? pivotColumnIndex + 1 : pivotColumnIndex;

                    var dtType = dtsTable.Columns[dtColumnIndex].DataType;
                    bool isInt = dtType == typeof(int) || dtType == typeof(long) || dtType == typeof(Int16) || dtType == typeof(Int32);
                    bool isDecimal = dtType == typeof(decimal) ||dtType == typeof(double) ||dtType == typeof(float);
                    bool isDate = dtType == typeof(DateTime);

                    foreach (DataRow dtsRow in dtsTable.Rows)
                    {
                        string newColumnName = dtsRow[pivotColumnName].ToString();
                        if (string.IsNullOrEmpty(newColumnName)) newColumnName = "(Blank)";
                        if (!dt.Columns.Contains(newColumnName))
                        {
                            dt.Columns.Add(newColumnName, pivotFunction.StartsWith("Count") ? typeof(int) : dtType);
                        }
                        
                        if (pivotFunction == "Count Distinct")
                        {
                            if (!distinctValues.ContainsKey(newColumnName))
                            {
                                distinctValues[newColumnName] = new HashSet<object>();
                            }
                            distinctValues[newColumnName].Add(dtsRow[dtColumnIndex]);
                        }
                        else if (pivotFunction == "Max")
                        {
                            object currentValue = dtsRow[dtColumnIndex];
                            if (!maxValues.ContainsKey(newColumnName) || CompareValues(currentValue, maxValues[newColumnName], dtType) > 0)
                            {
                                maxValues[newColumnName] = currentValue;
                            }
                        }
                        else if (pivotColumnIndex + 1 < dtsTable.Columns.Count)
                        {
                            if (pivotFunction == "Count")
                            {
                                row[newColumnName] = (string.IsNullOrEmpty(row[newColumnName].ToString()) ? 0 : Convert.ToInt32(row[newColumnName])) + 1;
                            }
                            else if (isInt)
                            {
                                row[newColumnName] = (string.IsNullOrEmpty(row[newColumnName].ToString()) ? 0 : Convert.ToInt32(row[newColumnName])) + (string.IsNullOrEmpty(dtsRow[pivotColumnIndex + 1].ToString()) ? 0 : Convert.ToInt32(dtsRow[pivotColumnIndex + 1]));

                            }
                            else if (isDecimal)
                            {
                                row[newColumnName] = (string.IsNullOrEmpty(row[newColumnName].ToString()) ? 0 : Convert.ToDecimal(row[newColumnName])) + (string.IsNullOrEmpty(dtsRow[pivotColumnIndex + 1].ToString()) ? 0 : Convert.ToDecimal(dtsRow[pivotColumnIndex + 1]));

                            }
                            else if (isDate)
                            {
                                row[newColumnName] = (string.IsNullOrEmpty(row[newColumnName].ToString()) ? "" : Convert.ToDateTime(row[newColumnName]).ToShortDateString()) + " " + (string.IsNullOrEmpty(dtsRow[pivotColumnIndex + 1].ToString()) ? "" : Convert.ToDateTime(dtsRow[pivotColumnIndex + 1]).ToShortDateString());
                            }
                            else
                            {
                                row[newColumnName] = row[newColumnName].ToString() + " " + dtsRow[pivotColumnIndex + 1].ToString();
                            }
                        }
                    }

                    if (pivotFunction == "Count Distinct")
                    {
                        foreach (var column in distinctValues)
                        {
                            row[column.Key] = column.Value.Count;
                        }
                    }
                    if (pivotFunction == "Max")
                    {
                        foreach (var column in maxValues)
                        {
                            row[column.Key] = column.Value;
                        }
                    }
                }
            }

            return dt;
        }


        public static async Task<byte[]> GetExcelFile(string reportSql, string connectKey, string reportName, string chartData = null, bool allExpanded = false,
                string expandSqls = null, List<ReportHeaderColumn> columns = null, bool includeSubtotal = false, bool pivot = false)
        {
            var sql = Decrypt(reportSql);
            var sqlFields = SplitSqlColumns(sql);

            // Execute sql
            var dt = new DataTable();
            using (var conn = new OleDbConnection(GetConnectionString(connectKey, true)))
            {
                conn.Open();
                var command = new OleDbCommand(sql, conn);
                var adapter = new OleDbDataAdapter(command);

                adapter.Fill(dt);

                if (pivot) dt = Transpose(dt);

                if (columns?.Count > 0)
                {
                    foreach (var col in columns)
                    {
                        if (dt.Columns.Contains(col.fieldName) && col.hideStoredProcColumn)
                        {
                            dt.Columns.Remove(col.fieldName);
                        }
                        else if (!String.IsNullOrWhiteSpace(col.customfieldLabel))
                        {
                            dt.Columns[col.fieldName].ColumnName = col.customfieldLabel;
                        }                       
                    }
                }
                using (ExcelPackage xp = new ExcelPackage())
                {
                    ExcelWorksheet ws = xp.Workbook.Worksheets.Add(reportName);

                    int rowstart = 1;
                    int colstart = 1;
                    int rowend = rowstart;
                    int colend = dt.Columns.Count;

                    if (dt.Rows.Count > 0)
                    {
                        ws.Cells[rowstart, colstart, rowend, colend].Merge = true;
                        ws.Cells[rowstart, colstart, rowend, colend].Value = reportName;
                        ws.Cells[rowstart, colstart, rowend, colend].Style.Font.Bold = true;
                        ws.Cells[rowstart, colstart, rowend, colend].Style.Font.Size = 14;

                        rowstart += 2;
                        rowend = rowstart + dt.Rows.Count;

                        FormatExcelSheet(dt, ws, rowstart, colstart, columns, includeSubtotal,true,chartData);

                        if (allExpanded)
                        {
                            var insertRowIndex = 3;

                            var drilldownRow = new List<string>();
                            var dr = dt.Rows[0];

                            int i = 0;
                            foreach (DataColumn dc in dt.Columns)
                            {
                                var col = sqlFields[i++]; //columns.FirstOrDefault(x => x.fieldName == dc.ColumnName) ?? new ReportHeaderColumn();
                                drilldownRow.Add($@"
                                    {{
                                        ""Value"":""{dr[dc]}"",
                                        ""FormattedValue"":""{dr[dc]}"",
                                        ""LabelValue"":""'{dr[dc]}'"",
                                        ""NumericValue"":null,
                                        ""Column"":{{
                                            ""SqlField"":""{col.Substring(0, col.LastIndexOf(" AS "))}"",
                                            ""ColumnName"":""{dc.ColumnName}"",
                                            ""DataType"":""{dc.DataType.ToString()}"",
                                            ""IsNumeric"":{(dc.DataType.Name.StartsWith("Int") || dc.DataType.Name == "Double" || dc.DataType.Name == "Decimal" ? "true" : "false")},
                                            ""FormatType"":""""
                                        }}
                                     }}
                                ");
                            }

                            var reportData = expandSqls.Replace("\"DrillDownRow\":[]", $"\"DrillDownRow\": [{string.Join(",", drilldownRow)}]").Replace("\"IsAggregateReport\":true", "\"IsAggregateReport\":false");
                            var drilldownSql = RunReportApiCall(reportData);

                            var combinedSqls = "";
                            if (!string.IsNullOrEmpty(drilldownSql))
                            {
                                foreach (DataRow ddr in dt.Rows)
                                {
                                    i = 0;
                                    var filteredSql = drilldownSql;
                                    foreach (DataColumn dc in dt.Columns)
                                    {
                                        var value = ddr[dc].ToString().Replace("'", "''");
                                        filteredSql = filteredSql.Replace($"<{dc.ColumnName}>", value);
                                    }

                                    combinedSqls += filteredSql += ";\n";
                                }

                                using (var dts = new DataSet())
                                {
                                    using (var cmd = new OleDbCommand(combinedSqls, conn))
                                    using (var adp = new OleDbDataAdapter(cmd))
                                    {
                                        adp.Fill(dts);
                                    }

                                    foreach (DataTable ddt in dts.Tables)
                                    {
                                        ws.InsertRow(insertRowIndex + 2, ddt.Rows.Count);

                                        FormatExcelSheet(ddt, ws, insertRowIndex == 3 ? 3 : (insertRowIndex + 1), ddt.Columns.Count + 1, columns, false, insertRowIndex == 3);

                                        insertRowIndex += ddt.Rows.Count + 1;
                                    }
                                }
                            }
                        }
                    }
                    ws.View.FreezePanes(4, 1);
                    return xp.GetAsByteArray();
                }
            }
        }

        public static ReportHeaderColumn GetColumnFormatting(DataColumn dc, List<ReportHeaderColumn> columns, ref string value)
        {
            var isCurrency = false;
            var isNumeric = dc.DataType.Name.StartsWith("Int") || dc.DataType.Name == "Double" || dc.DataType.Name == "Decimal";
            var formatColumn = columns?.FirstOrDefault(x => dc.ColumnName.StartsWith(x.fieldName)) ?? new ReportHeaderColumn();
            string decimalFormat = new string('0', formatColumn.decimalPlacesDigit.GetValueOrDefault());
            try
            {
                if (dc.DataType == typeof(decimal) || (formatColumn != null && (formatColumn.fieldFormating == "Decimal" || formatColumn.fieldFormating == "Double")))
                {
                    if (formatColumn.decimalPlacesDigit != null)
                    {
                        value = Convert.ToDecimal(value).ToString("###,###,##0." + decimalFormat);
                    }
                    else
                    {
                        value = Convert.ToDecimal(value).ToString("###,###,##0.00");
                    }
                    isNumeric = true;
                }
                if (formatColumn != null && formatColumn.fieldFormating == "Currency")
                {
                    if (formatColumn.currencySymbol != null && formatColumn.decimalPlacesDigit != null)
                    {
                        value = Convert.ToDecimal(value).ToString(formatColumn.currencySymbol + "###,###,##0." + decimalFormat);
                    }
                    else if (formatColumn.currencySymbol != null)
                    {
                        value = Convert.ToDecimal(value).ToString(formatColumn.currencySymbol + "###,###,##0.00");
                    }
                    isCurrency = true;
                }
                if (formatColumn != null && (formatColumn.fieldFormating == "Date" || formatColumn.fieldFormating == "Date and Time" || formatColumn.fieldFormating == "Time") && dc.DataType.Name == "DateTime")
                {
                    var date = Convert.ToDateTime(value);
                    value = formatColumn.fieldFormating.StartsWith("Date") ? date.ToShortDateString() + " " : "";
                    value += formatColumn.fieldFormating.EndsWith("Time") ? date.ToShortTimeString() : "";
                    value = value.Trim();
                }
            }
            catch (Exception ex)
            {
                // ignore formatting exceptions
            }

            if (formatColumn != null)
            {
                formatColumn.isNumeric = isNumeric;
                formatColumn.isCurrency = isCurrency;
            }

            return formatColumn ?? new ReportHeaderColumn
            {
                isNumeric = isNumeric,
                isCurrency = isCurrency
            };
        }


        private static XSolidBrush GetBrushWithColor(string htmlColor = "")
        {
            var color = ColorTranslator.FromHtml(!string.IsNullOrEmpty(htmlColor) ? htmlColor : "#007bff");
            var xColor = XColor.FromArgb(color.R, color.G, color.B);
            return new XSolidBrush(xColor);
        }

        private static List<string> WrapText(XGraphics gfx, string value, XRect rect, XFont font, XStringFormat format)
        {
            // Manually wrap the text if it's too long
            List<string> lines = new List<string>();
            string line = "";
            foreach (char c in value)
            {
                XSize size = gfx.MeasureString(line + c, font);
                if (size.Width > rect.Width || c == '\r' || c == '\n')
                {
                    lines.Add(line);
                    line = c == '\r' || c == '\n' ? "" : c.ToString();
                }
                else
                {
                    line += c;
                }
            }
            lines.Add(line);

            return lines;
        }

        public static byte[] GetPdfFileAlt(string reportSql, string connectKey, string reportName, string chartData = null,
                    List<ReportHeaderColumn> columns = null, bool includeSubtotal = false, bool pivot = false)
        {
            var sql = Decrypt(reportSql);
            var sqlFields = SplitSqlColumns(sql);

            var dt = new DataTable();
            using (var conn = new OleDbConnection(GetConnectionString(connectKey, true)))
            {
                conn.Open();
                var command = new OleDbCommand(sql, conn);
                var adapter = new OleDbDataAdapter(command);

                adapter.Fill(dt);
            }

            var document = new PdfDocument();
            var page = document.AddPage();
            var gfx = XGraphics.FromPdfPage(page);
            var tfx = new XTextFormatter(gfx);
            int maxColumnsPerPage = 10;
            int leftMargin = 40;

            if (pivot)
            {
                dt = Transpose(dt);
                page.Orientation = PageOrientation.Landscape;
            }

            if (dt.Columns.Count > maxColumnsPerPage)
            {
                page.Orientation = PdfSharp.PageOrientation.Landscape;
            }

            DataTableToDotNetReportDataModel(dt, sqlFields, false);

            // Calculate the height of the page
            double pageHeight = page.Height.Point - 50;

            var subTotals = new decimal[dt.Columns.Count];

            using (var ms = new MemoryStream())
            {
                //Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
                var fontNormal = new XFont("Arial", 11, XFontStyle.Regular);
                var fontBold = new XFont("Arial", 12, XFontStyle.Bold);
                var tableWidth = page.Width - 100;
                var columnWidth = Math.Max(tableWidth / dt.Columns.Count, 100f);
                var currentYPosition = 30;
                var currentXPosition = leftMargin;
                double cellPadding = 3; // set the padding value
                XRect rect = new XRect();

                // Report header
                gfx.DrawString(reportName,
                    new XFont("Arial", 14, XFontStyle.Bold), XBrushes.Black,
                    new XRect(0, currentYPosition, page.Width, 30),
                    XStringFormats.Center);

                currentYPosition += 40;

                // Render chart
                if (!string.IsNullOrEmpty(chartData) && chartData != "undefined")
                {
                    byte[] sPDFDecoded = Convert.FromBase64String(chartData.Substring(chartData.LastIndexOf(',') + 1));
                    var imageStream = new MemoryStream(sPDFDecoded);
                    var image = XImage.FromStream(imageStream);
                    var maxWidth = page.Width - 100;
                    var maxHeight = page.Height - currentYPosition - 20;

                    if (image.PixelWidth > maxWidth || image.PixelHeight > maxHeight)
                    {
                        var aspectRatio = (double)image.PixelWidth / image.PixelHeight;
                        var width = maxWidth;
                        var height = maxWidth / aspectRatio;

                        if (height > maxHeight)
                        {
                            height = maxHeight;
                            width = maxHeight * aspectRatio;
                        }

                        rect = new XRect(50, currentYPosition, width, height);
                        gfx.DrawImage(image, rect);
                    }
                    else
                    {
                        rect = new XRect(50, currentYPosition, image.PixelWidth, image.PixelHeight);
                        gfx.DrawImage(image, rect);
                    }

                    currentYPosition += (int)rect.Height + 20;
                }

                var usingMultipleRows = false;
                currentXPosition = leftMargin;
                for (int k = 0; k < dt.Columns.Count; k++)
                {
                    // Draw column headers
                    var columnFormatting = columns[k];
                    var columnName = !string.IsNullOrEmpty(columns[k].customfieldLabel) ? columns[k].customfieldLabel : columns[k].fieldName;

                    rect = new XRect(currentXPosition, currentYPosition, columnWidth, 20);

                    gfx.DrawRectangle(XPens.LightGray, rect);
                    rect.Inflate(-cellPadding, -cellPadding);
                    tfx.DrawString(columnName, fontBold, GetBrushWithColor(), rect, XStringFormats.TopLeft);
                    currentXPosition += (int)columnWidth;
                    if (currentXPosition + columnWidth > page.Width && k != dt.Columns.Count - 1)
                    {
                        // Move to the next row
                        currentYPosition += 20;
                        currentXPosition = leftMargin;
                        usingMultipleRows = true;
                    }
                }

                currentYPosition += 20;

                // Draw table rows
                for (int i = 0; i < dt.Rows.Count; i++)
                {

                    // Check if we need to add a new page
                    if (currentYPosition > pageHeight)
                    {
                        // Add a new page to the document
                        page = document.AddPage();
                        gfx = XGraphics.FromPdfPage(page);
                        tfx = new XTextFormatter(gfx);
                        currentYPosition = 20;

                        if (dt.Columns.Count > maxColumnsPerPage)
                        {
                            page.Orientation = PdfSharp.PageOrientation.Landscape;
                        }

                        currentXPosition = leftMargin;
                        for (int k = 0; k < dt.Columns.Count; k++)
                        {
                            // Draw column headers
                            var columnName = dt.Columns[k].ColumnName;
                            rect = new XRect(currentXPosition, currentYPosition, columnWidth, 20);
                            gfx.DrawRectangle(XPens.LightGray, rect);
                            tfx.DrawString(columnName, fontBold, GetBrushWithColor(), rect, XStringFormats.TopLeft);
                            currentXPosition += (int)columnWidth;
                            if (currentXPosition + columnWidth > page.Width && k != dt.Columns.Count - 1)
                            {
                                // Move to the next row
                                currentYPosition += 20;
                                currentXPosition = leftMargin;
                            }

                        }

                        currentYPosition += 20;
                    }

                    var maxLines = 1;
                    currentXPosition = leftMargin;

                    for (int j = 0; j < dt.Columns.Count; j++)
                    {
                        var value = dt.Rows[i][j].ToString();
                        var dc = dt.Columns[j];
                        var formatColumn = GetColumnFormatting(dc, columns, ref value);

                        var lines = WrapText(gfx, value, rect, fontNormal, XStringFormats.Center);
                        maxLines = Math.Max(maxLines, lines.Count);

                        rect = new XRect(currentXPosition, currentYPosition, columnWidth, 20 * maxLines);
                        gfx.DrawRectangle(XPens.WhiteSmoke, rect);

                        var horizontalAlignment = XStringFormat.Center;
                        if (formatColumn != null)
                            horizontalAlignment = formatColumn.fieldAlign == "Right" || (formatColumn.isNumeric && (formatColumn.fieldAlign == "Auto" || string.IsNullOrEmpty(formatColumn.fieldAlign))) ? XStringFormats.CenterRight : formatColumn.fieldAlign == "Center" ? XStringFormats.Center : XStringFormats.CenterLeft;

                        var yPosition = currentYPosition + 1;
                        foreach (string l in lines)
                        {
                            XRect lineRect = new XRect(rect.Left, yPosition, rect.Width, fontNormal.Height);
                            lineRect.Inflate(-cellPadding, -cellPadding);
                            gfx.DrawString(l, fontNormal, XBrushes.Black, lineRect, horizontalAlignment);

                            yPosition += fontNormal.Height;
                        }

                        currentXPosition += (int)columnWidth;
                        if (currentXPosition + columnWidth > page.Width && j != dt.Columns.Count - 1)
                        {
                            // Move to the next row
                            currentYPosition += (20 * maxLines);
                            currentXPosition = leftMargin;
                        }
                    }

                    currentYPosition += (20 * maxLines);
                    if (usingMultipleRows) // add extra spacing
                    {
                        currentYPosition += 10;
                    }
                }            

                if (includeSubtotal)
                {
                    // Draw subtotals
                    currentYPosition += 10;
                    currentXPosition = leftMargin;

                    for (int j = 0; j < dt.Columns.Count; j++)
                    {
                        var value = subTotals[j].ToString();
                        var dc = dt.Columns[j];
                        var formatColumn = GetColumnFormatting(dc, columns, ref value);

                        rect = new XRect(currentXPosition, currentYPosition, columnWidth, 20);
                        gfx.DrawRectangle(XPens.LightGray, rect);

                        if (formatColumn.isNumeric && !(formatColumn?.dontSubTotal ?? false))
                        {
                            gfx.DrawString(value, fontNormal, XBrushes.Black, rect, XStringFormats.CenterRight);
                        }
                        else
                        {
                            gfx.DrawString(" ", fontNormal, XBrushes.Black, rect, XStringFormats.Center);
                        }

                        currentXPosition += (int)columnWidth;
                        if (currentXPosition + columnWidth > page.Width && j != dt.Columns.Count - 1)
                        {
                            // Move to the next row
                            currentYPosition += 20;
                            currentXPosition = leftMargin;
                        }
                    }
                }

                gfx.Save();
                document.Save(ms);
                return ms.ToArray();
            }
        }

        public static async Task<byte[]> GetWordFile(string reportSql, string connectKey, string reportName, string chartData = null, bool allExpanded = false,
            string expandSqls = null, List<ReportHeaderColumn> columns = null, bool includeSubtotal = false, bool pivot = false)
        {
            var sql = Decrypt(reportSql);
            var sqlFields = SplitSqlColumns(sql);

            // Execute sql
            var dt = new DataTable();
            using (var conn = new OleDbConnection(GetConnectionString(connectKey, true)))
            {
                conn.Open();
                var command = new OleDbCommand(sql, conn);
                var adapter = new OleDbDataAdapter(command);

                adapter.Fill(dt);

                if (pivot) dt = Transpose(dt);

                if (columns?.Count > 0)
                {
                    foreach (var col in columns)
                    {
                        if (dt.Columns.Contains(col.fieldName) && col.hideStoredProcColumn)
                        {
                            dt.Columns.Remove(col.fieldName);
                        }
                        else if (!String.IsNullOrWhiteSpace(col.customfieldLabel))
                        {
                            dt.Columns[col.fieldName].ColumnName = col.customfieldLabel;
                        }
                    }
                }

                using (MemoryStream memStream = new MemoryStream())
                {
                    using (WordprocessingDocument wordDocument = WordprocessingDocument.Create(memStream, WordprocessingDocumentType.Document))
                    {
                        MainDocumentPart mainPart = wordDocument.AddMainDocumentPart();
                        mainPart.Document = new Document();
                        Body body = mainPart.Document.AppendChild(new Body());                     
                        // Add report header
                        Paragraph header = new Paragraph(new Run(new RunProperties()
                        {
                            FontSize = new DocumentFormat.OpenXml.Wordprocessing.FontSize() { Val = "28" },// Font size 14 points (2 * 14)
                            Bold = new Bold(),
                        }, new Text(reportName)));
                        header.ParagraphProperties = new ParagraphProperties(new Justification() { Val = JustificationValues.Center });
                        body.AppendChild(header);

                        // Render chart
                        if (!string.IsNullOrEmpty(chartData) && chartData != "undefined")
                        {
                            byte[] imageDecoded = Convert.FromBase64String(chartData.Substring(chartData.LastIndexOf(',') + 1));
                            using (MemoryStream imageStream = new MemoryStream(imageDecoded))
                            {
                                ImagePart imagePart = mainPart.AddImagePart(ImagePartType.Jpeg);
                                imagePart.FeedData(imageStream);
                                // Specify the size in pixels and convert to EMUs
                                int widthInPixels = 500;
                                int heightInPixels = 400;
                                long widthInEmus = widthInPixels * 9525;
                                long heightInEmus = heightInPixels * 9525;
                                AddImageToBody(wordDocument, mainPart.GetIdOfPart(imagePart),widthInEmus, heightInEmus);
                            }
                        }
                        // Add data in table format
                        if (dt.Rows.Count > 0)
                        {
                            // Create table
                            Table table = new Table();
                            TableProperties props = new TableProperties(new Justification() { Val = JustificationValues.Center },
                             new TableBorders(
                             new TopBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 8 },
                             new BottomBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 10 },
                             new LeftBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 10 },
                             new RightBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 10 },
                             new InsideHorizontalBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 8 },
                             new InsideVerticalBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 10 }
                             ));

                            // Append table properties
                            table.AppendChild<TableProperties>(props);
                            // Add header row
                            TableRow headerRow = new TableRow();
                            // Calculate max text width for each column
                            int[] maxColumnWidths = new int[dt.Columns.Count];
                            foreach (DataColumn column in dt.Columns)
                            {
                                maxColumnWidths[column.Ordinal] = EstimateTextWidth(column.ColumnName);
                                RunProperties runProperties = new RunProperties(
                                    new Bold(),
                                    new DocumentFormat.OpenXml.Wordprocessing.Color() { Val = "#156082" } // Example color
                                );
                                Run run = new Run(runProperties, new Text(column.ColumnName));
                                ParagraphProperties paragraphProperties = new ParagraphProperties(
                                    new SpacingBetweenLines() { Before = "100", After = "100", Line = "240", LineRule = LineSpacingRuleValues.Auto },
                                    new Indentation() { Left = "180", Right = "180"} // Adjust values as needed
                                );
                                Paragraph paragraph = new Paragraph(paragraphProperties, run);
                                TableCell cell = new TableCell(paragraph);
                                headerRow.AppendChild(cell);
                            }
                            table.AppendChild(headerRow);

                            // Normalize column widths to fit the available width
                            int totalWidth = 0;
                            foreach (int width in maxColumnWidths)
                            {
                                totalWidth += width;
                            }
                            if (totalWidth > 13900)
                            {
                                // Set landscape orientation
                                SectionProperties sectionProperties = new SectionProperties();
                                DocumentFormat.OpenXml.Wordprocessing.PageSize pageSize = new DocumentFormat.OpenXml.Wordprocessing.PageSize() { Width = Convert.ToUInt32(totalWidth +(1440*2)), Orient = PageOrientationValues.Landscape };
                                sectionProperties.Append(pageSize);
                                body.Append(sectionProperties);
                            }
                            else
                            {
                                // Set page orientation to landscape
                                SectionProperties sectionProps = new SectionProperties();
                                DocumentFormat.OpenXml.Wordprocessing.PageSize defaultpageSize = new DocumentFormat.OpenXml.Wordprocessing.PageSize()
                                {
                                    Orient = PageOrientationValues.Landscape,
                                    Width = 16838,  // 11.69 inch in Twips (297 mm)
                                };
                                sectionProps.Append(defaultpageSize);
                                body.Append(sectionProps);
                            }
                            // Add data rows
                            foreach (DataRow row in dt.Rows)
                            {
                                TableRow dataRow = new TableRow();
                                foreach (DataColumn column in dt.Columns)
                                {
                                    var value = row[column.ColumnName].ToString();
                                    var formatColumn = GetColumnFormatting(column, columns, ref value);
                                    Run run = new Run( new Text(value));
                                    ParagraphProperties paragraphProperties = new ParagraphProperties(
                                        new SpacingBetweenLines() { Before = "100", After = "100", Line = "240", LineRule = LineSpacingRuleValues.Auto },
                                        new Indentation() { Left = "180", Right = "180" } // Adjust values as needed
                                    );
                                    Paragraph paragraph = new Paragraph(paragraphProperties, run);
                                    TableCell cell = new TableCell(paragraph);
                                    dataRow.AppendChild(cell);
                                }
                                table.AppendChild(dataRow);
                            }
                            body.AppendChild(table);
                            // Add expanded data if applicable
                            if (allExpanded)
                            {
                                Paragraph expandedData = new Paragraph(new Run(new Text("Additional expanded data")));
                                body.AppendChild(expandedData);
                            }
                        }
                        else
                        {
                            Paragraph expandedData = new Paragraph(new Run(new Text("No RecordS Found")));
                            body.AppendChild(expandedData);
                        }
                        // Ensure word wrapping doesn't break words
                        foreach (TableCell cell in body.Descendants<TableCell>())
                        {
                            cell.TableCellProperties = new TableCellProperties();
                            NoWrap noWrap = new NoWrap();
                            cell.TableCellProperties.Append(noWrap);
                        }
                        wordDocument.Save();
                    }
                    return memStream.ToArray();
                }
            }
        }
        static void AddImageToBody(WordprocessingDocument wordDoc, string relationshipId, long cx, long cy)
        {
            // Define the reference of the image.
            var element =
                 new  Drawing(
                     new DW.Inline(
                         new DW.Extent() { Cx = cx, Cy = cy },
                         new DW.EffectExtent()
                         {
                             LeftEdge = 0L,
                             TopEdge = 0L,
                             RightEdge = 0L,
                             BottomEdge = 0L
                         },
                         new DW.DocProperties()
                         {
                             Id = (UInt32Value)1U,
                             Name = "Chart Image"
                         },
                         new DW.NonVisualGraphicFrameDrawingProperties(
                             new A.GraphicFrameLocks() { NoChangeAspect = true }),
                         new A.Graphic(
                             new A.GraphicData(
                                 new PIC.Picture(
                                     new PIC.NonVisualPictureProperties(
                                         new PIC.NonVisualDrawingProperties()
                                         {
                                             Id = (UInt32Value)0U,
                                             Name = "New Bitmap Chart Image.jpg"
                                         },
                                         new PIC.NonVisualPictureDrawingProperties()),
                                     new PIC.BlipFill(
                                         new A.Blip(
                                             new A.BlipExtensionList(
                                                 new A.BlipExtension()
                                                 {
                                                     Uri =
                                                        "{28A0092B-C50C-407E-A947-70E740481C1C}"
                                                 })
                                         )
                                         {
                                             Embed = relationshipId,
                                             CompressionState =
                                             A.BlipCompressionValues.Print
                                         },
                                         new A.Stretch(
                                             new A.FillRectangle())),
                                     new PIC.ShapeProperties(
                                         new A.Transform2D(
                                             new A.Offset() { X = 0L, Y = 0L },
                                             new A.Extents() { Cx = 990000L, Cy = 792000L }),
                                         new A.PresetGeometry(
                                             new A.AdjustValueList()
                                         )
                                         { Preset = A.ShapeTypeValues.Rectangle }))
                             )
                             { Uri = "http://schemas.openxmlformats.org/drawingml/2006/picture" })
                     )
                     {
                         DistanceFromTop = (UInt32Value)0U,
                         DistanceFromBottom = (UInt32Value)0U,
                         DistanceFromLeft = (UInt32Value)0U,
                         DistanceFromRight = (UInt32Value)0U,
                         EditId = "50D07946"
                     });

            if (wordDoc.MainDocumentPart is null || wordDoc.MainDocumentPart.Document.Body is null)
            {
                throw new ArgumentNullException("MainDocumentPart and/or Body is null.");
            }
            Paragraph paragraph = new Paragraph(new Run(element));
            paragraph.ParagraphProperties = new ParagraphProperties(new Justification() { Val = JustificationValues.Center });
            wordDoc.MainDocumentPart.Document.Body.AppendChild(paragraph);
        }
        static private int EstimateTextWidth(string text)
        {
            // Simple estimation: number of characters * average width of a character in twips
            // Note: 1 inch = 1440 twips, and average character width can vary. Adjust this multiplier as needed.
            int averageCharWidthInTwips = 120;
            return text.Length * averageCharWidthInTwips;
        }
        
        public static string GetXmlFile(string reportSql, string connectKey, string reportName)
        {
            var sql = Decrypt(reportSql);

            // Execute sql
            var dt = new DataTable();
            var ds = new DataSet();
            using (var conn = new OleDbConnection(GetConnectionString(connectKey, true)))
            {
                conn.Open();
                var command = new OleDbCommand(sql, conn);
                var adapter = new OleDbDataAdapter(command);

                adapter.Fill(dt);
            }

            ds.Tables.Add(dt);
            ds.DataSetName = "data";
            foreach (DataColumn c in dt.Columns)
            {
                c.ColumnName = c.ColumnName.Replace(" ", "_").Replace("(", "").Replace(")", "");
            }
            dt.TableName = "item";
            var xml = ds.GetXml();
            return xml;
        }

        /// <summary>
        /// Method to Deycrypt encrypted sql statement. PLESE DO NOT CHANGE THIS METHOD
        /// </summary>
        public static string Decrypt(string encryptedText)
        {
            encryptedText = encryptedText.Split(new string[] { "%2C", "," }, StringSplitOptions.RemoveEmptyEntries)[0];
            byte[] initVectorBytes = Encoding.ASCII.GetBytes("yk0z8f39lgpu70gi"); // PLESE DO NOT CHANGE THIS KEY
            int keysize = 256;

            byte[] cipherTextBytes = Convert.FromBase64String(encryptedText.Replace("%3D", "="));
            var passPhrase = ConfigurationManager.AppSettings["dotNetReport.privateApiToken"].ToLower();
            using (PasswordDeriveBytes password = new PasswordDeriveBytes(passPhrase, null))
            {
                byte[] keyBytes = password.GetBytes(keysize / 8);
                using (RijndaelManaged symmetricKey = new RijndaelManaged())
                {
                    symmetricKey.Mode = CipherMode.CBC;
                    using (ICryptoTransform decryptor = symmetricKey.CreateDecryptor(keyBytes, initVectorBytes))
                    {
                        using (MemoryStream memoryStream = new MemoryStream(cipherTextBytes))
                        {
                            using (CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                            {
                                byte[] plainTextBytes = new byte[cipherTextBytes.Length];
                                int decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);
                                var decryptedString = Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount);
                                while (decryptedByteCount > 0)
                                {
                                    decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);
                                    decryptedString += Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount);
                                }

                                return decryptedString;
                            }
                        }
                    }
                }
            }
        }
        public static byte[] GetCSVFile(string reportSql, string connectKey, List<ReportHeaderColumn> columns = null, bool includeSubtotal = false)
        {
            var sql = Decrypt(reportSql);

            // Execute sql
            var dt = new DataTable();
            using (var conn = new OleDbConnection(GetConnectionString(connectKey, true)))
            {
                conn.Open();
                var command = new OleDbCommand(sql, conn);
                var adapter = new OleDbDataAdapter(command);

                adapter.Fill(dt);
                var subTotals = new decimal[dt.Columns.Count];

                //Build the CSV file data as a Comma separated string.
                string csv = string.Empty;
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    DataColumn column = dt.Columns[i];
                    var columnName = !string.IsNullOrEmpty(columns[i].customfieldLabel) ? columns[i].customfieldLabel : columns[i].fieldName;
                    csv += columnName + ',';
                }

                //Add new line.
                csv += "\r\n";

                foreach (DataRow row in dt.Rows)
                {
                    var i = 0;
                    foreach (DataColumn column in dt.Columns)
                    {
                        var value = row[column.ColumnName].ToString();
                        var formatColumn = GetColumnFormatting(column, columns, ref value);

                        if (includeSubtotal)
                        {
                            if (formatColumn.isNumeric && !(formatColumn?.dontSubTotal ?? false))
                            {
                                subTotals[i] += Convert.ToDecimal(row[column.ColumnName]);
                            }
                        }

                        //Add the Data rows.
                        csv += $"{(i == 0 ? "" : ",")}\"{value}\"";
                        i++;
                    }

                    //Add new line.
                    csv += "\r\n";
                }

                if (includeSubtotal)
                {
                    for (int j = 0; j < dt.Columns.Count; j++)
                    {
                        var value = subTotals[j].ToString();
                        var dc = dt.Columns[j];
                        var formatColumn = GetColumnFormatting(dc, columns, ref value);
                        if (formatColumn.isNumeric && !(formatColumn?.dontSubTotal ?? false))
                        {
                            csv += $"{(j == 0 ? "" : ",")}\"{value}\"";
                        }
                        else
                        {
                            csv += $"{(j == 0 ? "" : ",")}\"\"";
                        }
                    }

                    csv += "\r\n";
                }

                return Encoding.ASCII.GetBytes(csv);
                //return csv;
            }
        }

        public static dynamic GetDbConnectionSettings(string account, string dataConnect, bool addOledbProvider = true)
        {
            var _configFilePath = Path.Combine(Directory.GetCurrentDirectory(), _configFileName);
            if (!System.IO.File.Exists(_configFilePath))
            {
                var emptyConfig = new JObject();
                System.IO.File.WriteAllText(_configFilePath, emptyConfig.ToString(Newtonsoft.Json.Formatting.Indented));
            }

            string configContent = System.IO.File.ReadAllText(_configFilePath);

            var config = JObject.Parse(configContent);
            var dotNetReportSection = config[$"dotNetReport"] as JObject;

            // First try to get connection from the dotnetreport appsettings file
            if (dotNetReportSection != null && !string.IsNullOrEmpty(dataConnect))
            {
                var dataConnectSection = dotNetReportSection[dataConnect] as JObject;
                if (dataConnectSection != null)
                {
                    return dataConnectSection.ToObject<dynamic>();
                }
            }
            else
            {
                // Next try to get config from appsettings (original method)
                var connection = DotNetReportHelper.GetConnection();
                var connectionString = DotNetReportHelper.GetConnectionString(connection, addOledbProvider).Result;

                if (!string.IsNullOrEmpty(connectionString))
                {
                    var dbConfig = new JObject
                    {
                        ["DatabaseType"] = "MS Sql",
                        ["ConnectionKey"] = "Default",
                        ["ConnectionString"] = connectionString
                    };
                    return dbConfig.ToObject<dynamic>();
                }
            }

            return null;
        }

        public static void UpdateDbConnection(UpdateDbConnectionModel model)
        {
            // Use dependency injection to get the appropriate implementation based on the database type
            var databaseConnection = DatabaseConnectionFactory.GetConnection(model.dbType);

            var connectionString = "";
            if (model.connectionType == "Build")
            {
                connectionString = databaseConnection.CreateConnection(model);
            }
            else
            {
                connectionString = DotNetReportHelper.GetConnectionString(model.connectionKey, false);

                if (string.IsNullOrEmpty(connectionString))
                {
                    throw new Exception($"Connection string with key '{model.connectionKey}' was not found in App Config");
                }
            }

            try
            {
                // Test the database connection
                if (!databaseConnection.TestConnection(connectionString))
                {
                    throw new Exception("Could not connect to the Database.");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Could not connect to the Database. Error: {ex.Message}");
            }

            if (!model.testOnly)
            {
                var _configFilePath = Path.Combine(Directory.GetCurrentDirectory(), _configFileName);
                if (!System.IO.File.Exists(_configFilePath))
                {
                    var emptyConfig = new JObject();
                    System.IO.File.WriteAllText(_configFilePath, emptyConfig.ToString(Newtonsoft.Json.Formatting.Indented));
                }

                // Get the existing JSON configuration
                var config = JObject.Parse(System.IO.File.ReadAllText(_configFilePath));
                if (config["dotNetReport"] == null)
                {
                    config["dotNetReport"] = new JObject();
                }

                // Update the specified properties within the "dotNetReport" and "dataConfig" section
                var dotNetReportSection = config["dotNetReport"] as JObject;
                if (dotNetReportSection[model.dataConnect] == null)
                {
                    dotNetReportSection[model.dataConnect] = new JObject();
                }

                var dataConnectSection = dotNetReportSection[model.dataConnect] as JObject;

                dataConnectSection["DatabaseType"] = model.dbType;
                dataConnectSection["ConnectionType"] = model.connectionType;
                if (model.connectionType == "Build")
                {
                    dataConnectSection["ConnectionKey"] = "Default";
                    dataConnectSection["ConnectionString"] = connectionString;
                    dataConnectSection["DatabaseHost"] = model.dbServer;
                    dataConnectSection["DatabasePort"] = model.dbPort;
                    dataConnectSection["DatabaseName"] = model.dbName;
                    dataConnectSection["Username"] = model.dbUsername;
                    dataConnectSection["Password"] = model.dbPassword;
                    dataConnectSection["AuthenticationType"] = model.dbAuthType;

                }
                else if (model.connectionType == "Key")
                {
                    dataConnectSection["ConnectionKey"] = model.connectionKey;
                    dataConnectSection["ConnectionString"] = connectionString;
                }

                if (model.isDefault)
                {
                    dotNetReportSection["DefaultConnection"] = model.dataConnect;
                }

                // Save the updated JSON back to the file
                File.WriteAllText(_configFilePath, config.ToString());
            }

        }

        public static void UpdateUserConfigSetting(UpdateUserConfigModel model)
        {
            var _configFilePath = Path.Combine(Directory.GetCurrentDirectory(), _configFileName);
            if (!System.IO.File.Exists(_configFilePath))
            {
                var emptyConfig = new JObject();
                System.IO.File.WriteAllText(_configFilePath, emptyConfig.ToString(Newtonsoft.Json.Formatting.Indented));
            }

            // Get the existing JSON configuration
            var config = JObject.Parse(System.IO.File.ReadAllText(_configFilePath));
            if (config["dotNetReport"] == null)
            {
                config["dotNetReport"] = new JObject();
            }

            // Update the specified properties within the "dotNetReport" and "dataConfig" section
            var dotNetReportSection = config["dotNetReport"] as JObject;
            if (dotNetReportSection[model.dataConnect] == null)
            {
                dotNetReportSection[model.dataConnect] = new JObject();
            }

            var dataConnectSection = dotNetReportSection[model.dataConnect] as JObject;
            dataConnectSection["UserConfig"] = model.userConfig;

            // Save the updated JSON back to the file
            System.IO.File.WriteAllText(_configFilePath, config.ToString());

        }

        public static void UpdateConfigurationFile(string accountApiKey, string privateApiKey, string dataConnectKey, bool onlyIfEmpty = false)
        {
            var _configFileName = "appsettings.json";
            var _configFilePath = Path.Combine(Directory.GetCurrentDirectory(), _configFileName);

            JObject existingConfig;
            if (System.IO.File.Exists(_configFilePath))
            {
                existingConfig = JObject.Parse(System.IO.File.ReadAllText(_configFilePath));
                if (existingConfig["dotNetReport"] is JObject dotNetReportObject)
                {
                    if (!onlyIfEmpty || (onlyIfEmpty && dotNetReportObject["accountApiToken"] != null && dotNetReportObject["accountApiToken"].ToString() == "Your Account API Key"))
                    {
                        dotNetReportObject["accountApiToken"] = accountApiKey;
                        dotNetReportObject["dataconnectApiToken"] = dataConnectKey;

                        if (!string.IsNullOrEmpty(privateApiKey))
                            dotNetReportObject["privateApiToken"] = privateApiKey;

                        System.IO.File.WriteAllText(_configFilePath, existingConfig.ToString(Newtonsoft.Json.Formatting.Indented));
                    }
                }
            }
        }

        public static AppSettingModel GetAppSettings()
        {
            var _configFilePath = Path.Combine(Directory.GetCurrentDirectory(), _configFileName);
            if (!System.IO.File.Exists(_configFilePath))
            {
                var emptyConfig = new JObject();
                System.IO.File.WriteAllText(_configFilePath, emptyConfig.ToString(Newtonsoft.Json.Formatting.Indented));
            }

            string configContent = System.IO.File.ReadAllText(_configFilePath);

            var config = JObject.Parse(configContent);
            // Extract the AppSetting section
            var appSetting = config["dotNetReport"]?["AppSetting"];

            // Create an instance of ApiSettingModel and populate its properties
            var settings = new AppSettingModel
            {
                emailAddress = appSetting?["email"]?["fromemail"]?.ToString(),
                emailName = appSetting?["email"]?["fromname"]?.ToString(),
                emailServer = appSetting?["email"]?["server"]?.ToString(),
                emailPort = appSetting?["email"]?["port"]?.ToString(),
                emailUserName = appSetting?["email"]?["username"]?.ToString(),
                emailPassword = appSetting?["email"]?["password"]?.ToString(),
                backendApiUrl = appSetting?["BaseApiUrl"]?.ToString() ?? "https://dotnetreport.com/api",
                timeZone = appSetting?["TimeZone"]?.ToString() ?? "-6",
                appThemes = appSetting?["AppTheme"]?.ToString() ?? "default"
            };

            return settings;
        }

        public static void SaveAppSettings(AppSettingModel model)
        {
            var _configFilePath = Path.Combine(Directory.GetCurrentDirectory(), _configFileName);
            if (!System.IO.File.Exists(_configFilePath))
            {
                var emptyConfig = new JObject();
                System.IO.File.WriteAllText(_configFilePath, emptyConfig.ToString(Newtonsoft.Json.Formatting.Indented));
            }

            //// Get the existing JSON configuration
            var config = JObject.Parse(System.IO.File.ReadAllText(_configFilePath));
            if (config["dotNetReport"] == null)
            {
                config["dotNetReport"] = new JObject();
            }
            var appsetting = config["dotNetReport"]["AppSetting"] ?? new JObject();
            appsetting["email"] = new JObject
            {
                ["fromemail"] = model.emailAddress,
                ["fromname"] = model.emailName,
                ["server"] = model.emailServer,
                ["port"] = model.emailPort,
                ["username"] = model.emailUserName,
                ["password"] = model.emailPassword
            };

            appsetting["BaseApiUrl"] = model.backendApiUrl;
            appsetting["TimeZone"] = model.timeZone;
            appsetting["AppTheme"] = model.appThemes;

            config["dotNetReport"]["AppSetting"] = appsetting;

            // Save the updated JSON back to the file
            System.IO.File.WriteAllText(_configFilePath, config.ToString());

        }

    }

    public class UpdateUserConfigModel
    {
        public string account { get; set; }
        public string dataConnect { get; set; }
        public string userConfig { get; set; }
    }

    public class UpdateDbConnectionModel
    {
        public string account { get; set; } = "";
        public string dataConnect { get; set; } = "";
        public string dbType { get; set; } = "";
        public string connectionType { get; set; } = "";
        public string connectionKey { get; set; } = "";
        public string connectionString { get; set; } = "";
        public string dbServer { get; set; } = "";
        public string dbPort { get; set; } = "";
        public string dbName { get; set; } = "";
        public string dbAuthType { get; set; } = "";
        public string dbUsername { get; set; } = "";
        public string dbPassword { get; set; } = "";
        public string providerName { get; set; } = "";
        public bool isDefault { get; set; }
        public bool testOnly { get; set; }
    }

    public class AppSettingModel
    {
        public string account { get; set; } = "";
        public string dataConnect { get; set; } = "";
        public string emailUserName { get; set; } = "";
        public string emailPassword { get; set; } = "";
        public string emailServer { get; set; } = "";
        public string emailPort { get; set; } = "";
        public string emailName { get; set; } = "";
        public string emailAddress { get; set; } = "";
        public string backendApiUrl { get; set; } = "";
        public string timeZone { get; set; } = "";
        public string appThemes { get; set; } = "";
    }

    public static class DatabaseConnectionFactory
    {
        public static IDatabaseConnection GetConnection(string dbtype)
        {
            IDatabaseConnection databaseConnection;
            switch (dbtype.ToLower())
            {
                case "ms sql":
                    databaseConnection = new SqlServerDatabaseConnection();
                    break;
                default:
                    databaseConnection = new OleDbDatabaseConnection();
                    break;
            }

            return databaseConnection;
        }


    }
    public interface IDatabaseConnection
    {
        bool TestConnection(string connectionString);
        string CreateConnection(UpdateDbConnectionModel model);
        int GetTotalRecords(string connectionString, string sqlCount, string sql);
        DataTable ExecuteQuery(string connectionString, string sql);
        DataSet ExecuteDataSetQuery(string connectionString, string combinedSqls);
    }
    public class SqlServerDatabaseConnection : IDatabaseConnection
    {
        public bool TestConnection(string connectionString)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    //Test Connection
                    conn.Open();
                    conn.Close();
                    return true;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }


        public string CreateConnection(UpdateDbConnectionModel model)
        {
            SqlConnectionStringBuilder sqlConnectionStringBuilder = new SqlConnectionStringBuilder();

            // Set other SQL connection properties
            sqlConnectionStringBuilder.DataSource = model.dbServer;
            sqlConnectionStringBuilder.Add("Initial Catalog", model.dbName);
            sqlConnectionStringBuilder.Encrypt = false;
            if (model.dbAuthType.ToLower() == "username")
            {
                sqlConnectionStringBuilder.Add("User ID", model.dbUsername);
                sqlConnectionStringBuilder.Add("Password", model.dbPassword);
            }
            else
            {
                sqlConnectionStringBuilder.Add("Integrated Security", "SSPI");
            }

            return sqlConnectionStringBuilder.ConnectionString;
        }

        public int GetTotalRecords(string connectionString, string sqlCount, string sql)
        {
            int totalRecords = 0;

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    using (SqlCommand command = new SqlCommand(sqlCount, conn))
                    {
                        if (!sql.StartsWith("EXEC")) totalRecords = Math.Max(totalRecords, (int)command.ExecuteScalar());

                    }
                }
            }
            catch (Exception ex)
            {
                // Handle exception (log, rethrow, etc.)
                throw new Exception($"Error executing SQL query for total records: {ex.Message}", ex);
            }

            return totalRecords;
        }

        public DataTable ExecuteQuery(string connectionString, string sql)
        {
            DataTable dataTable = new DataTable();

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    using (SqlCommand command = new SqlCommand(sql, conn))
                    {
                        using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                        {
                            adapter.Fill(dataTable);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle exception (log, rethrow, etc.)
                throw new Exception($"Error executing SQL query: {ex.Message}", ex);
            }

            return dataTable;
        }

        public DataSet ExecuteDataSetQuery(string connectionString, string combinedSqls)
        {
            var dts = new DataSet();
            try
            {
                using (var conn = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand(combinedSqls, conn))
                using (var adp = new SqlDataAdapter(cmd))
                {
                    adp.Fill(dts);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error executing SQL query: {ex.Message}", ex);
            }
            return dts;
        }
    }

    public class OleDbDatabaseConnection : IDatabaseConnection
    {
        public bool TestConnection(string connectionString)
        {
            using (OleDbConnection conn = new OleDbConnection(connectionString))
            {
                try
                {
                    //Test Connection
                    conn.Open();
                    conn.Close();
                    return true;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }


        public string CreateConnection(UpdateDbConnectionModel model)
        {
            OleDbConnectionStringBuilder OleDbConnectionStringBuilder = new OleDbConnectionStringBuilder();

            // Set other OleDb connection properties
            OleDbConnectionStringBuilder.Provider = model.providerName;
            OleDbConnectionStringBuilder.DataSource = model.dbServer;
            OleDbConnectionStringBuilder.Add("Initial Catalog", model.dbName);
            if (model.dbAuthType.ToLower() == "username")
            {
                OleDbConnectionStringBuilder.Add("User ID", model.dbUsername);
                OleDbConnectionStringBuilder.Add("Password", model.dbPassword);
            }
            else
            {
                OleDbConnectionStringBuilder.Add("Integrated Security", "SSPI");
            }

            return OleDbConnectionStringBuilder.ConnectionString;
        }

        public int GetTotalRecords(string connectionString, string sqlCount, string sql)
        {
            int totalRecords = 0;

            try
            {
                using (OleDbConnection conn = new OleDbConnection(connectionString))
                {
                    conn.Open();

                    using (OleDbCommand command = new OleDbCommand(sqlCount, conn))
                    {
                        if (!sql.StartsWith("EXEC")) totalRecords = Math.Max(totalRecords, (int)command.ExecuteScalar());
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle exception (log, rethrow, etc.)
                throw new Exception($"Error executing OleDb query for total records: {ex.Message}", ex);
            }

            return totalRecords;
        }

        public DataTable ExecuteQuery(string connectionString, string sql)
        {
            DataTable dataTable = new DataTable();

            try
            {
                using (OleDbConnection conn = new OleDbConnection(connectionString))
                {
                    conn.Open();

                    using (OleDbCommand command = new OleDbCommand(sql, conn))
                    {
                        using (OleDbDataAdapter adapter = new OleDbDataAdapter(command))
                        {
                            adapter.Fill(dataTable);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle exception (log, rethrow, etc.)
                throw new Exception($"Error executing OleDb query: {ex.Message}", ex);
            }

            return dataTable;
        }
        public DataSet ExecuteDataSetQuery(string connectionString, string combinedSqls)
        {
            var dts = new DataSet();
            try
            {
                using (var conn = new OleDbConnection(connectionString))
                using (var cmd = new OleDbCommand(combinedSqls, conn))
                using (var adp = new OleDbDataAdapter(cmd))
                {
                    adp.Fill(dts);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error executing SQL query: {ex.Message}", ex);
            }
            return dts;
        }
    }



}
