public interface ITickable {
    void DoTick(int tick);
    void RollbackTick(int tick);
}
