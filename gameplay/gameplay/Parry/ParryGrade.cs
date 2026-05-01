namespace Game.Gameplay.Parry
{
    public enum ParryGrade : byte
    {
        PerfectParry = 0,
        NormalParry = 1,
        FailedParry = 2,
    }

    public enum ParryFailureReason : byte
    {
        None = 0,
        NoInput = 1,
        OutOfWindow = 2,
    }

    public enum EchoIntent : byte
    {
        None = 0,
        High = 1,
    }
}
