﻿using FWO.Api.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FWO.ApiClient;
using FWO.Report.Filter;
using FWO.ApiClient.Queries;
using FWO.Ui.Display;

namespace FWO.Report
{
    public class ReportChanges : ReportBase
    {
        public ReportChanges(DynGraphqlQuery query) : base(query) { }

        public override async Task GetObjectsInReport(int objectsPerFetch, APIConnection apiConnection, Func<Management[], Task> callback)
        {
            await callback(Managements);
            // currently no further objects to be fetched
            GotObjectsInReport = true;
        }

        public override Task GetObjectsForManagementInReport(Dictionary<string, object> objQueryVariables, byte objects, APIConnection apiConnection, Func<Management[], Task> callback)
        {
            throw new NotImplementedException();
        }


        public override async Task Generate(int changesPerFetch, APIConnection apiConnection, Func<Management[], Task> callback)
        {
            Query.QueryVariables["limit"] = changesPerFetch;
            Query.QueryVariables["offset"] = 0;
            bool gotNewObjects = true;
            Managements = Array.Empty<Management>();

            // save selected device state
            Management[] tempDeviceFilter = await apiConnection.SendQueryAsync<Management[]>(DeviceQueries.getDevicesByManagements);
            DeviceFilter.syncFilterLineToLSBFilter(Query.RawFilter, tempDeviceFilter);

            Managements = await apiConnection.SendQueryAsync<Management[]>(Query.FullQuery, Query.QueryVariables);
            while (gotNewObjects)
            {
                Query.QueryVariables["offset"] = (int)Query.QueryVariables["offset"] + changesPerFetch;
                gotNewObjects = Managements.Merge(await apiConnection.SendQueryAsync<Management[]>(Query.FullQuery, Query.QueryVariables));
                await callback(Managements);
            }
            DeviceFilter.restoreSelectedState(tempDeviceFilter, Managements);
        }

        public override string ExportToCsv()
        {
            StringBuilder csvBuilder = new StringBuilder();

            foreach (Management management in Managements.Where(mgt => !mgt.Ignore))
            {
                //foreach (var item in collection)
                //{

                //}
            }

            throw new NotImplementedException();
        }

        private const int ColumnCount = 13;

        public override string ExportToHtml()
        {
            StringBuilder report = new StringBuilder();
            RuleChangeDisplay ruleChangeDisplay = new RuleChangeDisplay(null); // TODO: Add real UserConfig     

            foreach (Management management in Managements.Where(mgt => !mgt.Ignore))
            {
                report.AppendLine($"<h3>{management.Name}</h3>");
                report.AppendLine("<hr>");

                foreach (Device device in management.Devices)
                {
                    report.AppendLine($"<h4>{device.Name}</h4>");
                    report.AppendLine("<hr>");

                    report.AppendLine("<table>");
                    report.AppendLine("<tr>");
                    report.AppendLine("<th>Change Time</th>");
                    report.AppendLine("<th>Change Type</th>");
                    report.AppendLine("<th>Name</th>");
                    report.AppendLine("<th>Source Zone</th>");
                    report.AppendLine("<th>Source</th>");
                    report.AppendLine("<th>Destination Zone</th>");
                    report.AppendLine("<th>Destination</th>");
                    report.AppendLine("<th>Services</th>");
                    report.AppendLine("<th>Action</th>");
                    report.AppendLine("<th>Track</th>");
                    report.AppendLine("<th>Enabled</th>");
                    report.AppendLine("<th>UID</th>");
                    report.AppendLine("<th>Comment</th>");
                    report.AppendLine("</tr>");

                    if (device.RuleChanges.Length > 0)
                    {
                        foreach (RuleChange ruleChange in device.RuleChanges)
                        {
                            report.AppendLine("<tr>");
                            report.AppendLine($"<td>{ruleChangeDisplay.DisplayChangeTime(ruleChange)}</td>");
                            report.AppendLine($"<td>{ruleChangeDisplay.DisplayChangeAction(ruleChange)}</td>");
                            report.AppendLine($"<td>{ruleChangeDisplay.DisplayName(ruleChange)}</td>");
                            report.AppendLine($"<td>{ruleChangeDisplay.DisplaySourceZone(ruleChange)}</td>");
                            report.AppendLine($"<td>{ruleChangeDisplay.DisplaySource(ruleChange)}</td>");
                            report.AppendLine($"<td>{ruleChangeDisplay.DisplayDestinationZone(ruleChange)}</td>");
                            report.AppendLine($"<td>{ruleChangeDisplay.DisplayDestination(ruleChange)}</td>");
                            report.AppendLine($"<td>{ruleChangeDisplay.DisplayService(ruleChange)}</td>");
                            report.AppendLine($"<td>{ruleChangeDisplay.DisplayAction(ruleChange)}</td>");
                            report.AppendLine($"<td>{ruleChangeDisplay.DisplayTrack(ruleChange)}</td>");
                            report.AppendLine($"<td>{ruleChangeDisplay.DisplayEnabled(ruleChange, export: true)}</td>");
                            report.AppendLine($"<td>{ruleChangeDisplay.DisplayUid(ruleChange)}</td>");
                            report.AppendLine($"<td>{ruleChangeDisplay.DisplayComment(ruleChange)}</td>");
                            report.AppendLine("</tr>");
                        }
                    }
                    else
                    {
                        report.AppendLine("<tr>");
                        report.AppendLine($"<td colspan=\"{ColumnCount}\">No changes found!</td>");
                        report.AppendLine("</tr>");
                    }

                    report.AppendLine("</table>");
                }
            }

            return GenerateHtmlFrame(title: "Changes Report", Query.RawFilter, DateTime.Now, report);
        }
    }
}
