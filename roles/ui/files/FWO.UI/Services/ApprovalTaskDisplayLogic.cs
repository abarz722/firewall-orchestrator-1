using FWO.Basics;
using FWO.Config.Api;
using FWO.Data;
using FWO.Data.Flow;
using FWO.Data.Workflow;
using FWO.Services.Workflow;
using FWO.Ui.Display;

namespace FWO.Ui.Services
{
    public enum ApprovalTaskElementDisplayMode
    {
        Resolved,
        Technical,
        UnresolvedGroups
    }

    /// <summary>
    /// Presentation-only logic for the task bodies shown by the approval task view.
    /// Workflow state changes remain in ApprovalTaskDetailsView.
    /// </summary>
    public sealed class ApprovalTaskDisplayLogic
    {
        private readonly WfHandler wfHandler;
        private readonly UserConfig userConfig;
        private readonly List<RuleAction> ruleActions;
        private readonly List<Tracking> trackings;
        private readonly List<IpProtocol> ipProtocols;
        private readonly List<ComplianceNetworkZone> networkZones;
        private readonly ApprovalTaskElementDisplayMode displayMode;

        public ApprovalTaskDisplayLogic(WfHandler wfHandler, UserConfig userConfig,
            List<RuleAction> ruleActions, List<Tracking> trackings, List<IpProtocol> ipProtocols,
            List<ComplianceNetworkZone> networkZones, bool technicalRuleView)
            : this(wfHandler, userConfig, ruleActions, trackings, ipProtocols, networkZones,
                technicalRuleView ? ApprovalTaskElementDisplayMode.Technical : ApprovalTaskElementDisplayMode.Resolved)
        {
        }

        public ApprovalTaskDisplayLogic(WfHandler wfHandler, UserConfig userConfig,
            List<RuleAction> ruleActions, List<Tracking> trackings, List<IpProtocol> ipProtocols,
            List<ComplianceNetworkZone> networkZones, ApprovalTaskElementDisplayMode displayMode)
        {
            this.wfHandler = wfHandler;
            this.userConfig = userConfig;
            this.ruleActions = ruleActions;
            this.trackings = trackings;
            this.ipProtocols = ipProtocols;
            this.networkZones = networkZones;
            this.displayMode = displayMode;
        }

        private FlowIntegrationConfig FlowIntegration => FlowIntegrationConfig.Parse(userConfig.ReqFlowIntegration);

        private IReadOnlyDictionary<string, string> TimeObjectOriginSuffixes =>
            TimeObjectNamingHelper.ParseSuffixes(userConfig.ReqTimeObjectOriginSuffixes);

        public string Handlers(IEnumerable<WfReqTask> tasks) => string.Join(", ", tasks
            .Select(task => task.CurrentHandler?.Name)
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Distinct());

        public string AssignedGroups(IEnumerable<WfReqTask> tasks) => string.Join(", ", tasks
            .Select(task => task.AssignedGroup)
            .Where(group => !string.IsNullOrWhiteSpace(group))
            .Distinct());

        public string ManagementIds(IEnumerable<WfReqTask> tasks) => string.Join(", ", tasks
            .Select(task => task.ManagementId?.ToString())
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Distinct());

        public string Owners(IEnumerable<WfReqTask> tasks) => string.Join(", ", tasks
            .SelectMany(task => task.Owners)
            .Select(owner => owner.Owner?.Name)
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Distinct());

        public string DistinctValues(IEnumerable<WfReqTask> tasks, Func<WfReqTask, string?> selector) => string.Join("; ", tasks
            .Select(selector)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct());

        public string TimeObjectDisplay(IEnumerable<WfReqTask> tasks, AccessTaskFieldVisibilityConfig visibility)
        {
            string name = string.Join("; ", tasks
                .Select(GetTimeObjectName)
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Distinct());
            string begin = visibility.ShowValidFrom ? Dates(tasks, false) : "";
            string end = visibility.ShowValidTo ? Dates(tasks, true) : "";
            string validity = string.IsNullOrWhiteSpace(begin) ? end
                : string.IsNullOrWhiteSpace(end) ? begin
                : $"{begin} - {end}";
            return string.IsNullOrWhiteSpace(validity) ? name
                : string.IsNullOrWhiteSpace(name) ? $"({validity})"
                : $"{name} ({validity})";
        }

        public string RuleActions(IEnumerable<WfReqTask> tasks) => string.Join("; ", tasks
            .Select(task => ruleActions.FirstOrDefault(action => action.Id == task.RuleAction)?.Name ?? task.RuleAction?.ToString())
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct());

        public string Trackings(IEnumerable<WfReqTask> tasks) => string.Join("; ", tasks
            .Select(task => trackings.FirstOrDefault(tracking => tracking.Id == task.Tracking)?.Name ?? task.Tracking?.ToString())
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct());

        public IEnumerable<string> RuleElementValues(IEnumerable<WfReqTask> tasks, ElemFieldType field)
        {
            return displayMode switch
            {
                ApprovalTaskElementDisplayMode.Technical => TechnicalElementValues(tasks, field),
                ApprovalTaskElementDisplayMode.UnresolvedGroups => UnresolvedGroupElementValues(tasks, field),
                _ => ResolvedElementValues(tasks, field)
            };
        }

        private string Dates(IEnumerable<WfReqTask> tasks, bool isEndDate) => string.Join("; ", tasks
            .Select(task => EffectiveTimeObjectDate(task, isEndDate))
            .OfType<DateTime>()
            .Select(date => FormatDate(date, isEndDate))
            .Distinct());

        private DateTime? EffectiveTimeObjectDate(WfReqTask task, bool isEndDate)
        {
            WfReqElement? timeElement = TimeObjectReferenceResolver.FindParallelTimeElement(
                wfHandler.ActTicket.Tasks, task, null, null);
            timeElement ??= TimeObjectElementHelper.Find(task.Elements);
            if (timeElement != null)
            {
                DateTime? elementDate = isEndDate ? timeElement.EndTime : timeElement.StartTime;
                if (elementDate.HasValue)
                {
                    return elementDate;
                }
            }

            return isEndDate ? task.TargetEndDate : task.TargetBeginDate;
        }

        private string GetTimeObjectName(WfReqTask task)
        {
            WfReqElement? timeElement = TimeObjectElementHelper.Find(task.Elements);
            WfReqElement? effectiveTimeElement = TimeObjectReferenceResolver.FindParallelTimeElement(
                wfHandler.ActTicket.Tasks, task, null, null) ?? timeElement;
            string name = effectiveTimeElement?.Name ?? "";
            TimeObjectOrigin? origin = ParseTimeObjectOrigin(task.GetAddInfoValue(AdditionalInfoKeys.TimeObjectOrigin));
            if (string.IsNullOrWhiteSpace(name))
            {
                long? timeObjectId = TimeObjectElementHelper.GetSelectorId(timeElement);
                WfReqElement? parallelTimeElement = TimeObjectReferenceResolver.FindParallelTimeElement(
                    wfHandler.ActTicket.Tasks, task, timeObjectId, null);
                name = parallelTimeElement?.Name ?? TimeObjectNamingHelper.BuildDateBasedName(
                    task.TargetBeginDate, task.TargetEndDate, FlowIntegration.TimeObjectPrecision);
                origin ??= parallelTimeElement == null ? null : TimeObjectOrigin.Requester;
            }

            return TimeObjectNamingHelper.BuildDisplayName(name, origin, TimeObjectOriginSuffixes);
        }

        private IEnumerable<string> ResolvedElementValues(IEnumerable<WfReqTask> tasks, ElemFieldType field) => tasks
            .SelectMany(task => task.Elements)
            .Where(element => element.Field == field.ToString())
            .SelectMany(element => ExpandTicketGroup(element, field))
            .Select(ResolvedElementValue)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct();

        private IEnumerable<string> TechnicalElementValues(IEnumerable<WfReqTask> tasks, ElemFieldType field) => tasks
            .SelectMany(task => task.Elements)
            .Where(element => element.Field == field.ToString())
            .SelectMany(element => ExpandTicketGroup(element, field))
            .Select(TechnicalElementValue)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct();

        private IEnumerable<string> UnresolvedGroupElementValues(IEnumerable<WfReqTask> tasks, ElemFieldType field) => tasks
            .SelectMany(task => task.Elements)
            .Where(element => element.Field == field.ToString())
            .Select(UnresolvedGroupElementValue)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct();

        private string UnresolvedGroupElementValue(WfReqElement element)
        {
            bool isNetworkGroup = element.FlowNetworkGroupId.HasValue;
            bool isServiceGroup = element.FlowServiceGroupId.HasValue;
            if (!string.IsNullOrWhiteSpace(element.GroupName) || isNetworkGroup || isServiceGroup)
            {
                string name = element.GroupName ?? element.Name
                    ?? (isNetworkGroup ? element.FlowNetworkGroupId?.ToString() : element.FlowServiceGroupId?.ToString())
                    ?? "";
                return AddZones(element, name);
            }

            return ResolvedElementValue(element);
        }

        private IEnumerable<WfReqElement> ExpandTicketGroup(WfReqElement element, ElemFieldType field)
        {
            string groupName = !string.IsNullOrWhiteSpace(element.GroupName) ? element.GroupName : element.Name ?? "";
            string memberField = field == ElemFieldType.service
                ? ElemFieldType.service.ToString()
                : ElemFieldType.source.ToString();

            List<WfReqTask> groupTasks = [.. wfHandler.ActTicket.Tasks
                .Where(task => task.TaskType is nameof(WfTaskType.group_create) or nameof(WfTaskType.group_modify))
                .Where(task => MatchesTicketGroup(task, element, groupName, memberField))];

            List<WfReqElement> members = [.. groupTasks
                .SelectMany(task => task.Elements)
                .Where(member => member.Field == memberField)
                .Where(member => !string.Equals(member.Name, groupName, StringComparison.OrdinalIgnoreCase)
                    || member.IpString != null || member.Port.HasValue || member.ProtoId.HasValue)];

            return members.Count > 0 ? members : [element];
        }

        private static bool MatchesTicketGroup(WfReqTask task, WfReqElement element, string groupName, string memberField)
        {
            if (!string.IsNullOrWhiteSpace(groupName)
                && string.Equals(task.GetAddInfoValue(AdditionalInfoKeys.GrpName).Trim(), groupName.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return task.Elements.Any(candidate => candidate.Field == memberField
                && ((!string.IsNullOrWhiteSpace(groupName)
                        && (string.Equals(candidate.GroupName?.Trim(), groupName.Trim(), StringComparison.OrdinalIgnoreCase)
                            || string.Equals(candidate.Name?.Trim(), groupName.Trim(), StringComparison.OrdinalIgnoreCase)))
                    || element.FlowNetworkGroupId.HasValue && candidate.FlowNetworkGroupId == element.FlowNetworkGroupId
                    || element.FlowServiceGroupId.HasValue && candidate.FlowServiceGroupId == element.FlowServiceGroupId));
        }

        private string ResolvedElementValue(WfReqElement element)
        {
            string value = element.Field == ElemFieldType.service.ToString()
                ? ServiceValue(element)
                : ObjectValue(element);
            return AddZones(element, value);
        }

        private string TechnicalElementValue(WfReqElement element)
        {
            if (element.Field == ElemFieldType.service.ToString())
            {
                string service = DisplayBase.DisplayService(ToServiceElement(element), ipProtocols);
                return AddZones(element, string.IsNullOrWhiteSpace(service) ? ServiceValue(element) : service);
            }

            string ip = NwObjDisplay.DisplayIp(element.IpString, element.IpEnd);
            return AddZones(element, string.IsNullOrWhiteSpace(ip) ? ObjectValue(element) : ip);
        }

        private string ObjectValue(WfReqElement element) =>
            DisplayBase.DisplayIpWithName(WfElementBase.ToNetworkObject(element));

        private string ServiceValue(WfReqElement element)
        {
            string name = element.Name ?? "";
            string details = DisplayBase.DisplayService(ToServiceElement(element), ipProtocols);
            return string.IsNullOrWhiteSpace(name) || string.Equals(name, details, StringComparison.OrdinalIgnoreCase)
                ? string.IsNullOrWhiteSpace(name) ? details : name
                : string.IsNullOrWhiteSpace(details) ? name : $"{name} ({details})";
        }

        private static NwServiceElement ToServiceElement(WfReqElement element) => new()
        {
            Port = element.Port,
            PortEnd = element.PortEnd,
            ProtoId = element.ProtoId ?? 0,
            HasProtocol = element.ProtoId.HasValue,
            Name = element.Name
        };

        private string AddZones(WfReqElement element, string value)
        {
            if (element.Field != ElemFieldType.source.ToString() && element.Field != ElemFieldType.destination.ToString())
            {
                return value;
            }

            string zones = Zones(element);
            return string.IsNullOrWhiteSpace(zones) ? value : $"{value} ({userConfig.GetText("zone")}: {zones})";
        }

        private string Zones(WfReqElement element)
        {
            var range = element.GetIpRange();
            if (range == null)
            {
                return "";
            }

            return string.Join(", ", networkZones
                .Where(zone => zone.IPRanges.Any(zoneRange => IpOperations.RangeOverlapExists(zoneRange, range)))
                .Select(zone => zone.Name)
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Distinct());
        }

        private string FormatDate(DateTime date, bool isEndDate)
        {
            if (FlowIntegration.TimeObjectPrecision == FlowIntegrationTimePrecisionOptions.Date)
            {
                if (isEndDate && date.TimeOfDay == TimeSpan.Zero)
                {
                    date = date.AddDays(-1);
                }
                return date.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
            }

            return FlowIntegration.TimeObjectPrecision switch
            {
                FlowIntegrationTimePrecisionOptions.Hours => date.ToString("yyyy-MM-dd H 'h'", System.Globalization.CultureInfo.InvariantCulture),
                FlowIntegrationTimePrecisionOptions.Minutes => date.ToString("yyyy-MM-dd HH:mm", System.Globalization.CultureInfo.InvariantCulture),
                _ => date.ToString("yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture)
            };
        }

        private static TimeObjectOrigin? ParseTimeObjectOrigin(string value) =>
            Enum.TryParse(value, true, out TimeObjectOrigin origin) ? origin : null;
    }
}
