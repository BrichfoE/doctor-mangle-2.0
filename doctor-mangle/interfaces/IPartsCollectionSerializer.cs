namespace doctor_mangle.interfaces
{
    public interface IPartsCollectionSerializer
    {
        void MovePartsForSerilaization<T>(T objectWithCollection);

        void MovePartsAfterDeserilaization<T>(T objectWithCollection);
    }
}
