﻿<%@ Page Title="" Language="C#" MasterPageFile="~/DotNetReport/ReportLayout.Master" AutoEventWireup="true" CodeBehind="Dashboard.aspx.cs" Inherits="ReportBuilder.WebForms.DotNetReport.Dashboard" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
     <link rel="stylesheet" href="//cdnjs.cloudflare.com/ajax/libs/gridstack.js/0.4.0/gridstack.min.css" />
    <link rel="stylesheet" href="//cdnjs.cloudflare.com/ajax/libs/gridstack.js/0.4.0/gridstack.min.css" />
    <style type="text/css">
        .report-chart { min-height: auto !important; }
        .expanded {
            position: fixed !important;
            padding: 0 !important;
            margin: 0 !important;
            top: 0 !important;
            left: 0 !important;
            width: 100% !important;
            height: 100% !important;
            z-index: 99993 !important;
        }
    </style>

</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="scripts" runat="server">
    <script type="text/javascript" src='//cdnjs.cloudflare.com/ajax/libs/gridstack.js/0.4.0/gridstack.min.js'></script>
    <script type="text/javascript" src='//cdnjs.cloudflare.com/ajax/libs/gridstack.js/0.4.0/gridstack.jQueryUI.min.js'></script>
    <script type="text/javascript">
        $(document).ready(function () {
            var reports = [];
            var dashboards = [];
            var queryParams = Object.fromEntries((new URLSearchParams(window.location.search)).entries());
            var adminMode = queryParams.adminMode == 'true' ? 'true' : 'false';
            var svc = "/DotNetReport/ReportService.asmx/";

            ajaxcall({ url: svc + 'GetDashboards', data: { adminMode: adminMode } }).done(function (dashboardData) {
                if (dashboardData.d) { dashboardData = dashboardData.d; }
                _.forEach(dashboardData, function (d) {
                    dashboards.push({ id: d.Id, name: d.Name, description: d.Description, selectedReports: d.SelectedReports, userId: d.UserId, userRoles: d.UserRoles, viewOnlyUserId: d.ViewOnlyUserId, viewOnlyUserRoles: d.ViewOnlyUserRoles });
                });

                var dashboardId = parseInt(queryParams.id || 0);
                if (!dashboardId && dashboards.length > 0) { dashboardId = dashboards[0].id; }

                ajaxcall({ url: svc + 'LoadSavedDashboard', data: { id: dashboardId || null, adminMode: adminMode } }).done(function (reportsData) {
                    if (reportsData.d) { reportsData = reportsData.d; }
                    _.forEach(reportsData, function (r) {
                        reports.push({ reportSql: r.ReportSql, reportId: r.ReportId, reportFilter: r.ReportFilter, connectKey: r.ConnectKey, x: r.X, y: r.Y, width: r.Width, height: r.Height });
                    });

                    ajaxcall({ url: svc + 'GetUsersAndRoles' }).done(function (data) {
                        if (data.d) { data = data.d; }

                        var vm = new dashboardViewModel({
                            runReportUrl: svc + "Report",
                            execReportUrl: svc + "RunReport",
                            reportWizard: $("#filter-panel"),
                            lookupListUrl: svc + "GetLookupList",
                            apiUrl: svc + "CallReportApi",
                            runReportApiUrl: svc + "RunReportApi",
                            reportMode: "execute",
                            reportWizard: $("#modal-reportbuilder"),
                            linkModal: $("#linkModal"),
                            fieldOptionsModal: $("#fieldOptionsModal"),
                            reports: reports,
                            dashboards: dashboards,
                            users: data.users,
                            userRoles: data.userRoles,
                            allowAdmin: data.allowAdminMode,
                            dataFilters: data.dataFilters,
                            dashboardId: dashboardId,
                            runExportUrl: svc,
                            printReportUrl: window.location.protocol + "//" + window.location.host + "/DotnetReport/ReportPrint.aspx",
                            getTimeZonesUrl: svc + "GetAllTimezones",
                            loadSavedDashbordUrl: svc + 'LoadSavedDashboard'
                        });

                        vm.init().done(function () {
                            ko.applyBindings(vm);
                            $(function () {
                                var options = {
                                    cellHeight: 80,
                                    verticalMargin: 10
                                };
                                $('.grid-stack').gridstack(options);
                                $('.grid-stack').on('change', function (event, items) {
                                    _.forEach(items, function (x) {
                                        vm.updatePosition(x);
                                    });
                                });
                                $('.grid-stack').on('resizestop', function (event, item) {
                                    var e = $(event.target).find('.report-chart');
                                    var d = $(event.target).find('table');
                                    if (e.length > 0 && d.length == 0) {
                                        e.height(item.size.height - e[0].offsetTop - 40);
                                        vm.drawChart();
                                    }
                                });
                            });

                            setTimeout(function () {
                                vm.drawChart();

                                var items = $('.grid-stack-item');
                                _.forEach(items, function (x) {
                                    var e = $(x).find('.report-chart');
                                    var d = $(x).find('table');
                                    if (e.length > 0 && x.clientHeight && d.length == 0) {
                                        e.height(x.clientHeight - e[0].offsetTop - 40);
                                    }
                                });

                                vm.drawChart();
                                var grid = $('.grid-stack').data("gridstack");
                                grid.disable();

                            }, 1000);
                        });

                        $(window).resize(function () {
                            vm.drawChart();
                        });
                    });
                });
            });
        });
    </script>
    
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">

<div data-bind="template: {name: 'admin-mode-template'}, visible: allowAdmin" style="display: none;"></div>

<div class="row" style="display: none" data-bind="visible: currentDashboard">
    <div class="col-4" data-bind="with: currentDashboard">
        <h2 title="Switch Dashboard">
            <select class="form-control select-as-text" title="Switch Dashboard" data-bind="select2: {minimumResultsForSearch: Infinity}, options: $parent.dashboards, optionsText: 'name', optionsValue: 'id', value: $parent.selectDashboard"></select>
        </h2>
        <p data-bind="text: description"></p>
    </div>
    <div class="col-4">
        &nbsp;
    </div>
</div>
<div class="clearfix"></div>

<div class="row">
    <div class="col d-flex align-items-center">
        <button class="btn btn-primary btn-sm me-2" data-bs-toggle="modal" data-bs-target="#add-dashboard-modal" title="Edit Dashboard Settings" data-bind="click: editDashboard">Edit this Dashboard</button>
        <button class="btn btn-primary btn-sm me-2" data-bs-toggle="modal" data-bs-target="#add-dashboard-modal" title="Add a New Dashboard" data-bind="click: newDashboard">Add a new Dashboard</button>
        <button class="btn btn-primary btn-sm" title="Refresh to Load All Reports" data-bind="click: RefreshAllReports"><span class="fa fa-refresh"></span> Refresh Dashboard</button>
        <div class="btn-group">
            <button type="button" class="btn btn-primary btn-sm me-2 dropdown-toggle" data-bs-toggle="dropdown" aria-haspopup="true" aria-expanded="false">
                <span class="fa fa-download"></span> Export <span class="caret"></span>
            </button>
            <ul class="dropdown-menu">
                <li class="dropdown-item">
                    <a href="#" data-bind="click: ExportAllPdfReports">
                        <span class="fa fa-file-pdf-o"></span> Pdf
                    </a>
                </li>
                <li class="dropdown-item">
                    <a href="#" data-bind="click: ExportAllExcelReports">
                        <span class="fa fa-file-excel-o"></span> Excel
                    </a>
                </li>
                <li class="dropdown-item">
                    <a href="#" data-bind="click: ExportAllWordReports">
                        <span class="fa fa-file-word-o"></span> Word
                    </a>
                </li>
            </ul>
        </div>
    </div>
    <div class="col-auto d-flex align-items-center">
        <div class="bubble-badge">
            Arrange Dashboard
            <div class="form-check form-switch ms-2">
                <input class="form-check-input" id="arrange-mode" type="checkbox" data-bind="checked: arrangeDashboard">
            </div>
        </div>
    </div>
</div>

<div class="padded-top"></div>

<div class="centered" style="display: none;" data-bind="visible: dashboards().length == 0 ">
    No Dashboards yet. Click below to Start<br />
    <button class="btn btn-lg btn-primary" data-bs-toggle="modal" data-bs-target="#add-dashboard-modal"><i class="fa fa-dashboard"></i> Create a New Dashboard</button>
</div>

<div class="modal modal-fullscreen" id="add-dashboard-modal" tabindex="-1" aria-labelledby="add-dashboard-modal-label" aria-hidden="true">
    <div class="modal-dialog" role="document">
        <div class="modal-content" data-bind="with: dashboard">
            <div class="modal-header">
                <h4 class="modal-title" id="add-dashboard-modal-label"><span data-bind="text: Id() ? 'Edit' : 'Add'"></span> Dashboard</h4>
                <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
            </div>
            <div class="modal-body">
                <div class="mb-4 card card-body">
                    <div class="row mb-3">
                        <label class="col-md-3 col-form-label">Name</label>
                        <div class="col-md-6">
                            <input class="form-control" data-val="true" data-val-required="Dashboard Name is required." type="text" data-bind="value: Name" placeholder="Dashboard Name, e.g., Sales, Accounting" id="add-dash-name" required />
                        </div>
                    </div>
                    <div class="row mb-3">
                        <label class="col-md-3 col-form-label">Description</label>
                        <div class="col-md-6">
                            <textarea class="form-control" data-bind="value: Description" placeholder="Optional Description for the Dashboard"></textarea>
                        </div>
                    </div>
                </div>

                <div class="mb-4 card card-body">
                    <div class="row align-items-center mb-3">
                        <div class="col-md-6">
                            <h5><i class="fa fa-paperclip"></i> Choose Reports for the Dashboard</h5>
                        </div>
                        <div class="col-md-6">
                            <input type="text" class="form-control" placeholder="Search Report by Name or Description..." data-bind="textInput: $parent.searchReports" />
                        </div>
                    </div>

                    <div data-bind="if: $parent.searchReports() && $parent.reportsInSearch().length==0">
                        <div class="card card-body">
                            No reports found matching your search.
                        </div>
                    </div>
                    <div data-bind="if: $parent.searchReports() &&  $parent.reportsInSearch().length>0">
                        <ul class="list-group" data-bind="foreach: $parent.reportsInSearch">
                            <li class="list-group-item">
                                <div class="checkbox">
                                    <label class="list-group-item-heading">
                                        <input type="checkbox" data-bind="checked: selected">
                                        <span class="fa" data-bind="css: {'fa-file': reportType=='List', 'fa-th-list': reportType=='Summary', 'fa-bar-chart': reportType=='Bar', 'fa-pie-chart': reportType=='Pie', 'fa-line-chart': reportType=='Line', 'fa-globe': reportType =='Map', 'fa-window-restore': reportType=='Treemap'}" style="font-size: 14pt; color: #808080"></span>
                                        <span data-bind="highlightedText: { text: reportName, highlight: $parent.searchReports, css: 'highlight' }"></span>
                                    </label>
                                </div>
                                <p class="list-group-item-text small" data-bind="text: reportDescription"></p>
                            </li>
                        </ul>
                    </div>

                    <div data-bind="if: !$parent.searchReports()" class="list-group mb-3">
                        <div data-bind="foreach: $parent.reportsAndFolders">
                            <div class="list-group-item">
                                <a role="button" class="d-flex justify-content-between align-items-center" data-bs-toggle="collapse" data-bind="attr: {href: '#folder-' + folderId }" style="text-decoration: none; font-weight: normal;">
                                    <div class="d-flex align-items-center">
                                        <i class="fa fa-folder me-2" style="color: #ffd800"></i>
                                        <span data-bind="text: folder"></span>
                                    </div>
                                    <i class="fa fa-chevron-down"></i>
                                </a>
                                <div class="collapse mt-2" data-bind="attr: {id: 'folder-' + folderId }">
                                    <ul class="list-group list-group-flush" data-bind="foreach: reports">
                                        <li class="list-group-item">
                                            <div class="form-check">
                                                <input class="form-check-input" type="checkbox" data-bind="checked: selected">
                                                <label class="form-check-label">
                                                    <i class="fa" data-bind="css: {'fa-file': reportType=='List', 'fa-th-list': reportType=='Summary', 'fa-bar-chart': reportType=='Bar', 'fa-pie-chart': reportType=='Pie', 'fa-line-chart': reportType=='Line', 'fa-globe': reportType=='Map', 'fa-window-restore': reportType=='Treemap'}" style="font-size: 14pt; color: #808080"></i>
                                                    <span data-bind="text: reportName"></span>
                                                </label>
                                            </div>
                                            <small class="text-muted" data-bind="text: reportDescription"></small>
                                        </li>
                                    </ul>
                                </div>
                            </div>
                        </div>
                    </div>

                </div>
                <div class="card card-body mb-3">
                    <h5><i class="fa fa-hourglass"></i>&nbsp;Choose Schedule</h5>
                    <div data-bind="template: {name: 'report-schedule'}"></div>
                </div>

                <div data-bind="visible: $parent.adminMode" class="card card-body mb-3">
                    <div data-bind="template: {name: 'manage-access-template'}"></div>
                </div>
            </div>
            <div class="modal-footer">
                <button type="button" class="btn btn-danger" data-bind="click: $root.deleteDashboard, visible: Id">Delete Dashboard</button>
                <button type="button" class="btn btn-primary" data-bind="click: $root.saveDashboard">Save Dashboard</button>
            </div>
        </div>
    </div>
</div>

<div class="clearfix"></div>
<div class="row">
    <div class="col-md-12">
        <div data-bind="template: {name: 'fly-filter-template'}"></div>
        <br />
    </div>
</div>

<div class="grid-stack" data-bind="visible: reports().length>0, foreach: reports" style="display: none;">
    <div class="grid-stack-item" data-bind="attr: {'data-gs-x': x, 'data-gs-y': y, 'data-gs-width': width, 'data-gs-height': height, 'data-gs-auto-position': true, 'data-gs-id': ReportID}">

        <div class="card" data-bind="attr: {class: 'card ' + panelStyle + ' grid-stack-item-content'}, css: { expanded: isExpanded }" style="overflow-y: hidden;">
            <div class="padded-div" style="padding-bottom: 0; margin-bottom: 0;">
                <div class="pull-left">
                    <button type="button" class="btn" data-bs-toggle="dropdown" aria-haspopup="false" aria-expanded="false">
                        <span class="fa fa-ellipsis-v"></span>
                    </button>
                    <ul class="dropdown-menu small" style="z-index: 1001;">
                        <li class="dropdown-item" data-bind="visible: FlyFilters().length> 0">
                            <a href="#" data-bind="click: toggleFlyFilters">
                                <span class="fa fa-filter"></span> Filter
                            </a>
                        </li>
                        <li class="dropdown-item" data-bind="visible: CanEdit">
                            <a href="#" data-bind="click: openReport">
                                <span class="fa fa-pencil"></span> Edit
                            </a>
                        </li>
                        <li class="dropdown-item">
                            <a href="#" data-bind="click: downloadExcel">
                                <span class="fa fa-file-excel-o"></span> Excel
                            </a>
                        </li>
                        <li class="dropdown-item">
                            <a href="#" data-bind="click: downloadPdf">
                                <span class="fa fa-file-pdf-o"></span> PDF
                            </a>
                        </li>
                        <li class="dropdown-item">
                            <a href="#" data-bind="click: downloadWord">
                                <span class="fa fa-file-word-o"></span> Word
                            </a>
                        </li>
                        <li class="dropdown-item">
                            <a data-bind="attr: {href: '/DotNetReport/Report?linkedreport=true&noparent=true&reportId=' + ReportID() }" target="_blank">
                                <span class="fa fa-file"></span> Report
                            </a>
                        </li>

                        <li class="dropdown-item">
                            <a href="#" data-bind="click: function() { $parent.removeReportFromDashboard(ReportID()); }">
                                <span class="fa fa-close"></span> Remove
                            </a>
                        </li>

                         <li class="dropdown-item">
                            <a href="#"  data-bind="click: function() { RefreshReport(ReportID()); }">
                                <span class="fa fa-refresh"></span> Refresh
                            </a>
                        </li>
                    </ul>
                </div>

                <h2 class="pull-left" data-bind="text: ReportName"></h2>
                <div class="pull-right">
                    <a class="btn btn-link" data-bind="click: toggleExpand"><span class="fa" data-bind="css: {'fa-expand': !isExpanded(), 'fa-minus': isExpanded() }, visible: ReportType() != 'Single'"></span></a>
                </div>
            </div>
            <div class="card-body list-overflow-auto" style="padding-top: 0; margin-top: 0;">               
                <p data-bind="html: ReportDescription, visible: ReportDescription"></p>
                <div data-bind="template: {name: 'fly-filter-template'}, visible: showFlyFilters"></div>
                <div data-bind="with: ReportResult" class="small">
                    <div data-bind="visible: !ReportData()">
                        <div class="report-spinner"></div>
                    </div>
                    <div data-bind="template: 'report-template', data: $data"></div>
                </div>
            </div>
            <div class="form-inline">
                <div class="small" data-bind="with: pager">
                    <div class="form-group pull-left total-records" data-bind="if: totalRecords()>1">
                        <span data-bind="text: 'Total Records: ' + totalRecords()"></span><br />
                    </div>
                    <div class="form-group pull-right" data-bind="if: pages()>1">
                        <div data-bind="template: 'pager-template', data: $data"></div>
                    </div>
                    <div class="clearfix"></div>
                </div>
            </div>
        </div>

    </div>
</div>

<!-- Report Builder -->
<div class="modal modal-fullscreen" id="modal-reportbuilder" role="dialog" aria-labelledby="myModalLabel" aria-hidden="true" style="padding-right: 0px !important;" data-bind="with: selectedReport">
    <div data-bind="template: {name: 'report-designer', data: $data}"></div>
</div>

<!-- Field Options Modal -->
<div class="modal" id="fieldOptionsModal" tabindex="-1" role="dialog" aria-hidden="true" data-bind="with: selectedReport">
    <div data-bind="template: {name: 'report-field-options', data: $data}"></div>
</div>

<!-- Link Edit Modal -->
<div class="modal" id="linkModal" tabindex="-1" role="dialog" aria-hidden="true" data-bind="with: selectedReport">
    <div data-bind="template: {name: 'report-link-edit', data: $data}"></div>
</div>
</asp:Content>