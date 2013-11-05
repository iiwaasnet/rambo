namespace rambo.Interfaces
{
    public delegate void ChangedEventHandler();

    public interface IChangeNotifiable
    {
        event ChangedEventHandler Changed;
    }
}