public interface IUndoableCommand
{
    bool SecureEnd { get; set; }
    void Execute();
    void EndExecute();
    void UnDo();
}
