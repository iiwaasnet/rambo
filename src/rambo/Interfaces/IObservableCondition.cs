namespace rambo.Interfaces
{
    public interface IObservableCondition : IChangeNotifiable
    {
        bool Evaluate();       
    }
}