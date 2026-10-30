using FWO.Data;
using FWO.Data.Workflow;
using FWO.Ui.Pages.Request;

namespace FWO.Ui.Services
{
    public sealed class ApprovalTaskViewContext
    {
        public ApprovalTaskDisplayLogic DisplayLogic { get; }
        public AccessTaskFieldVisibilityConfig Visibility { get; }
        public List<IpProtocol> DisplayIpProtos { get; }
        public Func<IEnumerable<WfReqTask>, string, bool> CanEdit { get; }
        public Func<IEnumerable<WfReqTask>, bool> CanEditTimeObject { get; }
        public Func<IEnumerable<WfReqTask>, string> EditableTimeObjectField { get; }
        public Func<WfReqTask, Task> EditOwner { get; }
        public Func<WfReqTask, Task> EditRuleAction { get; }
        public Func<WfReqTask, Task> EditTracking { get; }
        public Func<WfReqTask, Task> EditManagement { get; }
        public Func<WfReqTask, string, Task> EditField { get; }
        public Func<WfReqTask, string, string, Task> EditText { get; }
        public Func<WfReqTask, string, Task> EditTargetDates { get; }

        public ApprovalTaskViewContext(
            ApprovalTaskDisplayLogic displayLogic,
            AccessTaskFieldVisibilityConfig visibility,
            List<IpProtocol> displayIpProtos,
            Func<IEnumerable<WfReqTask>, string, bool> canEdit,
            Func<IEnumerable<WfReqTask>, bool> canEditTimeObject,
            Func<IEnumerable<WfReqTask>, string> editableTimeObjectField,
            Func<WfReqTask, Task> editOwner,
            Func<WfReqTask, Task> editRuleAction,
            Func<WfReqTask, Task> editTracking,
            Func<WfReqTask, Task> editManagement,
            Func<WfReqTask, string, Task> editField,
            Func<WfReqTask, string, string, Task> editText,
            Func<WfReqTask, string, Task> editTargetDates)
        {
            DisplayLogic = displayLogic;
            Visibility = visibility;
            DisplayIpProtos = displayIpProtos;
            CanEdit = canEdit;
            CanEditTimeObject = canEditTimeObject;
            EditableTimeObjectField = editableTimeObjectField;
            EditOwner = editOwner;
            EditRuleAction = editRuleAction;
            EditTracking = editTracking;
            EditManagement = editManagement;
            EditField = editField;
            EditText = editText;
            EditTargetDates = editTargetDates;
        }
    }
}
