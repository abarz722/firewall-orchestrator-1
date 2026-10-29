namespace FWO.Data.Workflow
{
    /// <summary>Provides the canonical time-object element of a workflow task.</summary>
    public static class TimeObjectElementHelper
    {
        public static WfReqElement? Find(IEnumerable<WfReqElement> elements)
        {
            return elements.FirstOrDefault(element => element.Field == ElemFieldType.time.ToString());
        }

        public static WfReqElement GetOrCreate(WfReqTask task)
        {
            WfReqElement? element = Find(task.Elements);
            if (element != null)
            {
                return element;
            }

            element = new WfReqElement
            {
                TaskId = task.Id,
                Field = ElemFieldType.time.ToString(),
                RequestAction = RequestAction.create.ToString()
            };
            task.Elements.Add(element);
            return element;
        }

        public static long? GetSelectorId(WfReqElement? element)
        {
            if (element == null)
            {
                return null;
            }
            return element.FlowTimeObjectId ?? element.TimeObjId;
        }

        public static void SetReference(WfReqElement element, long selectorId)
        {
            if (selectorId == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(selectorId), "A time-object reference must not be zero.");
            }

            element.FlowTimeObjectId = selectorId > 0 ? selectorId : null;
            // Negative IDs are UI selectors for ticket-local time elements.
            // They must not be written to time_obj_id because that column is
            // a foreign key to the persistent time_object table.
            element.TimeObjId = null;
            element.TimeObjectTaskId = null;
        }

        public static void SetTicketTaskReference(WfReqElement element, long taskId)
        {
            if (taskId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(taskId), "A ticket time-object task reference must be positive.");
            }

            element.FlowTimeObjectId = null;
            element.TimeObjId = null;
            element.TimeObjectTaskId = taskId;
        }

        public static void ClearReference(WfReqElement element)
        {
            element.FlowTimeObjectId = null;
            element.TimeObjId = null;
            element.TimeObjectTaskId = null;
        }
    }
}
