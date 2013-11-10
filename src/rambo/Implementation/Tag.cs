using rambo.Interfaces;

namespace rambo.Implementation
{
    internal class Tag : ITag
    {
        private class InternalTag
        {
            internal string TagString { get; set; }
            internal long Index { get; set; }
        }

        private InternalTag tagValue;
        private readonly INode node;
        private readonly object locker = new object();

        public Tag(INode node)
        {
            tagValue = new InternalTag
                       {
                           Index = 0,
                           TagString = GetTagString(0, node)
                       };
            this.node = node;
        }

        public bool GreaterThan(ITag t)
        {
            var tag = (Tag) t;

            return (tagValue.Index.CompareTo(tag.tagValue.Index) > 0
                    || tagValue.Index.CompareTo(tag.tagValue.Index) == 0 && node.Id.CompareTo(tag.node.Id) > 0);
        }

        public void Increment()
        {
            lock (locker)
            {
                var newTag = new InternalTag
                             {
                                 Index = tagValue.Index + 1,
                                 TagString = GetTagString(tagValue.Index + 1, node)
                             };
                tagValue = newTag;
            }
        }

        private static string GetTagString(long index, INode node)
        {
            return index.ToString() + "." + node.Id.ToString();
        }

        public string Value
        {
            get { return tagValue.TagString; }
        }
    }
}