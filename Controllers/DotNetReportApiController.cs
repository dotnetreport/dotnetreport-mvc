﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using ReportBuilder.Web.Models;
using System.Data;
using System.Data.OleDb;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Web;

namespace ReportBuilder.Web.Controllers
{
    //[Authorize]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class DotNetReportApiController : ControllerBase
    {
        private readonly IConfigurationRoot _configuration;
        private readonly static string _configFileName = "appsettings.dotnetreport.json";

        public DotNetReportApiController()
        {
            var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            _configuration = builder.Build();
        }

        private DotNetReportSettings GetSettings()
        {
            var settings = new DotNetReportSettings
            {
                ApiUrl = _configuration.GetValue<string>("dotNetReport:apiUrl"),
                AccountApiToken = _configuration.GetValue<string>("dotNetReport:accountApiToken"), // Your Account Api Token from your http://dotnetreport.com Account
                DataConnectApiToken = _configuration.GetValue<string>("dotNetReport:dataconnectApiToken") // Your Data Connect Api Token from your http://dotnetreport.com Account            };
            };

            // Populate the values below using your Application Roles/Claims if applicable
            settings.ClientId = "";  // You can pass your multi-tenant client id here to track their reports and folders
            settings.UserId = ""; // You can pass your current authenticated user id here to track their reports and folders            
            settings.UserName = "";
            settings.CurrentUserRole = new List<string>(); // Populate your current authenticated user's roles

            settings.Users = new List<dynamic>(); // Populate all your application's user, ex  { "Jane", "John" } or { new { id="1", text="Jane" }, new { id="2", text="John" }}
            settings.UserRoles = new List<string>(); // Populate all your application's user roles, ex  { "Admin", "Normal" }       
            settings.CanUseAdminMode = true; // Set to true only if current user can use Admin mode to setup reports, dashboard and schema
            settings.DataFilters = new { }; // add global data filters to apply as needed https://dotnetreport.com/kb/docs/advance-topics/global-filters/

            return settings;
        }

        public class GetLookupListParameters
        {
            public string lookupSql { get; set; }
            public string connectKey { get; set; }
        }

        [HttpPost]
        public IActionResult GetLookupList(GetLookupListParameters model)
        {
            string lookupSql = model.lookupSql;
            string connectKey = model.connectKey;

            var sql = DotNetReportHelper.Decrypt(lookupSql);

            // Uncomment if you want to restrict max records returned
            sql = sql.Replace("SELECT ", "SELECT TOP 500 ");

            var json = new StringBuilder();
            var dt = new DataTable();
            using (var conn = new OleDbConnection(DotNetReportHelper.GetConnectionString(connectKey)))
            {
                conn.Open();
                var command = new OleDbCommand(sql, conn);
                var adapter = new OleDbDataAdapter(command);

                adapter.Fill(dt);
            }

            var data = new List<object>();
            foreach (DataRow dr in dt.Rows)
            {
                data.Add(new { id = dr[0], text = dr[1] });
            }

            return Ok(data);
        }

        public class PostReportApiCallMode
        {
            public string method { get; set; }
            public string headerJson { get; set; }
            public bool useReportHeader { get; set; }

        }

        [AllowAnonymous]
        public async Task<IActionResult> CallReportApiUnAuth(string method, string model)
        {
            var settings = new DotNetReportSettings
            {
                ApiUrl = _configuration.GetValue<string>("dotNetReport:apiUrl"),
                AccountApiToken = _configuration.GetValue<string>("dotNetReport:accountApiToken"), // Your Account Api Token from your http://dotnetreport.com Account
                DataConnectApiToken = _configuration.GetValue<string>("dotNetReport:dataconnectApiToken") // Your Data Connect Api Token from your http://dotnetreport.com Account            };
            };

            return await ExecuteCallReportApi(method, model, settings);
        }

        [HttpPost]
        public async Task<IActionResult> PostReportApi(PostReportApiCallMode data)
        {
            string method = data.method;
            return await CallReportApi(method, JsonSerializer.Serialize(data));
        }

        [HttpPost]
        public async Task<IActionResult> RunReportApi(DotNetReportApiCall data)
        {
            return await CallReportApi(data.Method, JsonSerializer.Serialize(data));
        }

        [HttpGet]
        public async Task<IActionResult> CallReportApi(string? method, string? model)
        {
            return string.IsNullOrEmpty(method) || string.IsNullOrEmpty(model) ? Ok() : await ExecuteCallReportApi(method, model);
        }

        private async Task<IActionResult> ExecuteCallReportApi(string method, string model, DotNetReportSettings settings = null)
        {
            using (var client = new HttpClient())
            {
                settings = settings ?? GetSettings();
                var keyvalues = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("account", settings.AccountApiToken),
                    new KeyValuePair<string, string>("dataConnect", settings.DataConnectApiToken),
                    new KeyValuePair<string, string>("clientId", settings.ClientId),
                    new KeyValuePair<string, string>("userId", settings.UserId),
                    new KeyValuePair<string, string>("userIdForSchedule", settings.UserIdForSchedule),
                    new KeyValuePair<string, string>("userRole", String.Join(",", settings.CurrentUserRole))
                };

                var data = JsonSerializer.Deserialize<Dictionary<string, dynamic>>(model);
                foreach (var key in data.Keys)
                {
                    if ((key != "adminMode" || (key == "adminMode" && settings.CanUseAdminMode)) && data[key] is not null)
                    {
                        keyvalues.Add(new KeyValuePair<string, string>(key, data[key].ToString()));
                    }
                }

                var content = new FormUrlEncodedContent(keyvalues);
                var response = await client.PostAsync(new Uri(settings.ApiUrl + method), content);
                var stringContent = await response.Content.ReadAsStringAsync();

                Response.StatusCode = (int)response.StatusCode;
                var result = JsonSerializer.Deserialize<dynamic>(stringContent);
                if (stringContent == "\"\"") result = new { };
                return Response.StatusCode == 200 ? Ok(result) : BadRequest(result);
            }

        }

        public class RunReportParameters
        {
            public string reportSql { get; set; }
            public string connectKey { get; set; }
            public string reportType { get; set; }
            public int pageNumber { get; set; }
            public int pageSize { get; set; }
            public string sortBy { get; set; }
            public bool desc { get; set; }
            public string ReportSeries { get; set; }
        }

        [HttpPost]
        public IActionResult RunReport(RunReportParameters data)
        {
            string reportSql = data.reportSql;
            string connectKey = data.connectKey;
            string reportType = data.reportType;
            int pageNumber = data.pageNumber;
            int pageSize = data.pageSize;
            string sortBy = data.sortBy;
            bool desc = data.desc;
            string reportSeries = data.ReportSeries;

            var sql = "";
            var sqlCount = "";
            int totalRecords = 0;

            try
            {
                if (string.IsNullOrEmpty(reportSql))
                {
                    throw new Exception("Query not found");
                }
                var allSqls = reportSql.Split(new string[] { "%2C" }, StringSplitOptions.RemoveEmptyEntries);
                var dtPaged = new DataTable();
                var dtCols = 0;

                List<string> fields = new List<string>();
                List<string> sqlFields = new List<string>();
                for (int i = 0; i < allSqls.Length; i++)
                {
                    sql = DotNetReportHelper.Decrypt(HttpUtility.HtmlDecode(allSqls[i]));
                    if (!sql.StartsWith("EXEC"))
                    {
                        var fromIndex = DotNetReportHelper.FindFromIndex(sql);
                        sqlFields = DotNetReportHelper.SplitSqlColumns(sql);

                        var sqlFrom = $"SELECT {sqlFields[0]} {sql.Substring(fromIndex)}";
                        sqlCount = $"SELECT COUNT(*) FROM ({(sqlFrom.Contains("ORDER BY") ? sqlFrom.Substring(0, sqlFrom.IndexOf("ORDER BY")) : sqlFrom)}) as countQry";

                        if (!String.IsNullOrEmpty(sortBy))
                        {
                            if (sortBy.StartsWith("DATENAME(MONTH, "))
                            {
                                sortBy = sortBy.Replace("DATENAME(MONTH, ", "MONTH(");
                            }
                            if (sortBy.StartsWith("MONTH(") && sortBy.Contains(")) +") && sql.Contains("Group By"))
                            {
                                sortBy = sortBy.Replace("MONTH(", "CONVERT(VARCHAR(3), DATENAME(MONTH, ");
                            }
                            if (!sql.Contains("ORDER BY"))
                            {
                                sql = sql + "ORDER BY " + sortBy + (desc ? " DESC" : "");
                            }
                            else
                            {
                                sql = sql.Substring(0, sql.IndexOf("ORDER BY")) + "ORDER BY " + sortBy + (desc ? " DESC" : "");
                            }
                        }

                        if (sql.Contains("ORDER BY") && !sql.Contains(" TOP "))
                            sql = sql + $" OFFSET {(pageNumber - 1) * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY";

                        if (sql.Contains("__jsonc__"))
                            sql = sql.Replace("__jsonc__", "");
                    }

                    // Execute sql
                    var connect = DotNetSetupController.GetConnection();
                    var dbConfig = GetDbConnectionSettings(connect.AccountApiKey, connect.DatabaseApiKey);
                    if (dbConfig == null)
                    {
                        throw new Exception("Data Connection settings not found");
                    }

                    var dbtype = dbConfig["DatabaseType"].ToString();
                    string connectionString = dbConfig["ConnectionString"].ToString();
                     var databaseConnection = DatabaseConnectionFactory.GetConnection(dbtype);

                    var dtPagedRun = new DataTable();

                    totalRecords = databaseConnection.GetTotalRecords(connectionString, sqlCount, sql);
                    dtPagedRun = databaseConnection.ExecuteQuery(connectionString, sql);

                    if (sql.StartsWith("EXEC"))
                    {
                        totalRecords = dtPagedRun.Rows.Count;
                        if (dtPagedRun.Rows.Count > 0)
                            dtPagedRun = dtPagedRun.AsEnumerable().Skip((pageNumber - 1) * pageSize).Take(pageSize).CopyToDataTable();
                    }
                    if (!sqlFields.Any())
                    {
                        foreach (DataColumn c in dtPagedRun.Columns) { sqlFields.Add($"{c.ColumnName} AS {c.ColumnName}"); }
                    }

                    string[] series = { };
                    if (i == 0)
                    {
                        dtPaged = dtPagedRun;
                        dtCols = dtPagedRun.Columns.Count;
                        fields.AddRange(sqlFields);
                    }
                    else if (i > 0)
                    {
                        // merge in to dt
                        if (!string.IsNullOrEmpty(reportSeries))
                            series = reportSeries.Split(new string[] { "%2C", "," }, StringSplitOptions.RemoveEmptyEntries);

                        var j = 1;
                        while (j < dtPagedRun.Columns.Count)
                        {
                            var col = dtPagedRun.Columns[j++];
                            dtPaged.Columns.Add($"{col.ColumnName} ({series[i - 1]})", col.DataType);
                            fields.Add(sqlFields[j - 1]);
                        }

                        foreach (DataRow dr in dtPaged.Rows)
                        {
                            DataRow match = null;
                            if (fields[0].ToUpper().StartsWith("CONVERT(VARCHAR(10)")) // group by day
                            {
                                match = dtPagedRun.AsEnumerable().Where(r => !string.IsNullOrEmpty(r.Field<string>(0)) && !string.IsNullOrEmpty((string)dr[0]) && Convert.ToDateTime(r.Field<string>(0)).Day == Convert.ToDateTime((string)dr[0]).Day).FirstOrDefault();
                            }
                            else if (fields[0].ToUpper().StartsWith("CONVERT(VARCHAR(3)")) // group by month/year
                            {

                            }
                            else
                            {
                                match = dtPagedRun.AsEnumerable().Where(r => r.Field<string>(0) == (string)dr[0]).FirstOrDefault();
                            }
                            if (match != null)
                            {
                                j = 1;
                                while (j < dtCols)
                                {
                                    dr[j + i + dtCols - 2] = match[j];
                                    j++;
                                }
                            }
                        }
                    }
                }                

                sql = DotNetReportHelper.Decrypt(HttpUtility.HtmlDecode(allSqls[0]));
                var model = new DotNetReportResultModel
                {
                    ReportData = DotNetReportHelper.DataTableToDotNetReportDataModel(dtPaged, fields),
                    Warnings = GetWarnings(sql),
                    ReportSql = sql,
                    ReportDebug = Request.Host.Host.Contains("localhost"),
                    Pager = new DotNetReportPagerModel
                    {
                        CurrentPage = pageNumber,
                        PageSize = pageSize,
                        TotalRecords = totalRecords,
                        TotalPages = (int)(totalRecords == pageSize ? (totalRecords / pageSize) : (totalRecords / pageSize) + 1)
                    }
                };

                return new JsonResult(model, new JsonSerializerOptions() { PropertyNamingPolicy = null });

            }

            catch (Exception ex)
            {
                var model = new DotNetReportResultModel
                {
                    ReportData = new DotNetReportDataModel(),
                    ReportSql = sql,
                    HasError = true,
                    Exception = ex.Message,
                    ReportDebug = Request.Host.Host.Contains("localhost"),
                };

                return new JsonResult(model, new JsonSerializerOptions() { PropertyNamingPolicy = null });
            }
        }

        [HttpGet]
        public async Task<IActionResult> RunReportLink(int reportId, int? filterId = null, string filterValue = "", bool adminMode = false)
        {
            var model = new DotNetReportModel();
            var settings = GetSettings();

            using (var client = new HttpClient())
            {
                var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("account", settings.AccountApiToken),
                    new KeyValuePair<string, string>("dataConnect", settings.DataConnectApiToken),
                    new KeyValuePair<string, string>("clientId", settings.ClientId),
                    new KeyValuePair<string, string>("userId", settings.UserId),
                    new KeyValuePair<string, string>("userRole", String.Join(",", settings.CurrentUserRole)),
                    new KeyValuePair<string, string>("reportId", reportId.ToString()),
                    new KeyValuePair<string, string>("filterId", filterId.HasValue ? filterId.ToString() : ""),
                    new KeyValuePair<string, string>("filterValue", filterValue.ToString()),
                    new KeyValuePair<string, string>("adminMode", adminMode.ToString()),
                    new KeyValuePair<string, string>("dataFilters", JsonSerializer.Serialize(settings.DataFilters))
                });

                var response = await client.PostAsync(new Uri(settings.ApiUrl + $"/ReportApi/RunLinkedReport"), content);
                var stringContent = await response.Content.ReadAsStringAsync();

                model = JsonSerializer.Deserialize<DotNetReportModel>(stringContent); 

            }

            return new JsonResult(model, new JsonSerializerOptions() { PropertyNamingPolicy = null });
        }


        [HttpGet]
        public async Task<IActionResult> GetDashboards(bool adminMode = false)
        {
            var model = await GetDashboardsData(adminMode);
            return Ok(model);
        }


        [HttpGet]
        public async Task<IActionResult> LoadSavedDashboard(int? id = null, bool adminMode = false)
        {
            var settings = GetSettings();
            var model = new List<DotNetDasboardReportModel>();
            var dashboards = (await GetDashboardsData(adminMode));
            if (!id.HasValue && dashboards.Count > 0)
            {
                id = ((dynamic)dashboards.First()).Id;
            }

            using (var client = new HttpClient())
            {
                var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("account", settings.AccountApiToken),
                    new KeyValuePair<string, string>("dataConnect", settings.DataConnectApiToken),
                    new KeyValuePair<string, string>("clientId", settings.ClientId),
                    new KeyValuePair<string, string>("userId", settings.UserId),
                    new KeyValuePair<string, string>("userRole", String.Join(",", settings.CurrentUserRole)),
                    new KeyValuePair<string, string>("id", id.HasValue ? id.Value.ToString() : "0"),
                    new KeyValuePair<string, string>("adminMode", adminMode.ToString()),
                    new KeyValuePair<string, string>("dataFilters", JsonSerializer.Serialize(settings.DataFilters))
                });

                var response = await client.PostAsync(new Uri(settings.ApiUrl + $"/ReportApi/LoadSavedDashboard"), content);
                var stringContent = await response.Content.ReadAsStringAsync();

                model = JsonSerializer.Deserialize<List<DotNetDasboardReportModel>>(stringContent);
            }

            return new JsonResult(model, new JsonSerializerOptions() { PropertyNamingPolicy = null });
        }

        private async Task<dynamic> GetDashboardsData(bool adminMode = false)
        {
            var settings = GetSettings();
            if (string.IsNullOrEmpty(settings.AccountApiToken))
            {
                return new { noAccount = true };
            }

            using (var client = new HttpClient())
            {
                var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("account", settings.AccountApiToken),
                    new KeyValuePair<string, string>("dataConnect", settings.DataConnectApiToken),
                    new KeyValuePair<string, string>("clientId", settings.ClientId),
                    new KeyValuePair<string, string>("userId", settings.UserId),
                    new KeyValuePair<string, string>("userRole", String.Join(",", settings.CurrentUserRole)),
                    new KeyValuePair<string, string>("adminMode", adminMode.ToString()),
                });

                var response = await client.PostAsync(new Uri(settings.ApiUrl + $"/ReportApi/GetDashboards"), content);
                var stringContent = await response.Content.ReadAsStringAsync();

                var model = JsonSerializer.Deserialize<dynamic>(stringContent);
                return model;
            }
        }

        [HttpGet]
        public IActionResult GetUsersAndRoles()
        {
            var settings = GetSettings();
            return Ok(new
            {
                noAccount = string.IsNullOrEmpty(settings.AccountApiToken) || settings.AccountApiToken == "Your Public Account Api Token",
                users = settings.CanUseAdminMode ? settings.Users : new List<dynamic>(),
                userRoles = settings.CanUseAdminMode ? settings.UserRoles : new List<string>(),
                currentUserId = settings.UserId,
                currentUserRoles = settings.UserRoles,
                currentUserName = settings.UserName,
                allowAdminMode = settings.CanUseAdminMode,
                userIdForSchedule = settings.UserIdForSchedule,
                dataFilters = settings.DataFilters,
                clientId = settings.ClientId
            });
        }

        [HttpPost]
        public async Task<IActionResult> GetSchemaFromSql([FromBody] SchemaFromSqlCall data)
        {
            try
            {
                var table = new TableViewModel
                {
                    AllowedRoles = new List<string>(),
                    Columns = new List<ColumnViewModel>(),
                    CustomTable = true,
                    Selected = true
                };

                table.CustomTableSql = data.value;

                var connString = await DotNetSetupController.GetConnectionString(DotNetSetupController.GetConnection(data.dataConnectKey));
                using (OleDbConnection conn = new OleDbConnection(connString))
                {
                    // open the connection to the database 
                    conn.Open();
                    OleDbCommand cmd = new OleDbCommand(data.value, conn);
                    cmd.CommandType = CommandType.Text;
                    using (OleDbDataReader reader = cmd.ExecuteReader())
                    {
                        // Get the column metadata using schema.ini file
                        DataTable schemaTable = new DataTable();
                        schemaTable = reader.GetSchemaTable();
                        var idx = 0;

                        foreach (DataRow dr in schemaTable.Rows)
                        {
                            var column = new ColumnViewModel
                            {
                                ColumnName = dr["ColumnName"].ToString(),
                                DisplayName = dr["ColumnName"].ToString(),
                                PrimaryKey = dr["ColumnName"].ToString().ToLower().EndsWith("id") && idx == 0,
                                DisplayOrder = idx,
                                FieldType = DotNetSetupController.ConvertToJetDataType((int)dr["ProviderType"]).ToString(),
                                AllowedRoles = new List<string>(),
                                Selected = true
                            };

                            idx++;
                            table.Columns.Add(column);
                        }
                        table.Columns = table.Columns.OrderBy(x => x.DisplayOrder).ToList();
                    }

                    return new JsonResult(table, new JsonSerializerOptions() { PropertyNamingPolicy = null });
                }
            }
            catch (Exception ex)
            {
                return new JsonResult(new { errorMessage = ex.Message }, new JsonSerializerOptions() { PropertyNamingPolicy = null });
            }
        }

        private string GetWarnings(string sql)
        {
            var warning = "";
            if (sql.ToLower().Contains("cross join"))
            {
                warning += "Some data used in this report have relations that are not setup properly, so data might duplicate incorrectly.<br/>";
            }

            return warning;
        }

        //[Authorize(Roles="Administrator")]
        [HttpGet]
        public async Task<IActionResult> LoadSetupSchema(string? databaseApiKey = "", bool onlyApi = false)
        {
            try
            {
                var settings = GetSettings();

                if (string.IsNullOrEmpty(settings.AccountApiToken))
                {
                    return new JsonResult(new { noAccount = true }, new JsonSerializerOptions() { PropertyNamingPolicy = null });
                }

                if (!settings.CanUseAdminMode)
                {
                    throw new Exception("Not Authorized to access this Resource");
                }

            var connect = DotNetSetupController.GetConnection(databaseApiKey);
            var tables = new List<TableViewModel>();
            var procedures = new List<TableViewModel>();
            //tables.AddRange(await DotNetSetupController.GetTables("TABLE", connect.AccountApiKey, connect.DatabaseApiKey, onlyApi));
            //tables.AddRange(await DotNetSetupController.GetTables("VIEW", connect.AccountApiKey, connect.DatabaseApiKey, onlyApi));
            //procedures.AddRange(await DotNetSetupController.GetApiProcs(connect.AccountApiKey, connect.DatabaseApiKey));
            var dbConfig = GetDbConnectionSettings(connect.AccountApiKey, connect.DatabaseApiKey) ?? new JObject();
            var model = new ManageViewModel
            {
                ApiUrl = connect.ApiUrl,
                AccountApiKey = connect.AccountApiKey,
                DatabaseApiKey = connect.DatabaseApiKey,
                Tables = tables,
                Procedures = procedures,
                DbConfig = dbConfig.ToObject<Dictionary<string, string>>(),
                UserAndRolesConfig = new UserRolesConfig { RequireLogin = true ,UserRolesSource=true,UsersSource=true}
            };

                return new JsonResult(model, new JsonSerializerOptions() { PropertyNamingPolicy = null });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { Message = ex.Message }, new JsonSerializerOptions() { PropertyNamingPolicy = null }) { StatusCode = (int)HttpStatusCode.InternalServerError };
            }
        }


        public static dynamic  GetDbConnectionSettings(string account, string dataConnect)
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
            if (dotNetReportSection != null)
            {
                var defaultConfig = dotNetReportSection["ConnectionString"];
                var dataConnectSection = dotNetReportSection[dataConnect] as JObject;
                if (dataConnectSection != null)
                {
                    return dataConnectSection.ToObject<dynamic>();
                }
            }
        
            return null;
        }

        public class UserModel
        {
            public string account { get; set; }
            public string dataConnect { get; set; }
            public string UserName { get; set; }
            public string Email { get; set; }
            public string Password { get; set; }
            public string RoleName { get; set; }
            public string UserId { get; set; }
            public string RoleId { get; set; }

        }
        public class UpdateDbConnectionModel
        {
            public string account { get; set; }
            public string dataConnect { get; set; }
            public string dbType { get; set; }
            public string connectionType { get; set; }
            public string connectionKey { get; set; }
            public string connectionString { get; set; }
            public string dbServer { get; set; }
            public string dbPort { get; set; }
            public string dbName { get; set; }
            public string dbAuthType { get; set; }
            public string dbUsername { get; set; }
            public string dbPassword { get; set; }
            public bool isDefault { get; set; }
            public bool testOnly { get; set; }
        }

        //[Authorize(Roles="Administrator")]
        [HttpPost]
        public async Task<IActionResult> UpdateDbConnection(UpdateDbConnectionModel model)
        {
            try
            {
                var settings = GetSettings();
                if (!settings.CanUseAdminMode)
                {
                    throw new Exception("Not Authorized to access this Resource");
                }
                // Use dependency injection to get the appropriate implementation based on the database type
                var  databaseConnection = DatabaseConnectionFactory.GetConnection(model.dbType);

                var connectionString = "";
                if (model.connectionType=="Build")
                {
                    connectionString = databaseConnection.CreateConnection(model);
                }
                else
                {
                    connectionString = DotNetReportHelper.GetConnectionString(model.connectionKey);
                    //For Replacing OLEDB Provider to Empty
                    connectionString = connectionString.Replace("Provider=sqloledb;", "");
                   
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
                    System.IO.File.WriteAllText(_configFilePath, config.ToString());

                    // Update to Account as well
                    var result = await ExecuteCallReportApi("UpdateDataConnection", JsonSerializer.Serialize(new
                    {
                        model.connectionKey,
                        model.dbType,
                    }));
                }

                return new JsonResult(new
                {
                    success = true,
                    message = $"Connection Settings {(model.testOnly ? "Tested" : "Saved")} Successfully"
                }, new JsonSerializerOptions() { PropertyNamingPolicy = null });
            }
            catch(Exception ex)
            {

                return new JsonResult(new
                {
                    success = false,
                    message = ex.Message
                }, new JsonSerializerOptions() { PropertyNamingPolicy = null });

            }
        }

        public class SearchProcCall { 
            public string value { get; set; } 
            public string accountKey { get; set; } 
            public string dataConnectKey { get; set; } 
        }

        public class SchemaFromSqlCall : SearchProcCall
        {
        }

        [HttpPost]
        public async Task<IActionResult> SearchProcedure([FromBody] SearchProcCall data)
        {
            string value = data.value; string accountKey = data.accountKey; string dataConnectKey = data.dataConnectKey;
            return new JsonResult(await GetSearchProcedure(value, accountKey, dataConnectKey), new JsonSerializerOptions() { PropertyNamingPolicy = null });
        }

        private async Task<List<TableViewModel>> GetSearchProcedure(string value = null, string accountKey = null, string dataConnectKey = null)
        {
            var tables = new List<TableViewModel>();
            var connString = await DotNetSetupController.GetConnectionString(DotNetSetupController.GetConnection(dataConnectKey));
            using (OleDbConnection conn = new OleDbConnection(connString))
            {
                // open the connection to the database 
                conn.Open();
                string spQuery = "SELECT ROUTINE_NAME, ROUTINE_DEFINITION, ROUTINE_SCHEMA FROM INFORMATION_SCHEMA.ROUTINES WHERE ROUTINE_DEFINITION LIKE '%" + value + "%' AND ROUTINE_TYPE = 'PROCEDURE'";
                OleDbCommand cmd = new OleDbCommand(spQuery, conn);
                cmd.CommandType = CommandType.Text;
                DataTable dtProcedures = new DataTable();
                dtProcedures.Load(cmd.ExecuteReader());
                int count = 1;
                foreach (DataRow dr in dtProcedures.Rows)
                {
                    var procName = dr["ROUTINE_NAME"].ToString();
                    var procSchema = dr["ROUTINE_SCHEMA"].ToString();
                    cmd = new OleDbCommand(procName, conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    // Get the parameters.
                    OleDbCommandBuilder.DeriveParameters(cmd);
                    List<ParameterViewModel> parameterViewModels = new List<ParameterViewModel>();
                    foreach (OleDbParameter param in cmd.Parameters)
                    {
                        if (param.Direction == ParameterDirection.Input)
                        {
                            var parameter = new ParameterViewModel
                            {
                                ParameterName = param.ParameterName,
                                DisplayName = param.ParameterName,
                                ParameterValue = param.Value != null ? param.Value.ToString() : "",
                                ParamterDataTypeOleDbTypeInteger = Convert.ToInt32(param.OleDbType),
                                ParamterDataTypeOleDbType = param.OleDbType,
                                ParameterDataTypeString = DotNetSetupController.GetType(DotNetSetupController.ConvertToJetDataType(Convert.ToInt32(param.OleDbType))).Name
                            };
                            if (parameter.ParameterDataTypeString.StartsWith("Int")) parameter.ParameterDataTypeString = "Int";
                            parameterViewModels.Add(parameter);
                        }
                    }
                    DataTable dt = new DataTable();
                    cmd = new OleDbCommand($"[{procSchema}].[{procName}]", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    foreach (var data in parameterViewModels)
                    {
                        cmd.Parameters.Add(new OleDbParameter { Value = DBNull.Value, ParameterName = data.ParameterName, Direction = ParameterDirection.Input, IsNullable = true });
                    }
                    OleDbDataReader reader = cmd.ExecuteReader();
                    dt = reader.GetSchemaTable();

                    if (dt == null) continue;

                    // Store the table names in the class scoped array list of table names
                    List<ColumnViewModel> columnViewModels = new List<ColumnViewModel>();
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        var column = new ColumnViewModel
                        {
                            ColumnName = dt.Rows[i].ItemArray[0].ToString(),
                            DisplayName = dt.Rows[i].ItemArray[0].ToString(),
                            FieldType = DotNetSetupController.ConvertToJetDataType((int)dt.Rows[i]["ProviderType"]).ToString()
                        };
                        columnViewModels.Add(column);
                    }
                    tables.Add(new TableViewModel
                    {
                        TableName = procName,
                        SchemaName = dr["ROUTINE_SCHEMA"].ToString(),
                        DisplayName = procName,
                        Parameters = parameterViewModels,
                        Columns = columnViewModels
                    });
                    count++;
                }
                conn.Close();
                conn.Dispose();
            }
            return tables;
        }

        private async Task<DataTable> GetStoreProcedureResult(TableViewModel model, string accountKey = null, string dataConnectKey = null)
        {
            DataTable dt = new DataTable();
            var connString = await DotNetSetupController.GetConnectionString(DotNetSetupController.GetConnection(dataConnectKey));
            using (OleDbConnection conn = new OleDbConnection(connString))
            {
                // open the connection to the database 
                conn.Open();
                OleDbCommand cmd = new OleDbCommand(model.TableName, conn);
                cmd.CommandType = CommandType.StoredProcedure;
                foreach (var para in model.Parameters)
                {
                    if (string.IsNullOrEmpty(para.ParameterValue))
                    {
                        if (para.ParamterDataTypeOleDbType == OleDbType.DBTimeStamp || para.ParamterDataTypeOleDbType == OleDbType.DBDate)
                        {
                            para.ParameterValue = DateTime.Now.ToShortDateString();
                        }
                    }
                    cmd.Parameters.AddWithValue("@" + para.ParameterName, para.ParameterValue);
                    //cmd.Parameters.Add(new OleDbParameter { 
                    //    Value =  string.IsNullOrEmpty(para.ParameterValue) ? DBNull.Value : (object)para.ParameterValue , 
                    //    ParameterName = para.ParameterName, 
                    //    Direction = ParameterDirection.Input, 
                    //    IsNullable = true });
                }
                dt.Load(cmd.ExecuteReader());
                conn.Close();
                conn.Dispose();
            }
            return dt;
        }

    }

}