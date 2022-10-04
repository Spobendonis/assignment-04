namespace Assignment.Core;

public interface IWorkItemRepository
{
    (Response Response, int ItemId) Create(WorkItemCreateDTO item);
    WorkItemDetailsDTO? Find(int itemId);
    IReadOnlyCollection<WorkItemDTO> Read();
    IReadOnlyCollection<WorkItemDTO> ReadRemoved();
    IReadOnlyCollection<WorkItemDTO> ReadByTag(string tag);
    IReadOnlyCollection<WorkItemDTO> ReadByUser(int userId);
    IReadOnlyCollection<WorkItemDTO> ReadByState(State state);
    Response Update(WorkItemUpdateDTO item);
    Response Delete(int itemId);
}
