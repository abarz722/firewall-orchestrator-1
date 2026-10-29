using Bunit;
using FWO.Api.Client;
using FWO.Config.Api;
using FWO.Data;
using FWO.Data.Flow;
using FWO.Data.Workflow;
using FWO.Ui.Pages.Request;
using FWO.Ui.Pages.Settings;
using FWO.Ui.Services;
using FWO.Ui.Shared;
using FWO.Services.Workflow;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System.Reflection;

namespace FWO.Test;

[TestFixture]
internal class UiSharedEditorTest
{
    [Test]
    public async Task OwnerEditor_SavesSelectedOwner()
    {
        await using BunitContext context = CreateContext();
        FwoOwner? saved = null;
        FwoOwner owner = new() { Id = 7, Name = "Owner" };

        IRenderedComponent<OwnerEditor> cut = context.Render<OwnerEditor>(parameters => parameters
            .Add(p => p.Display, true)
            .Add(p => p.SelectedOwner, owner)
            .Add(p => p.Owners, [owner])
            .Add(p => p.Saved, value => saved = value));

        cut.Find("button.btn-primary").Click();

        Assert.That(saved?.Id, Is.EqualTo(7));
    }

    [Test]
    public async Task RuleActionEditor_SavesSelectedAction()
    {
        await using BunitContext context = CreateContext();
        RuleAction? saved = null;
        RuleAction action = new() { Id = 3, Name = "allow" };

        IRenderedComponent<RuleActionEditor> cut = context.Render<RuleActionEditor>(parameters => parameters
            .Add(p => p.Display, true)
            .Add(p => p.SelectedRuleAction, action)
            .Add(p => p.RuleActions, [action])
            .Add(p => p.Saved, value => saved = value));

        cut.Find("button.btn-primary").Click();

        Assert.That(saved?.Id, Is.EqualTo(3));
    }

    [Test]
    public async Task TrackingEditor_SavesSelectedTracking()
    {
        await using BunitContext context = CreateContext();
        Tracking? saved = null;
        Tracking tracking = new() { Id = 4, Name = "log" };

        IRenderedComponent<TrackingEditor> cut = context.Render<TrackingEditor>(parameters => parameters
            .Add(p => p.Display, true)
            .Add(p => p.SelectedTracking, tracking)
            .Add(p => p.Trackings, [tracking])
            .Add(p => p.Saved, value => saved = value));

        cut.Find("button.btn-primary").Click();

        Assert.That(saved?.Id, Is.EqualTo(4));
    }

    [Test]
    public async Task ManagementEditor_SavesSelectedManagement()
    {
        await using BunitContext context = CreateContext();
        Management? saved = null;
        Management management = new() { Id = 5, Name = "management" };

        IRenderedComponent<ManagementEditor> cut = context.Render<ManagementEditor>(parameters => parameters
            .Add(p => p.Display, true)
            .Add(p => p.SelectedManagement, management)
            .Add(p => p.Managements, [management])
            .Add(p => p.Saved, value => saved = value));

        cut.Find("button.btn-primary").Click();

        Assert.That(saved?.Id, Is.EqualTo(5));
    }

    [Test]
    public async Task TextEditor_SavesChangedMultilineText()
    {
        await using BunitContext context = CreateContext();
        string? saved = null;

        IRenderedComponent<TextEditor> cut = context.Render<TextEditor>(parameters => parameters
            .Add(p => p.Display, true)
            .Add(p => p.Text, "before")
            .Add(p => p.TextKey, "reason")
            .Add(p => p.Multiline, true)
            .Add(p => p.Saved, value => saved = value));

        cut.Find("textarea").Change("after");
        cut.Find("button.btn-primary").Click();

        Assert.That(saved, Is.EqualTo("after"));
    }

    [Test]
    public async Task TimeObjectOriginSuffixes_SavesConfiguredValues()
    {
        await using BunitContext context = CreateContext();
        string? saved = null;

        IRenderedComponent<TimeObjectOriginSuffixes> cut = context.Render<TimeObjectOriginSuffixes>(parameters => parameters
            .Add(p => p.Display, true)
            .Add(p => p.ConfigValue, "{\"requester\":\"_old\",\"approver\":\"\"}")
            .Add(p => p.ConfigValueChanged, value => saved = value));

        cut.FindAll("input")[0].Change("_user");
        cut.FindAll("input")[1].Change("_approver");
        cut.Find("button.btn-primary").Click();

        Assert.That(TimeObjectNamingHelper.ParseSuffixes(saved), Is.EqualTo(new Dictionary<string, string>
        {
            ["requester"] = "_user",
            ["approver"] = "_approver"
        }));
    }

    [Test]
    public async Task TargetDatesEditor_RendersTimeObjectSelectorAndDateEditor()
    {
        await using BunitContext context = CreateContext();
        TargetDatesEditorModel model = new()
        {
            BeginDate = new DateTime(2026, 8, 1, 8, 0, 0),
            EndDate = new DateTime(2026, 8, 1, 17, 0, 0),
            FlowTimeObjectId = 12,
            FlowTimeObjectName = "Business hours"
        };

        IRenderedComponent<TargetDatesEditor> cut = context.Render<TargetDatesEditor>(parameters => parameters
            .Add(p => p.Display, true)
            .Add(p => p.Model, model));

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindComponent<TimeObjectSelector>(), Is.Not.Null);
            Assert.That(cut.FindComponent<DisplayTaskTargetDates>(), Is.Not.Null);
        });
    }

    [Test]
    public async Task TargetDatesEditor_DateChangeClearsSelectedObject()
    {
        await using BunitContext context = CreateContext();
        TargetDatesEditorModel model = new()
        {
            BeginDate = new DateTime(2026, 8, 1),
            EndDate = new DateTime(2026, 8, 2),
            FlowTimeObjectId = 12,
            FlowTimeObjectName = "Business hours"
        };
        IRenderedComponent<TargetDatesEditor> cut = context.Render<TargetDatesEditor>(parameters => parameters
            .Add(p => p.Display, true)
            .Add(p => p.Model, model));

        DateTime changed = new(2026, 8, 3);
        await UiRequestWorkflowTest.InvokePrivateTask(cut.Instance, "OnTargetBeginDateChanged", changed);
        TargetDatesEditorModel editingModel = UiRequestWorkflowTest.GetMember<TargetDatesEditorModel>(cut.Instance, "editingModel");

        Assert.Multiple(() =>
        {
            Assert.That(editingModel.BeginDate, Is.EqualTo(changed));
            Assert.That(editingModel.FlowTimeObjectId, Is.Null);
            Assert.That(editingModel.FlowTimeObjectName, Is.Null);
            Assert.That(editingModel.TicketElementId, Is.Null);
        });
    }

    [Test]
    public async Task TargetDatesEditor_SelectingTicketObjectSetsElementIdAndDates()
    {
        await using BunitContext context = CreateContext();
        FlowTimeObject ticketObject = new() { Id = -7, Name = "Ticket window" };
        TargetDatesEditorModel model = new()
        {
            TicketTimeObjects = [ticketObject],
            TicketTimeObjectElementIds = new Dictionary<long, long> { [-7] = 77 }
        };
        IRenderedComponent<TargetDatesEditor> cut = context.Render<TargetDatesEditor>(parameters => parameters
            .Add(p => p.Display, true)
            .Add(p => p.Model, model));

        await UiRequestWorkflowTest.InvokePrivateTask(cut.Instance, "OnSelectedTimeObjectIdChanged", -7L);
        await UiRequestWorkflowTest.InvokePrivateTask(cut.Instance, "OnTimeObjectSelected",
            new FlowTimeObject
            {
                Id = -7,
                StartTime = new DateTime(2026, 8, 1, 8, 0, 0),
                EndTime = new DateTime(2026, 8, 1, 17, 0, 0)
            });
        TargetDatesEditorModel editingModel = UiRequestWorkflowTest.GetMember<TargetDatesEditorModel>(cut.Instance, "editingModel");

        Assert.Multiple(() =>
        {
            Assert.That(editingModel.FlowTimeObjectId, Is.EqualTo(-7));
            Assert.That(editingModel.TicketElementId, Is.EqualTo(77));
            Assert.That(editingModel.BeginDate, Is.EqualTo(new DateTime(2026, 8, 1, 8, 0, 0)));
            Assert.That(editingModel.EndDate, Is.EqualTo(new DateTime(2026, 8, 1, 17, 0, 0)));
        });
    }

    [Test]
    public async Task TargetDatesEditor_SavesSelectedTimeObjectModel()
    {
        await using BunitContext context = CreateContext();
        TargetDatesEditorModel? saved = null;
        IRenderedComponent<TargetDatesEditor> cut = context.Render<TargetDatesEditor>(parameters => parameters
            .Add(p => p.Display, true)
            .Add(p => p.Model, new TargetDatesEditorModel { FlowTimeObjectId = 42 })
            .Add(p => p.Saved, value => saved = value));

        await UiRequestWorkflowTest.InvokePrivateTask(cut.Instance, "Save");

        Assert.That(saved?.FlowTimeObjectId, Is.EqualTo(42));
    }

    [Test]
    public void ApprovalAccessTaskView_GroupsOnlyMatchingBundledTasks()
    {
        SimulatedUserConfig userConfig = new()
        {
            ReqConsiderBundling = true,
            ReqDisplayBundledTasksAsOne = true
        };
        WfReqTask first = BundledAccessTask(1, 7, 2);
        WfReqTask second = BundledAccessTask(2, 7, 2);
        WfReqTask differentState = BundledAccessTask(3, 7, 3);
        WfReqTask unbundled = new() { Id = 4, TaskType = WfTaskType.access.ToString() };
        WfHandler handler = new() { ActTicket = new WfTicket { Tasks = [first, second, differentState, unbundled] } };
        ApprovalAccessTaskView component = new();
        UiRequestWorkflowTest.SetMember(component, nameof(ApprovalAccessTaskView.WfHandler), handler);
        UiRequestWorkflowTest.SetMember(component, "userConfig", userConfig);

        MethodInfo method = UiRequestWorkflowTest.GetPrivateMethod(typeof(ApprovalAccessTaskView), "GetTaskGroups");
        List<List<WfReqTask>> groups = [.. (IEnumerable<List<WfReqTask>>)method.Invoke(component, [])!];

        Assert.Multiple(() =>
        {
            Assert.That(groups, Has.Count.EqualTo(3));
            Assert.That(groups.Single(group => group.Any(task => task.Id == 1)), Has.Count.EqualTo(2));
            Assert.That(groups.Single(group => group.Any(task => task.Id == 3)), Has.Count.EqualTo(1));
        });
    }

    [Test]
    public void ApprovalAccessTaskView_AppliesFlowTimeObjectModelToTask()
    {
        WfReqTask task = new() { Id = 10, StateId = 2, TaskType = WfTaskType.access.ToString() };
        WfHandler handler = new() { ActReqTask = task, ActTicket = new WfTicket { Tasks = [task] } };
        ApprovalAccessTaskView component = new();
        UiRequestWorkflowTest.SetMember(component, nameof(ApprovalAccessTaskView.WfHandler), handler);
        UiRequestWorkflowTest.SetMember(component, "userConfig", new SimulatedUserConfig());
        TargetDatesEditorModel model = new()
        {
            FlowTimeObjectId = 42,
            FlowTimeObjectName = "Business hours",
            BeginDate = new DateTime(2026, 8, 1, 8, 0, 0),
            EndDate = new DateTime(2026, 8, 1, 17, 0, 0)
        };

        MethodInfo method = UiRequestWorkflowTest.GetPrivateMethod(typeof(ApprovalAccessTaskView), "ApplyTimeObjectModel");
        method.Invoke(component, [model]);

        WfReqElement? element = TimeObjectElementHelper.Find(task.Elements);
        Assert.Multiple(() =>
        {
            Assert.That(element?.FlowTimeObjectId, Is.EqualTo(42));
            Assert.That(element?.Name, Is.EqualTo("Business hours"));
            Assert.That(element?.StartTime, Is.EqualTo(model.BeginDate));
            Assert.That(element?.EndTime, Is.EqualTo(model.EndDate));
            Assert.That(task.GetAddInfoValue(AdditionalInfoKeys.TimeObjectOrigin), Is.EqualTo(TimeObjectOrigin.Approver.ToString()));
        });
    }

    private static WfReqTask BundledAccessTask(long id, long bundleId, int stateId)
    {
        WfReqTask task = new() { Id = id, StateId = stateId, TaskType = WfTaskType.access.ToString() };
        task.SetAddInfo(AdditionalInfoKeys.FlowBundleId, bundleId.ToString());
        return task;
    }

    private static BunitContext CreateContext()
    {
        BunitContext context = new();
        context.Services.AddSingleton<UserConfig>(new SimulatedUserConfig());
        context.Services.AddSingleton<DomEventService>();
        context.Services.AddSingleton<ApiConnection>(new UiRequestWorkflowTest.RequestWorkflowApiConn());
        return context;
    }
}
