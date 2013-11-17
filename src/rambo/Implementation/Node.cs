using rambo.Interfaces;

namespace rambo.Implementation
{
    public class Node : INode
    {
        public Node(int id)
        {
            Id = id;
        }

        public int Id { get; private set; }

        public bool Equals(INode other)
        {
            return Id == other.Id;
        }
    }
}