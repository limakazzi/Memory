namespace Memory.Models
{
    internal class MatrixCell
    {
        public string UndiscoveredValue { get; set; }
        public string DiscoveredValue { get; set; }

        public bool IsHeader { get; set; }
        public string HeaderName { get; set; }

        public string GetName(bool isDiscovering)
        {
            if (IsHeader && !isDiscovering)
                return HeaderName;
            else if (!IsHeader && isDiscovering)
                return DiscoveredValue;
            else
                return UndiscoveredValue;
        }
    }
}