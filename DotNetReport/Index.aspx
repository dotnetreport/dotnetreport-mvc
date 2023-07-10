﻿<%@ Page Title="" Language="C#" MasterPageFile="~/DotNetReport/ReportLayout.Master" AutoEventWireup="true" CodeBehind="Index.aspx.cs" Inherits="ReportBuilder.WebForms.DotNetReport.Index" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="scripts" runat="server">
<!--
The html and JavaScript code below is related to presentation for the Report Builder. You don't have to change it, unless you intentionally want to
change something in the Report Builder's behavior in your Application.

Its Recommended you use it as is, and only change styling as needed to match your application. You will be responsible for managing and maintaining any changes.
-->
    <script type="text/javascript">

        var toastr = toastr || { error: function (msg) { window.alert(msg); }, success: function (msg) { window.alert(msg); } };

        var queryParams = Object.fromEntries((new URLSearchParams(window.location.search)).entries());

        $(document).ready(function () {
            ajaxcall({ url: '/DotNetReport/ReportService.asmx/GetUsersAndRoles', type: 'POST' }).done(function (data) {
                if (data.d) data = data.d;
                var svc = "/DotNetReport/ReportService.asmx/";
                var vm = new reportViewModel({
                    runReportUrl: "/DotNetReport/Report.aspx",
                    reportWizard: $("#modal-reportbuilder"),
                    linkModal: $("#linkModal"),
                    reportHeader: "report-header",
                    fieldOptionsModal: $("#fieldOptionsModal"),
                    lookupListUrl: svc + "GetLookupList",
                    apiUrl: svc + "CallReportApi",
                    runReportApiUrl: svc + "RunReportApi",
                    getUsersAndRolesUrl: svc + "GetUsersAndRoles",
                    reportId: queryParams.reportId || 0,
                    userSettings: data,
                    dataFilters: data.dataFilters
                });

                vm.init(queryParams.folderid || 0, data.noAccount);
                ko.applyBindings(vm);
            });
        });

    </script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">

<div class="container-fluid">
    <div class="row">
        <div class="col-md-6">
            <p>
            </p>
        </div>
        <div class="col-md-6">
            <div class="pull-right">
            <a href="/DotnetReport/Dashboard.aspx")">View Dashboard</a> | Learn how to <a href="https://dotnetreport.com/getting-started-with-dotnet-report/" target="_blank">Integrate in your App here</a>.
            </div>
        </div>
    </div>

    <div data-bind="template: {name: 'admin-mode-template'}, visible: allowAdmin" style="display: none;"></div>

    <!--
        The markup code below is related to presentation. You don't have to change it, unless you
        intentionally want to change something in the Report Builder's behavior in your Application.
        It's Recommended you use it as is. You will be responsible for managing and maintaining any changes.
    -->
    <!-- Folders/Report List -->
    <div id="report-start" data-bind="if: ReportMode() == 'start' || ReportMode() == 'generate'">
        <div class="card folder-panel">
            <div class="card-header">
                <nav class="navbar navbar-expand-lg navbar-light bg-light">
                    <a class="navbar-brand" href="#" data-bind="click: function() {SelectedFolder(null); designingHeader(false); searchReports(''); $('#search-report').val([]).trigger('change'); }">Manage Reports</a>
                    <button class="navbar-toggler" type="button" data-toggle="collapse" data-target="#navbarSupportedContent" aria-controls="navbarSupportedContent" aria-expanded="false" aria-label="Toggle navigation">
                        <span class="navbar-toggler-icon"></span>
                    </button>

                    <div class="collapse navbar-collapse" id="navbarSupportedContent">
                        <ul class="navbar-nav mr-auto">

                            <li class="nav-item active" data-bind="visible: CanSaveReports() || adminMode()">
                                <a href="#" class="nav-link" data-bind="click: createNewReport" data-toggle="modal" data-target="#modal-reportbuilder">
                                    <span class="fa fa-plus"></span> Create a New Report
                                </a>
                            </li>

                            <li class="nav-item" data-bind="visible: CanManageFolders() || adminMode()">
                                <a href="#" class="nav-link" data-bind="click: ManageFolder.newFolder">
                                    <span class="fa fa-folder-o"></span> Add a new Folder
                                </a>
                            </li>
                            <li class="nav-item dropdown" data-bind="visible: (CanManageFolders() || adminMode()) && SelectedFolder()!=null">
                                <a href="#" class="nav-link dropdown-toggle" role="button" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false">
                                    <span class="fa fa-folder"></span> Folder Options
                                </a>
                                <div class="dropdown-menu">
                                    <a class="dropdown-item" href="#" data-bind="click: ManageFolder.editFolder">Edit Selected Folder</a>
                                    <a class="dropdown-item" href="#" data-bind="click: ManageFolder.deleteFolder">Delete Selected Folder</a>
                                </div>
                            </li>
                            <li class="nav-item active">
                                <a href="#" class="nav-link" data-bind="click: function(){ initHeaderDesigner(); }">
                                    <span class="fa fa-arrow-up"></span> Report Header
                                </a>
                            </li>
                        </ul>
                        <form class="form-inline my-5 my-md-0">
                            <div data-bind="with: searchFieldsInReport">
                                <select id="search-report" class="form-control" data-bind="select2: {placeholder: 'Search Report by Name, Description or Data Field...', ajax: { url: url, dataType: 'json', data: query, processResults: processResults }, minimumInputLength: 0, language: language, allowClear: true }, value: selectedOption"></select>
                            </div>
                        </form>
                    </div>
                </nav>
            </div>
            <div class="card-body">
                <div data-bind="visible: designingHeader, with: headerDesigner" style="display: none;" class="overflow-auto">
                    <div class="checkbox">
                        <label>
                            <input type="checkbox" data-bind="checked: UseReportHeader"> Use Report Header
                        </label>
                    </div>
                    <div data-bind="visible: UseReportHeader">
                        <p class="alert alert-info">
                            You can design the common report header below that will be applied to all reports where report headers are turned on.
                        </p>
                        <canvas id="report-header" width="900" height="120" style="border: solid 1px #ccc"></canvas>
                        <div class="form-inline">
                            <div class="form-group">
                                <button class="btn btn-sm" title="Add Text" data-bind="click: addText"><span class="fa fa-font"></span></button>
                                <label class="btn btn-sm" title="Add Image">
                                    <span class="fa fa-image"></span>
                                    <input type="file" accept="image/*" hidden data-bind="event: { change: function() { uploadImage($element.files[0]) } }" />
                                </label>
                                <button class="btn btn-sm" title="Add Line" data-bind="click: addLine"><span class="fa fa-arrows-h"></span></button>
                            </div>
                            <div data-bind="if: selectedObject()">
                                <div class="form-group">
                                    &nbsp;|&nbsp;
                                    <button class="btn btn-sm" data-bind="click: remove" title="Delete"><span class="fa fa-trash"></span></button>
                                    <div data-bind="if: getText()">
                                        <select class="form-control form-control-sm" title="Font Family" data-bind="event: {change: setFontFamily }, value: objectProperties.fontFamily">
                                            <option value="arial" selected>Arial</option>
                                            <option value="helvetica">Helvetica</option>
                                            <option value="myriad pro">Myriad Pro</option>
                                            <option value="delicious">Delicious</option>
                                            <option value="verdana">Verdana</option>
                                            <option value="georgia">Georgia</option>
                                            <option value="courier">Courier</option>
                                            <option value="comic sans ms">Comic Sans MS</option>
                                            <option value="impact">Impact</option>
                                            <option value="monaco">Monaco</option>
                                            <option value="optima">Optima</option>
                                            <option value="hoefler text">Hoefler Text</option>
                                            <option value="plaster">Plaster</option>
                                            <option value="engagement">Engagement</option>
                                        </select>
                                        &nbsp;
                                        <select class="form-control form-control-sm" title="Text Align" data-bind="event: {change: setTextAlign }, value: objectProperties.textAlign">
                                            <option>Left</option>
                                            <option>Center</option>
                                            <option>Right</option>
                                            <option>Justify</option>
                                        </select>
                                        &nbsp;
                                        <input type="color" size="5" class="btn-object-action" title="Font Color" data-bind="event: {change: setFontColor }, value: objectProperties.fontColor">
                                        <button class="btn btn-sm" title="Bold" data-bind="event: {click: setFontBold }, value: objectProperties.fontBold"><span class="fa fa-bold"></span></button>
                                        <button class="btn btn-sm" title="Italic" data-bind="event: {click: setFontItalic }, value: objectProperties.fontItalic"><span class="fa fa-italic"></span></button>
                                        <button class="btn btn-sm" title="Underline" data-bind="event: {click: setFontUnderline }, value: objectProperties.fontUnderline"><span class="fa fa-underline"></span></button>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                    <button class="btn btn-primary" data-bind="click: saveCanvas">Save Changes</button>
                </div>
                <div data-bind="ifnot: designingHeader">
                    <div data-bind="visible: !SelectedFolder() && !searchReports()">
                        <p>Please choose Folders below to view Reports</p>
                        <ul class="folder-list" data-bind="foreach: Folders">
                            <li data-bind="click: function(){ $parent.SelectedFolder($data); }">
                                <span class="fa fa-3x fa-folder" style="color: #ffd800"></span>
                                <span class="desc" data-bind="text: FolderName"></span>
                            </li>
                        </ul>
                    </div>
                    <div data-bind="visible: SelectedFolder() || searchReports()">
                        <div class="clearfix">
                            <p class="pull-left">Please choose a Report from this Folder</p>
                            <div class="pull-right">
                                <a href="#" data-bind="click: function(){ SelectedFolder(null); searchReports(''); $('#search-report').val([]).trigger('change');}">
                                    ..back to Folders List
                                </a>
                            </div>
                        </div>
                        <div class="list-group list-overflow">
                            <div data-bind="if: SelectedFolder()!=null && reportsInFolder().length==0">
                                No Reports Saved in this Folder
                            </div>
                            <div data-bind="if: searchReports() && reportsInSearch().length==0">
                                No Reports found matching your Search
                            </div>
                            <div data-bind="foreach: searchReports() ? reportsInSearch() : reportsInFolder()">
                                <div class="list-group-item">
                                    <div class="row">
                                        <div class="col-sm-7">
                                            <div class="fa fa-2x pull-left" data-bind="css: {'fa-file': reportType=='List', 'fa-th-list': reportType=='Summary', 'fa-bar-chart': reportType=='Bar', 'fa-pie-chart': reportType=='Pie',  'fa-line-chart': reportType=='Line', 'fa-globe': reportType =='Map', 'fa-window-maximize': reportType=='Single', 'fa-random': reportType=='Pivot'}"></div>
                                            <div class="pull-left">
                                                <h4>
                                                    <a data-bind="click: runReport" style="cursor: pointer">
                                                        <span data-bind="highlightedText: { text: reportName, highlight: $parent.searchReports, css: 'highlight' }"></span>
                                                    </a>
                                                </h4>
                                            </div>
                                            <div class="clearfix"></div>
                                            <p data-bind="text: reportDescription"></p>
                                            <p data-bind="if: $parent.searchReports()"><span class="fa fa-folder"></span> <span data-bind="text: folderName"></span></p>
                                            <div data-bind="if: $parent.adminMode">
                                                <div class="small">
                                                    <b>Report Access</b><br />
                                                    Manage by User <span class="badge badge-info" data-bind="text: userId ? userId : 'No User'"></span>
                                                    <br />
                                                    View only by User <span class="badge badge-info" data-bind="text: (viewOnlyUserId ? viewOnlyUserId : (userId ? userId : 'Any User'))"></span>
                                                    <br />
                                                    <div data-bind="if: deleteOnlyUserId">
                                                        Delete only by User <span class="badge badge-info" data-bind="text: deleteOnlyUserId"></span>
                                                        <br />
                                                    </div>
                                                    <div data-bind="if: userRole">
                                                        Manage by Role <span class="badge badge-info" data-bind="text: userRole ? userRole : 'No Role'"></span>
                                                        <br />
                                                    </div>
                                                    <div data-bind="if: viewOnlyUserRole">
                                                        View only by Role <span class="badge badge-info" data-bind="text: viewOnlyUserRole ? viewOnlyUserRole : 'Any Role'"></span>
                                                        <br />
                                                    </div>
                                                    <div data-bind="if: deleteOnlyUserRole">
                                                        Delete only by Role <span class="badge badge-info" data-bind="text: deleteOnlyUserRole ? deleteOnlyUserRole : 'Same as Manage'"></span>
                                                        <br />
                                                    </div>
                                                    <div data-bind="if: clientId">
                                                        For Client <span class="label label-info" data-bind="text: clientId ? clientId : 'Any'"></span>
                                                        <br />
                                                    </div>
                                                </div>
                                            </div>
                                        </div>
                                        <div class="col-sm-5 right-align">
                                            <div class="d-none d-md-block">
                                                <br />
                                                <span data-bind="if: $root.CanSaveReports() || $root.adminMode() ">
                                                    <button class="btn btn-sm btn-secondary" data-bind="click: openReport, visible: canEdit" data-toggle="modal" data-target="#modal-reportbuilder">
                                                        <span class="fa fa-edit" aria-hidden="true"></span>Edit
                                                    </button>
                                                    <button class="btn btn-sm btn-secondary" data-bind="click: copyReport" data-toggle="modal" data-target="#modal-reportbuilder">
                                                        <span class="fa fa-copy" aria-hidden="true"></span>Copy
                                                    </button>
                                                </span>
                                                <button class="btn btn-sm btn-primary" data-bind="click: runReport">
                                                    <span class="fa fa-gears" aria-hidden="true"></span>Run
                                                </button>
                                                <button class="btn btn-sm btn-danger" data-bind="click: deleteReport, visible: canDelete">
                                                    <span class="fa fa-trash" aria-hidden="true"></span>Delete
                                                </button>
                                                <br />
                                            </div>

                                            <div class="d-block d-md-none">
                                                <span data-bind="if: $root.CanSaveReports() || $root.adminMode()">
                                                    <button class="btn btn-sm btn-secondary" title="Edit Report" data-bind="click: openReport, visible: canEdit" data-toggle="modal" data-target="#modal-reportbuilder">
                                                        <span class="fa fa-edit" aria-hidden="true"></span>
                                                    </button>
                                                    <button class="btn btn-sm btn-secondary" data-bind="click: copyReport" title="Copy Report" data-toggle="modal" data-target="#modal-reportbuilder">
                                                        <span class="fa fa-copy" aria-hidden="true"></span>
                                                    </button>
                                                </span>
                                                <button class="btn btn-sm btn-primary" data-bind="click: runReport" title="Run Report">
                                                    <span class="fa fa-gears" aria-hidden="true"></span>
                                                </button>
                                                <button class="btn btn-sm btn-danger" title="Delete Report" data-bind="click: deleteReport, visible: canDelete">
                                                    <span class="fa fa-trash" aria-hidden="true"></span>
                                                </button>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>

    <!-- Report Builder -->
    <div class="modal modal-fullscreen" id="modal-reportbuilder" role="dialog" aria-labelledby="myModalLabel" aria-hidden="true" style="padding-right: 0px !important;">
        <div data-bind="template: {name: 'report-designer', data: $data}"></div>
    </div>

    <!-- Field Options Modal -->
    <div class="modal" id="fieldOptionsModal" tabindex="-1" role="dialog" aria-hidden="true">
        <div data-bind="template: {name: 'report-field-options', data: $data}"></div>
    </div>

    <!-- Link Edit Modal -->
    <div class="modal" id="linkModal" tabindex="-1" role="dialog" aria-hidden="true">
        <div data-bind="template: {name: 'report-link-edit', data: $data}"></div>
    </div>

    <!-- Folder Edit Modal -->
    <div class="modal" id="folderModal" tabindex="-1" role="dialog" aria-hidden="true">
        <div class="modal-dialog">
            <div class="modal-content" data-bind="with: ManageFolder">
                <div class="modal-header">
                    <h5 class="modal-title">Manage Folder</h5>
                    <button type="button" class="close" data-dismiss="modal" aria-label="Close">
                        <span aria-hidden="true">&times;</span>
                    </button>
                </div>
                <div class="modal-body">
                    <div class="form-group">
                        <label class="col-sm-4 col-md-4 control-label">Folder Name</label>
                        <div class="col-sm-8 col-md-8">
                            <input type="text" class="form-control" id="folderName" required placeholder="Folder Name" data-bind="value: FolderName">
                        </div>
                    </div>
                    <br />
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-secondary" data-dismiss="modal">Cancel</button>
                    <button type="button" class="btn btn-primary" data-bind="click: saveFolder">Save</button>
                </div>
            </div>
        </div>
    </div>

    <div class="modal" id="noaccountModal" tabindex="-1" role="dialog" aria-hidden="true">
        <div class="modal-dialog">
            <div class="modal-content">
                <div class="modal-header">
                    <button type="button" class="close" data-dismiss="modal" aria-label="Close"><span aria-hidden="true">&times;</span></button>
                    <h4 class="modal-title">Account not Setup</h4>
                </div>
                <div class="modal-body">
                    <p class="alert alert-danger">dotnet Report Account Configuration missing!</p>
                    <p>You do not have the neccessary initial configuration completed to use dotnet Report.</p>
                    <p>Please view the <a href="https://dotnetreport.com/blog/2016/03/getting-started-with-dotnet-report/" target="_blank">Getting Started Guide</a> to correctly configure dotnet Report.</p>
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-default" data-dismiss="modal">Close</button>
                </div>
            </div>
        </div>
    </div>

    <div data-bind="if: ReportMode() == 'execute' || ReportMode() == 'Linked'">

         <div class="card">
            <div class="card-header">
                <nav class="navbar navbar-expand-lg navbar-light bg-light">
                    <a class="navbar-brand" href="#">Viewing Report</a>
                    <button class="navbar-toggler" type="button" data-toggle="collapse" data-target="#navbarSupportedContent" aria-controls="navbarSupportedContent" aria-expanded="false" aria-label="Toggle navigation">
                        <span class="navbar-toggler-icon"></span>
                    </button>
                     <div class="collapse navbar-collapse" >
                        <ul class="navbar-nav mr-auto">

                        </ul>
                        <div class="form-inline my-5 my-md-0">
                            <button data-bind="click: function() {$root.ReportMode('start');}" class="btn btn-primary">
                                Back to Reports
                            </button>
                            <button class="btn btn-primary" data-bind="visible: ReportMode()=='linked'">
                                Back to Parent Report
                            </button>
                            <a href="#" class="btn btn-primary" data-bind="visible: $root.CanEdit()" data-toggle="modal" data-target="#modal-reportbuilder">
                                Edit Report
                            </a>

                            <div class="btn-group">
                                <button type="button" class="btn btn-secondary dropdown-toggle" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false">
                                    <span class="fa fa-download"></span> Export <span class="caret"></span>
                                </button>
                                <ul class="dropdown-menu">
                                    <li class="dropdown-item">
                                        <a href="#" data-bind="click: downloadPdfAlt">
                                            <span class="fa fa-file-pdf-o"></span> Pdf
                                        </a>
                                    </li>
                                    <li class="dropdown-item">
                                        <a href="#" data-bind="click: downloadExcel">
                                            <span class="fa fa-file-excel-o"></span> Excel
                                        </a>
                                    </li>
                                    <li class="dropdown-item">
                                        <a href="#" data-bind="click: downloadCsv">
                                            <span class="fa fa-file-excel-o"></span> Csv
                                        </a>
                                    </li>
                                    <li class="dropdown-item">
                                        <a href="#" data-bind="click: downloadXml">
                                            <span class="fa fa-file-code-o"></span> Xml
                                        </a>
                                    </li>
                                </ul>
                            </div>
                        </div>
                    </div>
                </nav>
            </div>
            <div class="card-body">
                <div data-bind="with: ReportResult" class="report-view">
                    <div data-bind="ifnot: HasError">
                        <div data-bind="with: $root">

                            <div data-bind="if: EditFiltersOnReport">
                                <div class="card">
                                    <div class="card-header">
                                        <h5 class="card-title">
                                            <a data-toggle="collapse" data-target="#filter-panel" href="#">
                                                <i class="fa fa-filter"></i>Choose filter options
                                            </a>
                                        </h5>
                                    </div>
                                    <div id="filter-panel" class="card-body">
                                        <div data-bind="if: useStoredProc">
                                            <div class="row">
                                                <div data-bind="template: {name: 'filter-parameters'}" class="col-md-12"></div>
                                            </div>
                                        </div>
                                        <div data-bind="ifnot: useStoredProc">
                                            <div class="row">
                                                <div data-bind="template: {name: 'filter-group'}" class="col-md-12"></div>
                                            </div>
                                        </div>
                                        <br />
                                        <button class="btn btn-primary" data-bind="click: SaveFilterAndRunReport">Update Filters</button>
                                    </div>
                                </div>
                                <br />
                            </div>
                            <div data-bind="ifnot: EditFiltersOnReport">
                                <div data-bind="template: {name: 'fly-filter-template'}"></div>
                                <br />
                            </div>
                            <div data-bind="if: canDrilldown">
                                <button class="btn btn-secondary btn-xs" data-bind="click: ExpandAll">Expand All</button>
                                <button class="btn btn-secondary btn-xs" data-bind="click: CollapseAll">Collapse All</button>
                                <br />
                                <br />
                            </div>
                            <div class="report-render" data-bind="css: { 'report-expanded': isExpanded }">
                                <div class="report-menubar">
                                    <div class="col-xs-12 col-centered" data-bind="with: pager">
                                        <div class="form-inline" data-bind="visible: pages()">
                                            <div class="form-group pull-left total-records">
                                                <span data-bind="text: ' Total Records: ' + totalRecords()"></span><br />
                                            </div>
                                            <div class="pull-left">
                                                <button class="btn btn-secondary btn-sm" data-bind="visible: !$root.isChart() || $root.ShowDataWithGraph(), click: $root.downloadExcel" title="Export to Excel">
                                                    <span class="fa fa-file-excel-o"></span>
                                                </button>
                                                <button class="btn btn-secondary btn-sm" data-bind="click: $parent.toggleExpand">
                                                    <span class="fa" data-bind="css: {'fa-expand': !$parent.isExpanded(), 'fa-minus': $parent.isExpanded() }"></span>
                                                </button>
                                            </div>
                                            <div class="form-group pull-right">
                                                <div data-bind="template: 'pager-template', data: $data"></div>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                                <div class="report-canvas">
                                    <div class="report-container">
                                        <div class="report-inner">
                                            <div class="canvas-container">
                                                <canvas id="report-header" width="900" height="120" data-bind="visible: useReportHeader"></canvas>
                                            </div>
                                            <h2 data-bind="text: ReportName"></h2>
                                            <p data-bind="html: ReportDescription">
                                            </p>
                                            <div data-bind="with: ReportResult" class="report-expanded-scroll">
                                                <div data-bind="template: 'report-template', data: $data"></div>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </div>
                            <br />
                            <span>Report ran on: @DateTime.Now.ToShortDateString() @DateTime.Now.ToShortTimeString()</span>
                        </div>
                    </div>
                    <div data-bind="if: HasError">
                        <h2 data-bind="text: $root.ReportName"></h2>
                        <p data-bind="text: $root.ReportDescription"></p>

                        <button data-bind="click: function() {$root.ReportMode('start');}" class="btn btn-primary">
                            Back to Reports
                        </button>

                        <a href="#" class="btn btn-primary" data-bind="visible: $root.CanEdit()">
                            Edit Report
                        </a>
                        <h3>An unexpected error occured while running the Report</h3>
                        <hr />
                        <b>Error Details</b>
                        <div data-bind="text: Exception"></div>
                    </div>
                    <div data-bind="if: ReportDebug() || HasError()">
                        <br />
                        <br />
                        <hr />
                        <code data-bind="html: ReportSql">
                        </code>
                    </div>
                </div>
            </div>
        </div>
    </div>
</div>

</asp:Content>