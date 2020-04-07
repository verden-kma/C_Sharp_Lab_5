namespace Lab_5.Models
{
    internal struct ProcessData
    {
        internal string Name { get; }
        internal bool IsActive { get; }
        internal string UserName { get; }
        internal string FilePath { get; }
        internal string FileName { get; }

        internal ProcessData(string name, bool isActive, string userName, string filePath, string fileName)
        {
            Name = name;
            IsActive = isActive;
            UserName = userName;
            FilePath = filePath;
            FileName = fileName;
        }
    }
}