namespace Assignment.Infrastructure;

public class WorkItemRepository : IWorkItemRepository
{
    public (Response Response, int ItemId) Create(WorkItemCreateDTO item)
    {
        throw new NotImplementedException();
    }

    public Response Delete(int itemId)
    {
        throw new NotImplementedException();
    }

    public WorkItemDetailsDTO Find(int itemId)
    {
        throw new NotImplementedException();
    }

    public IReadOnlyCollection<WorkItemDTO> Read()
    {
        throw new NotImplementedException();
    }

    public IReadOnlyCollection<WorkItemDTO> ReadByState(State state)
    {
        throw new NotImplementedException();
    }

    public IReadOnlyCollection<WorkItemDTO> ReadByTag(string tag)
    {
        throw new NotImplementedException();
    }

    public IReadOnlyCollection<WorkItemDTO> ReadByUser(int userId)
    {
        throw new NotImplementedException();
    }

    public IReadOnlyCollection<WorkItemDTO> ReadRemoved()
    {
        throw new NotImplementedException();
    }

    public Response Update(WorkItemUpdateDTO item)
    {
        throw new NotImplementedException();
    }
}
