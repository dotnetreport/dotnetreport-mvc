﻿<!DOCTYPE html>
<html>

<head>
    <meta charset="utf-8" />
    <link rel="shortcut icon" href="/favicon.ico">
    <meta http-equiv="X-UA-Compatible" content="IE=edge">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Asp .Net Ad Hoc Report Builder - @ViewBag.Title </title>
    <meta name="keywords" content="ad-hoc reporting, reporting, asp .net reporting, asp .net report, report builder, ad hoc report builder, ad-hoc report builder, adhoc report, ad hoc reports, .net report viewer, reportviewer, sql reportviewer, report builder mvc, report mvc, report builder web forms, query builder, sql report builder,visual report builder,custom query,query maker" />
    <meta name="description" content="Ad hoc Reporting software that allows programmers to easily add Reporting functionality to their ASP .NET Web Software Solution" />
    <link href="../Content/bootstrap.min.css" rel="stylesheet" />
    <link href="../Content/bootstrap-mvc-validation.css" rel="stylesheet" />
    <link href="../Content/themes/base/datepicker.css" rel="stylesheet" />
    <link href="../Content/themes/base/theme.css" rel="stylesheet" />
    <link href="../Content/toastr.min.css" rel="stylesheet" />
    <link href="../Content/font-awesome.min.css" rel="stylesheet" />
    <link href="../Content/css/select2.min.css" rel="stylesheet" />
    <link href="../Content/dotnetreport.css?v=4.2.0" rel="stylesheet" />
    <script type="text/javascript" src="https://www.gstatic.com/charts/loader.js"></script>
    <style type="text/css">
        .report-view {
            margin: 0 50px 0 50px;
            max-width: inherit;
        }
    </style>
</head>

<body>
    
    <form id="form1" runat="server">
    <div data-bind="with: ReportResult">

        <!-- ko ifnot: HasError -->

        <div class="report-view" data-bind="with: $root">
            <div class="report-inner" style="display: none;">
                <canvas id="report-header" width="1100" height="120" data-bind="visible: useReportHeader"></canvas>
                <h2 data-bind="text: ReportName"></h2>
                <p data-bind="html: ReportDescription">
                </p>

                <div data-bind="with: $root">
                    <div class="" data-bind="ifnot: EditFiltersOnReport">
                        <div data-bind="template: {name: 'fly-filter-template'}"></div>
                    </div>
                </div>

                <div data-bind="with: ReportResult">
                    <div data-bind="template: 'report-template', data: $data"></div>
                </div>
            </div>
            <br />
            <span>Report ran on: @DateTime.Now.ToShortDateString() @DateTime.Now.ToShortTimeString()</span>
        </div>
        <!-- /ko -->
        <!-- ko if: HasError -->
        <h2>@Model.ReportName</h2>
        <p>
            @Model.ReportDescription
        </p>

        <h3>An unexpected error occured while running the Report</h3>
        <hr />
        <b>Error Details</b>
        <p>
            <div data-bind="text: Exception"></div>
        </p>

        <!-- /ko -->

    </div>

    <script type="text/html" id="report-template">

        <div class="report-chart" data-bind="attr: {id: 'chart_div_' + $parent.ReportID()}, visible: $parent.isChart"></div>

        <div class="table-responsive" data-bind="with: ReportData">
            <table class="table table-hover table-condensed">
                <thead>
                    <tr class="no-highlight">
                        <!-- ko if: $parentContext.$parent.canDrilldown() && !IsDrillDown() -->
                        <th></th>
                        <!-- /ko -->
                        <!-- ko foreach: Columns -->
                        <th data-bind="attr: { id: 'col' + $index() }, css: {'right-align': IsNumeric}">
                            <a href="" data-bind="click: function(){ $parentContext.$parentContext.$parent.changeSort(SqlField); }">
                                <span data-bind="text: ColumnName"></span>
                                <span data-bind="text: $parentContext.$parentContext.$parent.pager.sortColumn() === SqlField ? ($parentContext.$parentContext.$parent.pager.sortDescending() ? '&#9660;' : '&#9650;') : ''"></span>
                            </a>
                        </th>
                        <!-- /ko -->
                    </tr>
                </thead>
                <tbody>
                    <tr style="display: none;" data-bind="visible: Rows.length < 1">
                        <td class="text-info" data-bind="attr:{colspan: Columns.length}">
                            No records found
                        </td>
                    </tr>
                    <!-- ko foreach: Rows  -->
                    <tr>
                        <!-- ko if: $parentContext.$parentContext.$parent.canDrilldown() && !$parent.IsDrillDown() -->
                        <td>&nbsp;</td>
                        <!-- /ko -->
                        <!-- ko foreach: Items -->
                        <!-- ko if: LinkTo-->
                        <td data-bind="css: {'right-align': Column.IsNumeric}">
                            <a data-bind="attr: {href: LinkTo}"><span data-bind="html: FormattedValue"></span></a>
                        </td>
                        <!-- /ko-->
                        <!-- ko ifnot: LinkTo-->
                        <td data-bind="html: FormattedValue, css: {'right-align': Column.IsNumeric}"></td>
                        <!-- /ko-->
                        <!-- /ko -->
                    </tr>
                    <!-- ko if: isExpanded -->
                    <tr>
                        <td></td>
                        <td data-bind="attr:{colspan: $parent.Columns.length }">
                            <!-- ko if: DrillDownData -->
                            <table class="table table-hover table-condensed" data-bind="with: DrillDownData">
                                <thead>
                                    <tr class="no-highlight">
                                        <!-- ko foreach: Columns -->
                                        <th data-bind="css: {'right-align': IsNumeric}">
                                            <a href="" data-bind="click: function(){ $parents[1].changeSort(SqlField); }">
                                                <span data-bind="text: ColumnName"></span>
                                                <span data-bind="text: $parents[1].pager.sortColumn() === SqlField ? ($parents[1].pager.sortDescending() ? '&#9660;' : '&#9650;') : ''"></span>
                                            </a>
                                        </th>
                                        <!-- /ko -->
                                    </tr>
                                </thead>
                                <tbody>
                                    <tr style="display: none;" data-bind="visible: Rows.length < 1">
                                        <td class="text-info" data-bind="attr:{colspan: Columns.length}">
                                            No records found
                                        </td>
                                    </tr>
                                    <!-- ko foreach: Rows  -->
                                    <tr>
                                        <!-- ko foreach: Items -->
                                        <td data-bind="html: FormattedValue, css: {'right-align': Column.IsNumeric}"></td>
                                        <!-- /ko -->
                                    </tr>
                                    <!-- /ko -->
                                </tbody>
                            </table>
                            <!-- /ko -->
                        </td>
                    </tr>
                    <!-- /ko -->
                    <!-- /ko -->
                </tbody>
                <!-- ko if: $parent.SubTotals().length == 1 -->
                <tfoot data-bind="foreach: $parent.SubTotals">
                    <tr>
                        <!-- ko if: $parentContext.$parentContext.$parent.canDrilldown() && !$parent.IsDrillDown() -->
                        <td></td>
                        <!-- /ko -->
                        <!-- ko foreach: Items -->
                        <td data-bind="html: FormattedValue, css: {'right-align': Column.IsNumeric}"></td>
                        <!-- /ko -->
                    </tr>
                </tfoot>
                <!-- /ko -->
            </table>
        </div>

    </script>

    <script src="../Scripts/jquery-3.6.0.min.js"></script>
    <script src="../Scripts/bootstrap.bundle.min.js"></script>
    <script src="../Scripts/jquery-ui-1.12.1.min.js""></script>
    <script src="../Scripts/jquery.validate.min.js"></script>
    <script src="../Scripts/knockout-3.5.1.js"></script>
    <script src="../Scripts/jquery.blockUI.js"></script>
    <script src="../Scripts/bootbox.min.js"></script>
    <script src="../Scripts/toastr.min.js"></script>
    <script src="../Scripts/knockout-sortable.min.js"></script>
    <script src="../Scripts/select2.min.js"></script>
    <script src="../Scripts/dotnetreport-helper.js"></script>
    <script src="../Scripts/lodash.min.js"></script>
    <script src="../Scripts/fabric.min.js"></script>
    <script src="../Scripts/dotnetreport.js?v=5.0.0"></script>

    <style type="text/css">
        a[href]:after {
            content: none !important;
        }
    </style>
    <script type="text/javascript">

        $(document).ready(function () {
            var data = {
                currentUserId: '<%= @Model.UserId %>',
                currentUserRoles: '<%= @Model.CurrentUserRoles %>',
                dataFilters: '<%= @Model.DataFilters %>',
                clientId: '<%= @Model.ClientId %>'
            };

            function decodeHTMLEntities(text) {
                var parser = new DOMParser();
                var dom = parser.parseFromString('<!doctype html><body>' + text, 'text/html');
                var decodedText = dom.body.textContent;

                // Remove new lines and carriage returns
                return decodedText.replace(/[\n\r]/g, '');
            }

            
            var svc = "/DotNetReport/ReportService.asmx/";
            var vm = new reportViewModel({
                runReportUrl: svc + "Report",
                execReportUrl: svc + "RunReport",
                runLinkReportUrl: svc + "RunReportLink",
                reportWizard: $("#filter-panel"),
                reportHeader: "report-header",
                lookupListUrl: svc + "GetLookupList",
                apiUrl: svc + "CallReportApi",
                runReportApiUrl: svc + "RunReportApi",
                reportFilter: htmlDecode('<%= Model.ReportFilter %>'),
                reportMode: "execute",
                reportSql: "<%= Model.ReportSql %>",
                reportConnect: "<%= Model.ConnectKey %>",
                reportSeries: "<%= Model.ReportSeries %>",
                AllSqlQuries: "<%= Model.ReportSql %>",
                reportHeader: 'report-header',
                userSettings: data,
                dataFilters: data.dataFilters,
                reportData: JSON.parse(decodeHTMLEntities("<%= Model.ReportData %>")),
                chartSize: { width: 1000, height: 450 }
            });
            vm.pager.pageSize(10000);
            ko.applyBindings(vm);
            vm.LoadReport((<%= Model.ReportId %>, true,"<%= Model.ReportSeries %>").done(function () {
                if (vm.useReportHeader()) {
                    vm.headerDesigner.init(true);
                    vm.headerDesigner.loadCanvas(true);
                }

                if (vm.useStoredProc()) {
                    setTimeout(function () {
                        vm.printReport();
                    }, 1000);
                } else {
                    vm.printReport();
                }

                setTimeout(function () {
                    $('.report-inner').show();
                }, 1500);

                setTimeout(function () {
                    $('.report-inner').show();
                }, 15000);
            });

        $(window).resize(function () {
            vm.DrawChart();
            vm.headerDesigner.resizeCanvas();
        });

    });

    </script>

    <script type="text/html" id="fly-filter-template">
        <div data-bind="visible: FlyFilters().length>0" style="padding-left: 30px; padding-right: 30px; padding-top: 20px">
            <div class="card card-body">
                <!-- ko foreach: FlyFilters -->
                <div class="row">
                    <div class="col-sm-5 col-xs-4">
                        <div data-bind="with: Field" >
                            <div data-bind="if: $parent.Apply">
                                <label>
                                    <span data-bind="text: selectedFilterName"></span>
                                </label>
                            </div>
                        </div>
                    </div>
                    <div data-bind="with: Field" class="col-sm-2 col-xs-3">
                        <div class="form-group" data-bind="if: $parent.Apply">
                            <span data-bind="text: $parent.Operator" ></span>
                        </div>
                    </div>
                    <div data-bind="with: Field" class="col-sm-5 col-xs-5">
                        <div data-bind="if: $parent.Apply">
                            <div data-bind="template: 'report-filter', data: $data"></div>
                        </div>
                    </div>
                </div>
                <!-- /ko -->
            </div>
        </div>
    </script>


    <script type="text/html" id="report-filter">
        <div class="form-group">
            <!-- ko if: !hasForeignKey-->
            <!-- ko if: fieldType=='DateTime'-->
            <!-- ko if: ['=','>','<','>=','<=', 'not equal'].indexOf($parent.Operator()) != -1 -->
            <input class="form-control" data-bind="datepicker: $parent.Value" required />
            <!-- /ko -->
            <!-- ko if: ['between'].indexOf($parent.Operator()) != -1 -->
            From
            <input required class="form-control" data-bind="datepicker: $parent.Value" />
            to
            <input data-bind="datepicker: $parent.Value2" class="form-control" required />
            <!-- /ko -->
            <!-- ko if: ['range'].indexOf($parent.Operator()) != -1 -->
            <select data-bind="value: $parent.Value" class="form-control" required>
                <option value=""></option>
                <option>Today</option>
                <option>Today +</option>
                <option>Today -</option>
                <option>Yesterday</option>
                <option>This Week</option>
                <option>Last Week</option>
                <option>This Month</option>
                <option>Last Month</option>
                <option>This Year</option>
                <option>Last Year</option>
                <option>This Month To Date</option>
                <option>This Year To Date</option>
                <option>Last 30 Days</option>
                <optgroup label="Comparison Options">
                    <option>>= Today</option>
                    <option><= Today</option>
                    <option>>= Today +</option>
                    <option><= Today +</option>
                    <option>>= Today -</option>
                    <option><= Today -</option>
                </optgroup>
            </select>
            <div data-bind="if: $parent.Value().indexOf('Today +') >= 0 || $parent.Value().indexOf('Today -') >= 0" class="form-group pull-left" style="padding-top: 5px;">
                <input type="number" class="form-control input-sm pull-left" style="width: 80px;" data-bind="value: $parent.Value2" required /><span style="padding: 5px 5px;" class="pull-left"> days</span>
            </div>
            <!-- /ko -->
            <!-- /ko -->
            <!-- ko if: ['Int','Money','Float','Double'].indexOf(fieldType) != -1 -->
            <!-- ko if: ['=','>','<','>=','<=', 'not equal'].indexOf($parent.Operator()) != -1 && ['is blank', 'is not blank', 'is null', 'is not null'].indexOf($parent.Operator()) == -1 -->
            <input class="form-control" type="number" data-bind="value: $parent.Value, disable: $parent.Operator() == 'is default'" required />
            <!-- /ko -->
            <!-- ko if: ['between'].indexOf($parent.Operator()) != -1 -->
            From
            <input class="form-control" type="number" data-bind="value: $parent.Value" required />
            to
            <input class="form-control" type="number" data-bind="value: $parent.Value2" required />
            <!-- /ko -->
            <!-- /ko -->
            <!-- ko if: fieldType=='Boolean' && ['is blank', 'is not blank', 'is null', 'is not null'].indexOf($parent.Operator()) == -1 -->
            <select required class="form-control" data-bind="value: $parent.Value, disable: $parent.Operator() == 'is default'">
                <option value="1">Yes</option>
                <option value="0">No</option>
            </select>
            <!-- /ko -->
            <!-- ko if: ['Int','Money','Float','Double','Date','DateTime','Boolean'].indexOf(fieldType) == -1 && ['is blank', 'is not blank', 'is null', 'is not null'].indexOf($parent.Operator()) == -1 -->
            <input class="form-control" type="text" data-bind="value: $parent.Value, disable: $parent.Operator() == 'is default'" required />
            <!-- /ko -->
            <!-- /ko -->
            <!-- ko if: hasForeignKey && $parent.Operator() != 'all' -->
            <!-- ko if: hasForeignParentKey && $parent.showParentFilter() -->
            <select multiple class="form-control" data-bind="select2: { placeholder: 'Please Choose', allowClear: true }, options: $parent.ParentList, optionsText: 'text', optionsValue: 'id', selectedOptions: $parent.ParentIn"></select>
            <!-- /ko -->
            <!-- ko if: $parent.Operator()=='='-->
            <select required class="form-control" data-bind="options: $parent.LookupList, optionsText: 'text', optionsValue: 'id', value: $parent.Value, optionsCaption: 'Please Choose'"></select>
            <!-- /ko -->
            <!-- ko if: $parent.Operator()=='in' || $parent.Operator()=='not in'-->
            <select required multiple class="form-control" data-bind="select2: { placeholder: 'Please Choose', allowClear: true }, options: $parent.LookupList, optionsText: 'text', optionsValue: 'id', selectedOptions: $parent.ValueIn"></select>
            <!-- /ko -->
            <!-- /ko -->
        </div>
    </script>
</body>
</html>